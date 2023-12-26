using System.Text;
using BitStreams;
using VYaml.Serialization;

namespace SimpleKaitaiParser;

public struct KaitaiScriptEntry
{
    public string Path;
    public string Type;
    public bool IsTrivialType;
    public string EnumName;
    public int Size;
    public string Encoding;

    public override string ToString ()
    {
        return $"{Path} {Type} {IsTrivialType} {EnumName} {Size} {Encoding}";
    }
}

public struct KaitaiParsedEntry
{
    public string Path;
    public string Type;
    public bool IsTrivialType;
    public string EnumName;
    public string EnumValue;
    public object Value;

    public override string ToString ()
    {
        var value = Value is null ? null :
            Value.GetType() == typeof (byte[]) ? Convert.ToHexString(Value as byte[]) : Value.ToString();
        return $"{Path} {Type} {IsTrivialType} {value} {EnumName} {EnumValue}";
    }
}

public class SimpleKaitaiParser
{
    public const string ByteArrayTypeName = "__byteArray";
    public const string UndefinedEnumEntry = "(undef)";
    public const string EnumEntryValueType = "__ENUM";
    private static Encoding Win1251 = null!;
    private readonly Dictionary<string, Dictionary<ulong, string>> DefinedEnums = new ();
    private readonly List<KaitaiScriptEntry> ScriptTypeOrder = new ();
    private BitStream BitStream;
    private Dictionary<object, object> DefinedTypes = new ();

    public SimpleKaitaiParser ()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Win1251 = Encoding.GetEncoding(1251);
    }

    private static bool IsTrivialType (string inputType)
    {
        return inputType == ByteArrayTypeName || inputType == "str" ||
               (inputType[0] is 'b' or 'u' or 's' or 'f' && inputType[1..].All(char.IsDigit));
    }

    public List<KaitaiScriptEntry> ParseKaitaiScript (string script)
    {
        ScriptTypeOrder.Clear();
        DefinedTypes.Clear();
        DefinedEnums.Clear();
        var parsed = YamlSerializer.Deserialize<Dictionary<string, object>>(Encoding.UTF8.GetBytes(script));
        if (parsed.TryGetValue("types", out var typeDefinitions))
        {
            DefinedTypes = typeDefinitions as Dictionary<object, object>;
        }

        var mainSeq = parsed["seq"] as List<object>;
        foreach (var typeDictBoxed in mainSeq)
        {
            AddTypeEntry(typeDictBoxed, new List<string>());
        }

        if (parsed.TryGetValue("enums", out var enumDefinitions))
        {
            foreach (var enumType in enumDefinitions as Dictionary<object, object>)
            {
                var enumName = enumType.Key as string;
                if (!DefinedEnums.ContainsKey(enumName))
                {
                    DefinedEnums.Add(enumName, new Dictionary<ulong, string>());
                }

                var enumEntries = enumType.Value as Dictionary<object, object>;

                foreach (var entry in enumEntries)
                {
                    DefinedEnums[enumName].Add(Convert.ToUInt64((int) entry.Key), (string) entry.Value);
                }
            }
        }

        return ScriptTypeOrder;
    }

    public List<KaitaiParsedEntry> ParseByteArray (string script, byte[] input)
    {
        ParseKaitaiScript(script);
        BitStream = new BitStream(input);
        var result = new List<KaitaiParsedEntry>();
        foreach (var typeEntry in ScriptTypeOrder)
        {
            var parsedEntry = new KaitaiParsedEntry
            {
                Path = typeEntry.Path,
                Type = typeEntry.Type,
                IsTrivialType = typeEntry.IsTrivialType,
                EnumName = typeEntry.EnumName
            };
            if (typeEntry.IsTrivialType)
            {
                var value = ReadTrivialTypeValue(typeEntry);
                if (value is byte[] valueBytes)
                {
                    Array.Reverse(valueBytes);
                    parsedEntry.Value = valueBytes;
                }
                else
                {
                    parsedEntry.Value = value;
                }

                if (!string.IsNullOrWhiteSpace(typeEntry.EnumName) && typeEntry.Type != ByteArrayTypeName)
                {
                    if (!(value.GetType() == typeof (byte[])))
                    {
                        parsedEntry.EnumValue = UndefinedEnumEntry;
                    }
                    else
                    {
                        var byteValue = value as byte[];
                        var valueCopy = new byte[byteValue.Length];
                        Array.Copy(byteValue, valueCopy, byteValue.Length);
                        parsedEntry.Type = EnumEntryValueType;
                        Array.Reverse(valueCopy);
                        Array.Resize(ref valueCopy, 8);
                        var enumValue = BitConverter.ToUInt64(valueCopy);

                        parsedEntry.EnumValue = DefinedEnums[typeEntry.EnumName].ContainsKey(enumValue)
                            ? DefinedEnums[typeEntry.EnumName][enumValue]
                            : UndefinedEnumEntry;
                    }
                }
            }

            result.Add(parsedEntry);
        }

        return result;
    }

    private object ReadTrivialTypeValue (KaitaiScriptEntry typeEntry)
    {
        if (typeEntry.Type == "str")
        {
            // assuming win1251
            var strBytes = BitStream.ReadBytes(typeEntry.Size, true);
            return Win1251.GetString(strBytes);
        }

        if (typeEntry.Type == ByteArrayTypeName || typeEntry.Size != 0)
        {
            return BitStream.ReadBytes(typeEntry.Size, true);
        }

        var length = long.Parse(typeEntry.Type[1..]);

        if (typeEntry.Type.StartsWith('b'))
        {
            // bits, assuming it's at max a b64
            return BitStream.ReadBytes(length);
        }

        var bytes = BitStream.ReadBytes(length, true);

        if (typeEntry.Type.StartsWith('f'))
        {
            return length == 4 ? BitConverter.ToSingle(bytes) : BitConverter.ToDouble(bytes);
        }

        return bytes;
    }

    private void ParseTypeRecursive (List<string> currentPath, string typeName)
    {
        var path = '/' + string.Join('/', currentPath);
        ScriptTypeOrder.Add(new KaitaiScriptEntry
        {
            Path = path,
            IsTrivialType = false,
            Type = typeName
        });
        var type = (DefinedTypes[typeName] as Dictionary<object, object>)["seq"] as List<object>;
        foreach (var field in type)
        {
            AddTypeEntry(field, currentPath);
        }
    }

    private void AddTypeEntry (object typeDictBoxed, List<string> currentPath)
    {
        var pathString = currentPath.Any() ? '/' + string.Join('/', currentPath) : string.Empty;
        var typeDict = typeDictBoxed as Dictionary<object, object>;
        var typeStr = typeDict.ContainsKey("type") ? typeDict["type"] as string : ByteArrayTypeName;
        var idStr = typeDict["id"] as string;
        var size = typeDict.ContainsKey("size") ? (int) typeDict["size"] : 0;
        var encoding = typeDict.ContainsKey("encoding") ? (string) typeDict["encoding"] : string.Empty;
        var enumName = typeDict.ContainsKey("enum") ? (string) typeDict["enum"] : string.Empty;
        if (IsTrivialType(typeStr))
        {
            ScriptTypeOrder.Add(new KaitaiScriptEntry
            {
                IsTrivialType = true,
                Path = pathString + '/' + idStr,
                Type = typeStr,
                Size = size,
                Encoding = encoding,
                EnumName = enumName
            });
        }
        else
        {
            var newPath = new List<string>(currentPath) { idStr };
            ParseTypeRecursive(newPath, typeStr);
        }
    }
}
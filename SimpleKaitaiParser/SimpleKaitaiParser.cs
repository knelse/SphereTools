using System.Text;
using BitStreams;
using SphereHelpers.Extensions;
using VYaml.Serialization;

namespace SimpleKaitaiParser;

public struct KaitaiScriptEntry
{
    public string Path;
    public string Type;
    public bool IsTrivialType;
    public string EnumName;
    public int Size;
    public string SizeRef;
    public string Encoding;
    public string Comment;

    public override string ToString ()
    {
        return $"{Path} {Type} {IsTrivialType} {EnumName} {Size} {Encoding} //{Comment}";
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
    public long StreamOffset;
    public int StreamBit;
    public string Comment;
    public long? LongValue;

    public override string ToString ()
    {
        var value = Value is null ? null :
            Value.GetType() == typeof (byte[]) ? Convert.ToHexString(Value as byte[]) : Value.ToString();
        return $"{Path} {Type} {IsTrivialType} {value} {EnumName} {EnumValue} // {Comment}";
    }
}

public class SimpleKaitaiParser
{
    public const string ByteArrayTypeName = "__byteArray";
    public const string UndefinedEnumEntry = "(undef)";
    public const string EnumEntryValueType = "__ENUM";
    private static Encoding Win1251 = null!;
    private readonly Dictionary<string, Dictionary<ulong, string>> DefinedEnums = new ();
    private readonly List<KaitaiParsedEntry> ParsedEntries = new ();
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
        ParsedEntries.Clear();
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
        foreach (var typeEntry in ScriptTypeOrder)
        {
            var parsedEntry = new KaitaiParsedEntry
            {
                Path = typeEntry.Path,
                Type = typeEntry.Type,
                IsTrivialType = typeEntry.IsTrivialType,
                EnumName = typeEntry.EnumName,
                StreamOffset = BitStream.Offset,
                StreamBit = BitStream.Bit,
                Comment = typeEntry.Comment
            };
            if (typeEntry.IsTrivialType)
            {
                var (value, longValue) = ReadTrivialTypeValue(typeEntry);
                parsedEntry.LongValue = longValue;
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

            ParsedEntries.Add(parsedEntry);
        }

        return ParsedEntries;
    }

    private Tuple<object, long?> ReadTrivialTypeValue (KaitaiScriptEntry typeEntry)
    {
        if (typeEntry.Type == "str")
        {
            // assuming win1251
            var actualSize = string.IsNullOrWhiteSpace(typeEntry.SizeRef)
                ? typeEntry.Size
                : ParsedEntries.First(x => x.Path.EndsWith(typeEntry.SizeRef)).LongValue ?? 0;
            if (actualSize == 0)
            {
                return new Tuple<object, long?>(string.Empty, null);
            }

            var strBytes = BitStream.ReadBytes(actualSize, true);
            return new Tuple<object, long?>(Win1251.GetString(strBytes), null);
        }

        if (typeEntry.Type == ByteArrayTypeName || typeEntry.Size != 0)
        {
            return new Tuple<object, long?>(BitStream.ReadBytes(typeEntry.Size, true), null);
        }

        var length = long.Parse(typeEntry.Type[1..]);

        if (typeEntry.Type.StartsWith('b'))
        {
            long? longValueBits = null;
            if (length <= 64)
            {
                longValueBits = BitStream.ReadInt64(length);
                BitStream.SeekBack((int) length);
            }

            return new Tuple<object, long?>(BitStream.ReadBytes(length), longValueBits);
        }

        long? longValue = null;
        if (length <= 8)
        {
            var bits = (int) length * 8;
            longValue = BitStream.ReadInt64(bits);
            BitStream.SeekBack(bits);
        }

        var bytes = BitStream.ReadBytes(length, true);

        if (typeEntry.Type.StartsWith('f'))
        {
            var floating = length == 4 ? BitConverter.ToSingle(bytes) : BitConverter.ToDouble(bytes);
            return new Tuple<object, long?>(floating, null);
        }

        return new Tuple<object, long?>(bytes, longValue);
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
        var size = 0;
        var sizeRef = "";
        if (typeDict.ContainsKey("size"))
        {
            var sizeStr = typeDict["size"].ToString();
            if (char.IsDigit(sizeStr, 0))
            {
                size = int.Parse(sizeStr);
            }
            else
            {
                sizeRef = sizeStr;
            }
        }

        var encoding = typeDict.ContainsKey("encoding") ? (string) typeDict["encoding"] : string.Empty;
        var enumName = typeDict.ContainsKey("enum") ? (string) typeDict["enum"] : string.Empty;
        var comment = typeDict.ContainsKey("comment") ? (string) typeDict["comment"] : string.Empty;
        if (IsTrivialType(typeStr))
        {
            ScriptTypeOrder.Add(new KaitaiScriptEntry
            {
                IsTrivialType = true,
                Path = pathString + '/' + idStr,
                Type = typeStr,
                Size = size,
                SizeRef = sizeRef,
                Encoding = encoding,
                EnumName = enumName,
                Comment = comment
            });
        }
        else
        {
            var newPath = new List<string>(currentPath) { idStr };
            ParseTypeRecursive(newPath, typeStr);
        }
    }
}
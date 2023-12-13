using System;
using Newtonsoft.Json;

namespace PacketLogViewer.AvalonEdit;

public class HexStringJsonConverter : JsonConverter
{
    public override bool CanConvert (Type objectType)
    {
        return typeof (int) == objectType || typeof (uint) == objectType || typeof (long) == objectType ||
               typeof (ulong) == objectType;
    }

    public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue($"0x{value:X} = {value}");
    }

    public override object ReadJson (JsonReader reader, Type objectType, object existingValue,
        JsonSerializer serializer)
    {
        var str = reader.ReadAsString();
        if (string.IsNullOrWhiteSpace(str) || !str.StartsWith("0x"))
        {
            throw new JsonSerializationException();
        }

        var hexValue = str.Split("=", StringSplitOptions.TrimEntries)[0];

        return Convert.ToUInt64(hexValue);
    }
}
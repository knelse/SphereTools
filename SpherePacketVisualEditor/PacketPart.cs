using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using BitStreams;
using SphServer.Helpers;

namespace SpherePacketVisualEditor;

public enum PacketPartType
{
    BITS,
    BYTES,
    INT64,
    UINT64,
    STRING,
    COORDS_CLIENT,
    COORDS_SERVER
}

public record PacketPartDisplayText (
    string bitsStr,
    string bytesStr,
    string textStr,
    string longStr,
    string ulongStr,
    string? enumValueStr,
    string? coordsClientStr,
    string? coordsServerStr)
{
    public readonly string Bits = bitsStr;
    public readonly string Bytes = bytesStr;
    public readonly string? EnumValue = enumValueStr;
    public readonly string Long = longStr;
    public readonly string Text = textStr;
    public readonly string Ulong = ulongStr;
    public readonly string? CoordsClient = coordsClientStr;
    public readonly string? CoordsServer = coordsServerStr;
}

public record StreamPosition (long Offset, int Bit)
{
    public int Bit = Bit;
    public long Offset = Offset;

    public override string ToString ()
    {
        return $"{Offset} ({FlipBitOffset(Bit)})";
    }

    public static int FlipBitOffset (int bit)
    {
        return bit == 0 ? 0 : 8 - bit;
    }

    public int CompareTo (StreamPosition other)
    {
        if (Offset > other.Offset)
        {
            return 1;
        }

        if (Offset < other.Offset)
        {
            return -1;
        }

        return Bit.CompareTo(other.Bit);
    }

    public long GetBitPosition ()
    {
        return Offset * 8 + Bit;
    }

    public long GetBitOffsetTo (StreamPosition other)
    {
        return Offset * 8 + Bit - (other.Offset * 8 + other.Bit);
    }

    public void ChangeOffsetAndBit (long byOffset, int byBit)
    {
        Offset += byOffset;
        Bit += byBit;
        while (Bit >= 8)
        {
            Offset += 1;
            Bit -= 8;
        }

        while (Bit < 0)
        {
            Offset -= 1;
            Bit += 8;
        }
    }
}

public class PacketPart
{
    public const string UndefinedFieldValue = "__undef";
    public const string LengthFromPreviousFieldValue = "__fromPrevious";
    public long BitLength;
    public readonly string? EnumName;
    public readonly Brush HighlightColor;
    public readonly bool LengthFromPreviousField;
    public readonly PacketPartType PacketPartType;
    public readonly StreamPosition StreamPositionEnd;
    public readonly StreamPosition StreamPositionStart;
    public PacketPartDisplayText DisplayText;
    public List<Bit> Value;

    public PacketPart (long length, Brush highlightColor, string name, string? enumName, bool lengthFromPreviousField,
        PacketPartType packetPartType,
        StreamPosition streamPositionStart, StreamPosition streamPositionEnd, List<Bit> value)
    {
        BitLength = length;
        LengthFromPreviousField = lengthFromPreviousField;
        HighlightColor = highlightColor;
        Name = name;
        EnumName = enumName;
        StreamPositionEnd = streamPositionEnd;
        StreamPositionStart = streamPositionStart;
        Value = value;
        PacketPartType = packetPartType;
        UpdateValueDisplayText();
    }

    public string PartListDisplayText { get; set; }

    public string Name { get; set; }

    public bool Overlaps (PacketPart other)
    {
        return StreamPositionStart.CompareTo(other.StreamPositionStart) <= 0 &&
               StreamPositionEnd.CompareTo(other.StreamPositionEnd) >= 0;
    }

    public bool ContainedWithin (PacketPart other)
    {
        return StreamPositionStart.CompareTo(other.StreamPositionStart) > 0 &&
               StreamPositionEnd.CompareTo(other.StreamPositionEnd) < 0;
    }

    public bool ContainsBitPosition (long bitPosition)
    {
        var startBitPosition = StreamPositionStart.GetBitPosition();
        var endBitPosition = StreamPositionEnd.GetBitPosition();
        return startBitPosition <= bitPosition && endBitPosition > bitPosition;
    }

    public PacketPart GetPiece (StreamPosition newStart, StreamPosition newEnd, string? name = null)
    {
        if (newStart.CompareTo(StreamPositionStart) < 0 || newEnd.CompareTo(StreamPositionEnd) > 0)
        {
            return this;
        }

        var newLength = newEnd.GetBitOffsetTo(newStart);
        var skipStart = (int) newStart.GetBitOffsetTo(StreamPositionStart);
        var skipEnd = (int) StreamPositionEnd.GetBitOffsetTo(newEnd);

        var newValue = Value.Skip(skipEnd > 0 ? skipEnd : 0).SkipLast(skipStart > 0 ? skipStart : 0).ToList();

        return new PacketPart(newLength, HighlightColor, name ?? Name, EnumName, false, PacketPartType,
            newStart, newEnd, newValue);
    }

    public string GetDisplayTextForValueType ()
    {
        switch (PacketPartType)
        {
            case PacketPartType.BITS:
                return DisplayText.Bits;
            case PacketPartType.BYTES:
                return DisplayText.Bytes;
            case PacketPartType.INT64:
                return DisplayText.Long;
            case PacketPartType.UINT64:
                return DisplayText.Ulong;
            case PacketPartType.STRING:
                return DisplayText.Text;
            case PacketPartType.COORDS_CLIENT:
                return DisplayText.CoordsClient;
            case PacketPartType.COORDS_SERVER:
                return DisplayText.CoordsServer;
            default:
                return string.Empty;
        }
    }

    public void UpdateValueDisplayText ()
    {
        var bits = new List<Bit>(Value);
        bits.Reverse();
        DisplayText = GetValueDisplayText(bits, EnumName);
        PartListDisplayText =
            $"{Name} ({StreamPositionStart.Offset}, {StreamPositionStart.Bit}) to ({StreamPositionEnd.Offset}, {StreamPositionEnd.Bit})";
    }

    public static PacketPartDisplayText GetValueDisplayText (List<Bit> bits, string? enumName)
    {
        var remainingBitsToFullByte = bits.Count % 8;
        var bitsPadding = remainingBitsToFullByte == 0 ? 0 : 8 - remainingBitsToFullByte;

        if (bitsPadding != 0)
        {
            for (var i = 0; i < bitsPadding; i++)
            {
                bits.Add(0);
            }
        }

        var bytes = BitStream.BitArrayToBytes(bits.ToArray());
        var stream = new BitStream(bytes);
        var textChars = MainWindow.Win1251.GetString(bytes).ToCharArray();
        var visibleChars = textChars.Select(MainWindow.GetVisibleChar).ToArray();
        var textString = new string(visibleChars);
        Array.Reverse(bytes);
        var bytesString = Convert.ToHexString(bytes);
        var actualBits = bits.ToArray()[..^bitsPadding];
        Array.Reverse(actualBits);
        var bitsString = string.Join("", actualBits.Select(x => (int) x));
        var longValue = stream.ReadInt64();
        var longValueStr = bytes.Length > 8 ? "(too large)" : $"0x{bytesString} = {longValue}";
        stream.Seek(0, 0);
        var ulongValue = stream.ReadUInt64();
        var ulongValueStr = bytes.Length > 8 ? "(too large)" : $"0x{bytesString} = {ulongValue}";
        stream.Seek(0, 0);
        var coordsServer = bytes.Length >= 4 ? CoordsHelper.DecodeServerCoordinate(bytes) : (double?) null;
        var coordsServerStr = coordsServer is null ? null : $"{coordsServer:F2}";
        var coordsClient = bytes.Length >= 5 ? CoordsHelper.DecodeClientCoordinate(bytes) : (double?) null;
        var coordsClientStr = coordsClient is null ? null : $"{coordsClient:F2}";

        var enumValueStr = enumName is null ? null : "(undef)";
        if (bytes.Length <= 8 && enumName is not null && MainWindow.DefinedEnums.ContainsKey(enumName) &&
            MainWindow.DefinedEnums[enumName].ContainsKey((int) ulongValue))
        {
            enumValueStr = MainWindow.DefinedEnums[enumName][(int) ulongValue];
        }

        return new PacketPartDisplayText(bitsString, bytesString, textString, longValueStr, ulongValueStr,
            enumValueStr, coordsClientStr, coordsServerStr);
    }

    public static List<PacketPart> LoadFromFile (string filePath, string groupName)
    {
        var contents = File.ReadAllLines(filePath);
        var parts = new List<PacketPart>();

        foreach (var line in contents)
        {
            var fieldValues = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);

            if (fieldValues.Length < 9)
            {
                Console.WriteLine($"Missing fields in {groupName}, line: {line}");
            }

            var partName = fieldValues[0];
            if (partName == UndefinedFieldValue)
            {
                // skip undef
                continue;
            }

            var packetPartType = Enum.TryParse(fieldValues[1], out PacketPartType partType)
                ? partType
                : PacketPartType.BITS;
            var start = int.Parse(fieldValues[2]);
            var length = 0;
            var lengthFromPrevious = false;
            if (fieldValues[3] == LengthFromPreviousFieldValue)
            {
                lengthFromPrevious = true;
            }
            else
            {
                length = int.Parse(fieldValues[3]);
            }

            var enumName = fieldValues[4];
            if (enumName == UndefinedFieldValue)
            {
                enumName = null;
            }

            var r = byte.Parse(fieldValues[5]);
            var g = byte.Parse(fieldValues[6]);
            var b = byte.Parse(fieldValues[7]);
            var a = byte.Parse(fieldValues[8]);
            var color = new Color
            {
                A = a,
                R = r,
                G = g,
                B = b
            };

            var startPosition = new StreamPosition(start / 8, start % 8);
            var end = start + length;
            var endPosition = new StreamPosition(end / 8, end % 8);

            var highlightColor = new SolidColorBrush
            {
                Color = color
            };

            var part = new PacketPart(length, highlightColor, partName, enumName, lengthFromPrevious, packetPartType,
                startPosition, endPosition, new List<Bit>());
            parts.Add(part);
        }

        return parts;
    }

    public PacketPart ChangeOffsetAndBit (long byOffset, int byBit)
    {
        StreamPositionStart.ChangeOffsetAndBit(byOffset, byBit);
        StreamPositionEnd.ChangeOffsetAndBit(byOffset, byBit);

        return this;
    }
}
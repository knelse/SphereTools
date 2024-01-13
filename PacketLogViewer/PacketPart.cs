using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using BitStreams;
using LiteDB;
using PacketLogViewer;
using SphServer.Helpers;
using SphServer.Helpers.Enums;

namespace SpherePacketVisualEditor;

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
    public string Bits { get; set; } = bitsStr;
    public string Bytes { get; set; } = bytesStr;
    public string? CoordsClient { get; set; } = coordsClientStr;
    public string? CoordsServer { get; set; } = coordsServerStr;
    public string? EnumValue { get; set; } = enumValueStr;
    public string Long { get; set; } = longStr;
    public string Text { get; set; } = textStr;
    public string Ulong { get; set; } = ulongStr;
}

public record StreamPosition (long offset, int bit)
{
    public int Bit { get; set; } = bit;
    public long Offset { get; set; } = offset;

    public override string ToString ()
    {
        return $"{Offset} ({Bit})";
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
    public string? EnumName { get; set; }
    public byte HighlightColorR { get; set; }
    public byte HighlightColorG { get; set; }
    public byte HighlightColorB { get; set; }
    public byte HighlightColorA { get; set; }
    public bool LengthFromPreviousField { get; set; }
    public PacketPartType PacketPartType { get; set; }
    public StreamPosition StreamPositionEnd { get; set; }
    public StreamPosition StreamPositionStart { get; set; }
    public long BitLength { get; set; }
    [BsonIgnore] public PacketPartDisplayText DisplayText;
    [BsonIgnore] public List<Bit> Value { get; set; }
    public string Comment { get; set; }
    public int? ActualIntValue { get; set; } = null;

    public PacketPart (long length, string name, string? enumName, bool lengthFromPreviousField,
        PacketPartType packetPartType, StreamPosition streamPositionStart, StreamPosition streamPositionEnd,
        List<Bit> value, byte r, byte g, byte b, byte a, string comment = "")
    {
        BitLength = length;
        LengthFromPreviousField = lengthFromPreviousField;
        HighlightColorR = r;
        HighlightColorG = g;
        HighlightColorB = b;
        HighlightColorA = a;
        Name = name;
        EnumName = enumName;
        StreamPositionEnd = streamPositionEnd;
        StreamPositionStart = streamPositionStart;
        Value = value;
        PacketPartType = packetPartType;
        Comment = comment;
        UpdateValueDisplayText();
    }

    public PacketPart ()
    {
        // required for litedb
    }

    public PacketPart Clone ()
    {
        var value = new List<Bit>(Value);
        return new PacketPart(BitLength, Name, EnumName, LengthFromPreviousField, PacketPartType,
            new StreamPosition(StreamPositionStart.Offset, StreamPositionStart.Bit),
            new StreamPosition(StreamPositionEnd.Offset, StreamPositionEnd.Bit), value, HighlightColorR,
            HighlightColorG, HighlightColorB, HighlightColorA, Comment);
    }

    [BsonIgnore] public string PartListDisplayText { get; set; }

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

        return new PacketPart(newLength, name ?? Name, EnumName, false, PacketPartType,
            newStart, newEnd, newValue, HighlightColorR, HighlightColorG, HighlightColorB, HighlightColorA);
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
                return DisplayText.CoordsClient ?? string.Empty;
            case PacketPartType.COORDS_SERVER:
                return DisplayText.CoordsServer ?? string.Empty;
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
        var textChars = PacketLogViewerMainWindow.Win1251.GetString(bytes).ToCharArray();
        var visibleChars = textChars.Select(PacketLogViewerMainWindow.GetVisibleChar).ToArray();
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
        var coordsClient = bytes.Length >= 4 ? CoordsHelper.DecodeClientCoordinateWithoutShift(bytes) : (double?) null;
        var coordsClientStr = coordsClient is null ? null : $"{coordsClient:F2}";

        var enumValueStr = enumName is null ? null : "(undef)";
        if (bytes.Length <= 8 && enumName is not null && PacketLogViewerMainWindow.DefinedEnums.ContainsKey(enumName) &&
            PacketLogViewerMainWindow.DefinedEnums[enumName].ContainsKey((int) ulongValue))
        {
            enumValueStr = PacketLogViewerMainWindow.DefinedEnums[enumName][(int) ulongValue];
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
            if (fieldValues[3].StartsWith(LengthFromPreviousFieldValue))
            {
                var startOffset = LengthFromPreviousFieldValue.Length;
                if (fieldValues[3].Length > startOffset)
                {
                    length = int.Parse(fieldValues[3][startOffset..]);
                }

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

            var startPosition = new StreamPosition(start / 8, start % 8);
            var end = start + length;
            var endPosition = new StreamPosition(end / 8, end % 8);

            var part = new PacketPart(length, partName, enumName, lengthFromPrevious, packetPartType,
                startPosition, endPosition, new List<Bit>(), r, g, b, a);
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
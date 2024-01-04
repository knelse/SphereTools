using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using BitStreams;

namespace SpherePacketVisualEditor;

public enum PacketPartType
{
    BITS,
    BYTES,
    INT64,
    UINT64,
    STRING
}

public record PacketPartDisplayText (string bitsStr, string bytesStr, string textStr, string longStr, string ulongStr)
{
    public readonly string Bits = bitsStr;
    public readonly string Bytes = bytesStr;
    public readonly string Long = longStr;
    public readonly string Text = textStr;
    public readonly string Ulong = ulongStr;
}

public record StreamPosition (long Offset, int Bit)
{
    public readonly int Bit = Bit;
    public readonly long Offset = Offset;

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
}

public class PacketPart
{
    public const string UndefinedPacketPartName = "__undef";
    public readonly long BitLength;
    public readonly Brush HighlightColor;
    public readonly string Name;
    public readonly PacketPartType PacketPartType;
    public readonly StreamPosition StreamPositionEnd;
    public readonly StreamPosition StreamPositionStart;
    public PacketPartDisplayText DisplayText;
    public List<Bit> Value;

    public PacketPart (long length, Brush highlightColor, string name, PacketPartType packetPartType,
        StreamPosition streamPositionStart,
        StreamPosition streamPositionEnd, List<Bit> value)
    {
        BitLength = length;
        HighlightColor = highlightColor;
        Name = name;
        StreamPositionEnd = streamPositionEnd;
        StreamPositionStart = streamPositionStart;
        Value = value;
        PacketPartType = packetPartType;
        UpdateValueDisplayText();
    }

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

        return new PacketPart(newLength, HighlightColor, name ?? Name, PacketPartType, newStart, newEnd, newValue);
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
            default:
                return string.Empty;
        }
    }

    public void UpdateValueDisplayText ()
    {
        var bits = new List<Bit>(Value);
        bits.Reverse();
        DisplayText = GetValueDisplayText(bits);
    }

    public static PacketPartDisplayText GetValueDisplayText (List<Bit> bits)
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

        return new PacketPartDisplayText(bitsString, bytesString, textString, longValueStr, ulongValueStr);
    }
}
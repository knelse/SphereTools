using BitStreams;

namespace SphereHelpers.Extensions;

public static class BitStreamExtensions
{
    public static void SeekBack (this BitStream bitStream, int countBits)
    {
        for (var i = 0; i < countBits; i++)
        {
            bitStream.ReturnBit();
        }
    }

    public static ushort ReadUInt16 (this BitStream bitStream, int countBits)
    {
        return (ushort) BitsToInt(bitStream.ReadBits(countBits));
    }

    public static ulong ReadUInt64 (this BitStream bitStream, long countBits)
    {
        return BitsToUInt64(bitStream.ReadBits(countBits));
    }

    public static void WriteUInt16 (this BitStream bitStream, ushort val, int countBits)
    {
        bitStream.WriteBits(IntToBits(val, countBits));
    }

    public static int BitsToInt (Bit[] bits)
    {
        var result = 0;

        for (var i = bits.Length - 1; i >= 0; i--)
        {
            result <<= 1;
            result += bits[i];
        }

        return result;
    }

    public static ulong BitsToUInt64 (Bit[] bits)
    {
        var result = (ulong) 0;

        for (var i = bits.Length - 1; i >= 0; i--)
        {
            result <<= 1;
            result += bits[i];
        }

        return result;
    }

    public static Bit[] IntToBits (int val, int length)
    {
        var result = new List<Bit>();

        while (val > 0)
        {
            result.Add(val & 0b1);
            val >>= 1;
        }

        while (result.Count < length)
        {
            result.Add(0);
        }

        return result.ToArray();
    }

    public static string ToByteString (this Bit[]? bits)
    {
        return Convert.ToHexString(BitStream.BitArrayToBytes(bits));
    }
}
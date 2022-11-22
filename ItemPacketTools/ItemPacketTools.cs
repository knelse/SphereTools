
using System.Text;
using BitStreams;

public enum ItemPacketEncodingGroup
{
    MantraBook,
    Mantra,
    MatPowderEli,
    Ring,
    WeaponArmor,
    FourSlotBag,
    Short4SlotBag,
    BrushwoodFood,
    Token,
    CraftFormula,
    ChestContainer
}

public struct ItemPacket
{
    public ushort Id;
    public Bit[] _skip1;
    public ushort Type;
    public Bit[] _skip2;
    public byte[] X;
    public byte[] Y;
    public byte[] Z;
    public byte[] T;
    public ushort GameId;
    public ushort SuffixMod;
    public Bit[] _skip3;
    public ushort BagId;
    public Bit[] _skip4;
    public ushort Count;
    // public Bit[] _skip5;
    public bool IsPremium;
    public Bit[] _premiumSkip;
    public Bit[] _strangeSkip;

    public bool FourBitShiftedSuffix;
    public bool IsStrangeSuffix;
    public ItemPacketEncodingGroup EncodingGroup;
    public byte ItemSeparator => shorterItemSeparator ? (byte) (0x7E >> 1) : (byte) 0x7E;
    // short item bags can have 7F as separator?
    public int ItemSeparatorLength => shorterItemSeparator ? 7 : noItemSeparator ? 0 : 8;

    private static readonly byte[] premiumByteMarker = { 0x05, 0x0F, 0x08 };
    private const int premiumIntMarker = 0x050F08;
    private bool shorterItemSeparator =>
        EncodingGroup is ItemPacketEncodingGroup.WeaponArmor;
    private bool noItemSeparator => EncodingGroup is ItemPacketEncodingGroup.FourSlotBag;

    public static ItemPacket FromStream(BitStream stream)
    {
        var offset = stream.Offset;
        var bit = stream.Bit;
        
        // var shortTypeMarkerOffset = 28; // 7th byte is 4 for short and 8 for long?
        // // shortTypeMarkerOffset -= (stream.Bit == 0 ? 0 : 8 - stream.Bit);
        // var shortOffset = offset + shortTypeMarkerOffset / 8;
        // var shortBit = shortTypeMarkerOffset % 8;
        // stream.Seek(shortOffset, shortBit);
        // // var itemIsShort = stream.ReadByte(4) == 0x4;
        var itemIsShort = false;
        // stream.Seek(offset, bit);
        
        var result = new ItemPacket
        {
            Id = stream.ReadUInt16(),
            _skip1 = stream.ReadBits(2),
            Type = stream.ReadUInt16(10),
            _skip2 = stream.ReadBits(10),
        };
        
        // short item could be anything, but type would be correct
        // TODO: fix short items
        // if (itemIsShort)
        // {
        //     result._skip3 = stream.ReadBits(result.Type == 405 ? 91 : 50);
        //         result.EncodingGroup = result.Type == 405 ? ItemPacketEncodingGroup.FourSlotBag : ItemPacketEncodingGroup.Short4SlotBag;
        //
        //     return result;
        // }
        // if (!itemIsShort)
        // {
        result.X = stream.ReadBytes(4, true);
        result.Y = stream.ReadBytes(4, true);
        result.Z = stream.ReadBytes(4, true);
        var longTailTest = stream.ReadBytes(4, true);
        var hasLongTail = longTailTest[2] != 0x16; 
        stream.SeekBack(32);
        result.T = stream.ReadBytes(4, true);
        result.GameId = stream.ReadUInt16(14);

        var suffix = stream.ReadUInt16(12) >> 8;
        result.IsStrangeSuffix = suffix is 0xE or 0x9;
        result.FourBitShiftedSuffix = suffix == 0x4;
        stream.SeekBack(12);

        if (result.IsStrangeSuffix)
        {
            result._strangeSkip = stream.ReadBits(suffix is 0xE ? 31 : 26);
        }

        switch (result.Type)
        {
            case 405: // 4 bag slot
                result.GetStreamDataAs4SlotBag(stream, itemIsShort);
                break;
            case 409: // small mantra book
            case 411: // mantra book
            case 412: // great mantra book
                result.GetStreamDataAsMantraBook(stream, itemIsShort);
                break; 
            case 1000: // white mantra
            case 1001: // black mantra
                result.GetStreamDataAsMantra(stream, itemIsShort);
                break;
            case 600: // mineral
            case 601: // plant
            case 602: // metal
            case 453: // powder
            case 455: // aoe powder
            case 471: // castle elixir
            case 472: // trap elixir
            case 709: // mob part
                result.GetStreamDataAsMaterialPowderElixir(stream, itemIsShort);
                break;
            case 760: // ring
            case 762: // golem ring
                result.GetStreamDataAsRing(stream, itemIsShort);
                break;
            case 552: // ruby ring
            case 653: // food
            case 700: // brushwood
                result.GetStreamDataAsBrushwoodFood(stream, itemIsShort);
                break;
            case 66: // token
            case 8: // token
                result.GetStreamDataAsToken(stream, itemIsShort);
                break;
            case 804: // craft formula
                result.GetStreamDataAsCraftFormula(stream, itemIsShort);
                break;
            case 0: // TBD: figure out, some chests get there
            case 210: // chest, container
            case 406: // chest
            case 415: // container with different shift
                result.GetStreamDataAsChestContainer(stream, itemIsShort);
                break;
            default:
                result.GetStreamDataAsWeaponArmor(stream, hasLongTail);
                break;
        }
        
        // no support for seeking forth from the current position?
        try
        {
            var premiumLevelSequence = (stream.ReadByte() << 16) + (stream.ReadByte() << 8) + stream.ReadByte();
            result.IsPremium = premiumLevelSequence == premiumIntMarker;

            if (result.IsPremium)
            {
                result._premiumSkip = stream.ReadBits(3);
            }
            else
            {
                stream.SeekBack(24);
            }
        }
        catch (IOException)
        {
            // expected for non-premium at the end of the packet
        }

        return result;
    }

    public void ToStream(BitStream stream, bool withSeparator = false)
    {
        stream.WriteUInt16(Id);
        stream.WriteBits(_skip1);
        stream.WriteUInt16(Type, 10);
        stream.WriteBits(_skip2);
        stream.WriteBytes(X, 4, true);
        stream.WriteBytes(Y, 4, true);
        stream.WriteBytes(Z, 4, true);
        stream.WriteBytes(T, 4, true);
        stream.WriteUInt16(GameId, 14);
        
        if (FourBitShiftedSuffix)
        {
            stream.WriteUInt16(SuffixMod, 12);
        }
        else
        {
            stream.WriteByte((byte) SuffixMod);
        }
        
        stream.WriteBits(_skip3);
        stream.WriteUInt16(BagId);
        stream.WriteBits(_skip4);

        // TODO: mantras (and prob other stuff) can have Count param too
        if (EncodingGroup is ItemPacketEncodingGroup.MatPowderEli)
        {
            stream.WriteUInt16(Count, 13);
        }

        if (IsPremium)
        {
            stream.WriteBytes(premiumByteMarker, premiumByteMarker.Length);
            stream.WriteBits(_premiumSkip);
        }
    }

    private void GetSuffixModSkip3BagId(BitStream stream, bool isShort)
    {
        SuffixMod = FourBitShiftedSuffix ? stream.ReadUInt16(12) : stream.ReadByte();

        if (SuffixMod is 0x11 or 0xD1)
        {
            _strangeSkip = stream.ReadBits(SuffixMod is 0x11 ? 28 : 22);
            var suffix = stream.ReadUInt16(12) >> 8;
            FourBitShiftedSuffix = suffix == 0x4;
            stream.SeekBack(12);
            SuffixMod = FourBitShiftedSuffix ? stream.ReadUInt16(12) : stream.ReadByte();
        }
        _skip3 = stream.ReadBits(17);
        BagId = stream.ReadUInt16();
    }

    private void GetStreamDataAsMantra(BitStream stream, bool isShort)
    {
        GetSuffixModSkip3BagId(stream, isShort);
        _skip4 = stream.ReadBits(42);
        EncodingGroup = ItemPacketEncodingGroup.Mantra;
    }

    private void GetStreamDataAsMaterialPowderElixir(BitStream stream, bool isShort)
    {
        GetSuffixModSkip3BagId(stream, isShort);
        _skip4 = stream.ReadBits(86);
        Count = stream.ReadUInt16(16);

        if (Count >> 15 == 1)
        {
            Count &= 0b111111111111;
            stream.SeekBack(12);
        }
        else
        {
            if (Id == 0x98D8)
            {
                stream.SeekBack(5);
            }
            stream.SeekBack(1);
        }
        EncodingGroup = ItemPacketEncodingGroup.MatPowderEli;
    }

    private void GetStreamDataAsRing(BitStream stream, bool isShort)
    {
        GetSuffixModSkip3BagId(stream, isShort);
        _skip4 = stream.ReadBits(197);
        EncodingGroup = ItemPacketEncodingGroup.Ring;
    }

    private void GetStreamDataAsMantraBook(BitStream stream, bool isShort)
    {
        SuffixMod = stream.ReadByte();
        _skip3 = stream.ReadBits(6);
        BagId = stream.ReadUInt16();
        _skip4 = stream.ReadBits(isShort ? 22 : 34); //93
        EncodingGroup = ItemPacketEncodingGroup.MantraBook;
    }

    private void GetStreamDataAs4SlotBag(BitStream stream, bool isShort)
    {
        SuffixMod = stream.ReadByte();

        if (SuffixMod != 192)
        {
            stream.SeekBack(22);
            _strangeSkip = stream.ReadBits(33);
            GameId = stream.ReadUInt16(14);
            SuffixMod = stream.ReadByte();
        }
        _skip3 = stream.ReadBits(6);
        BagId = stream.ReadUInt16();
        EncodingGroup = ItemPacketEncodingGroup.FourSlotBag;
        _skip4 = stream.ReadBits(int.MaxValue);

        // var endPacketTest = stream.ReadByte();
        // while (endPacketTest != 0b00001100)
        // {
        //     stream.SeekBack(8);
        //     skipList.Add(stream.ReadBit());
        //     endPacketTest = stream.ReadByte();
        // }
        // Bit skipBit;
        //
        // while ((skipBit = stream.ReadBit())!= 1)
        // {
        //     skipList.Add(skipBit);
        // }
        // stream.SeekBack(1);
        // var nextBits = BitStreamTools.BitArrayToString(stream.ReadBits(120));
        // stream.SeekBack(160);
        // var previousBits = BitStreamTools.BitArrayToString(stream.ReadBits(40));
        // var splitByteTest = stream.ReadByte(7);
        //
        // if (splitByteTest is not (0x3F))
        // {
        //     stream.SeekBack(7);
        //     skipList.AddRange(BitStreamTools.IntToBits(splitByteTest, 7));
        // }
        // _skip4 = skipList.ToArray();
        // var t = Convert.ToHexString(BitStream.BitArrayToBytes(_skip4));
        // Console.WriteLine(BitStreamTools.BitArrayToString(_skip4));
        // if ((Z[3]) == 0x63)
        // {
        //     // uhh
        //     stream.SeekBack(3);
        // }
    }

    private void GetStreamDataAsBrushwoodFood(BitStream stream, bool isShort)
    {
        SuffixMod = stream.ReadByte();
        _skip3 = stream.ReadBits(6);
        BagId = stream.ReadUInt16();
        _skip4 = stream.ReadBits(86);
        Count = stream.ReadUInt16(15);
        EncodingGroup = ItemPacketEncodingGroup.BrushwoodFood;
    }

    private void GetStreamDataAsToken(BitStream stream, bool isShort)
    {
        SuffixMod = stream.ReadByte();
        _skip3 = stream.ReadBits(6);
        BagId = stream.ReadUInt16();
        _skip4 = stream.ReadBits(86);
        Count = stream.ReadUInt16(15);
        _skip4 = stream.ReadBits(51); // 58 total
        EncodingGroup = ItemPacketEncodingGroup.Token;
    }

    private void GetStreamDataAsWeaponArmor(BitStream stream, bool hasLongTail)
    {
        GetSuffixModSkip3BagId(stream, false);
        //         _skip4 = stream.ReadBits(isPremium ? 86 : 71),
        _skip4 = stream.ReadBits(67);
        var skipBytes = BitStream.BitArrayToBytes(_skip4);

        if (skipBytes.Length > 1 && skipBytes[1] == 0xB)
        {
            var skipList = new List<Bit>(_skip4);
            var _skip = stream.ReadBits(29);
            skipList.AddRange(_skip);
            _skip4 = skipList.ToArray();
        }

        if (hasLongTail)
        {
            var skipList = new List<Bit>(_skip4);
            var _skip = stream.ReadBits(31);
            skipList.AddRange(_skip);
            _skip4 = skipList.ToArray();
        }
        EncodingGroup = ItemPacketEncodingGroup.WeaponArmor;
    }

    private void GetStreamDataAsCraftFormula(BitStream stream, bool isShort)
    {
        SuffixMod = stream.ReadByte();
        _skip3 = stream.ReadBits(6);
        BagId = stream.ReadUInt16();
        _skip4 = stream.ReadBits(414);
        EncodingGroup = ItemPacketEncodingGroup.CraftFormula;
    }

    private void GetStreamDataAsChestContainer(BitStream stream, bool isShort)
    {
        SuffixMod = stream.ReadByte(5);

        if (SuffixMod == 0x1A)
        {
            _skip3 = stream.ReadBits(42);
        }
        else if (SuffixMod == 0xA)
        {
            // happens with Type = 0, TBD
            _skip3 = stream.ReadBits(37);
        }
        else if (SuffixMod == 0x2)
        {
            // happens with Type = 415, TBD
            _skip3 = stream.ReadBits(64);
        }
        EncodingGroup = ItemPacketEncodingGroup.ChestContainer;
    }

    public string ToDebugString()
    {
        var typeName = Enum.GetName(EncodingGroup);
        string tabs = "\t";

        if (typeName.Length <= 11)
        {
            tabs = "\t\t";
        }
        return $"ID: {Id:X4}\tGMID: {GameId:D4}\tType: {Type} ({typeName}){tabs}Suff: {SuffixMod:D4}\tBag: {BagId:X4}\t" +
               $"X: {Convert.ToHexString(X)}\tY: {Convert.ToHexString(Y)}\tZ: {Convert.ToHexString(Z)}\tT: " +
               $"{Convert.ToHexString(X)}\tCount: {Count}\t PA: {IsPremium}\t{_skip1.ToByteString()}\t{_skip2.ToByteString()}\t" +
               $"{_skip3.ToByteString()}\t{_skip4.ToByteString()}\tPA: {_premiumSkip.ToByteString()}";
    }
}

public static class BitStreamTools
{
    public static List<ItemPacket> GetItemsFromPacket(byte[] packet, bool writeItemBytesToConsole = false)
    {
        byte[] trimmedPacket;
        if (packet[2] == 0x2C && packet[3] == 0x01 && packet[4] == 0x00)
        {
            // start of network packet
            trimmedPacket = new byte [packet.Length - 7];
            Array.Copy(packet, 7, trimmedPacket, 0, packet.Length - 7);
        }
        else
        {
            trimmedPacket = packet;
        }
        var containerStream = new BitStream(trimmedPacket);
        var result = new List<ItemPacket>();
        var offsets = new List<long>();

        try
        {
            while (containerStream.ValidPosition)
            {
                var test = containerStream.ReadBytes(4, true);

                if (IsItemPacket(test))
                {
                    var pos = (containerStream.Offset - 16) * 8 + containerStream.Bit;
                    offsets.Add(pos);
                }
                containerStream.SeekBack(31);
            }
        }
        catch (IOException)
        {
            
        }
        finally
        {
            containerStream.Seek(0, 0);
        }

        var packets = new List<byte[]>();

        for (var i = 0; i < offsets.Count - 1; i++)
        {
            var bitLength = offsets[i + 1] - offsets[i];
            var bits = containerStream.ReadBits(bitLength);
            var itemPacket = BitStream.BitArrayToBytes(bits);
            packets.Add(itemPacket);
        }

        var lastBits = containerStream.ReadBits(int.MaxValue);
        packets.Add(BitStream.BitArrayToBytes(lastBits));

        foreach (var itemPacket in packets)
        {
            var packetStream = new BitStream(itemPacket);
            var item = ItemPacket.FromStream(packetStream);
            result.Add(item);
            Console.WriteLine(Convert.ToHexString(itemPacket));
            
        }
        //
        // while (containerStream.ValidPosition)
        // {
        //     try
        //     {
        //         var offsetStart = containerStream.Offset;
        //         var bitStart = containerStream.Bit;
        //         var item = ItemPacket.FromStream(containerStream);
        //         result.Add(item);
        //         var offsetEnd = containerStream.Offset;
        //         var bitEnd = containerStream.Bit;
        //
        //         if (item.ItemSeparatorLength != 0)
        //         {
        //             containerStream.ReadByte(item.ItemSeparatorLength);
        //         }
        //
        //         if (writeItemBytesToConsole)
        //         {
        //             var packetBytes = containerStream.GetStreamDataBetween(offsetStart, bitStart, offsetEnd, bitEnd);
        //             Console.WriteLine("----" + Convert.ToHexString(packetBytes) + "----");
        //         }
        //     }
        //     catch (IOException)
        //     {
        //         break;
        //     }
        // }

        return result;
    }
    public static void SeekBack(this BitStream bitStream, int countBits)
    {
        for (var i = 0; i < countBits; i++)
        {
            bitStream.ReturnBit();
        }
    }

    public static ushort ReadUInt16(this BitStream bitStream, int countBits)
    {
        return (ushort) BitsToInt(bitStream.ReadBits(countBits));
    }

    public static void WriteUInt16(this BitStream bitStream, ushort val, int countBits)
    {
        bitStream.WriteBits(IntToBits(val, countBits));
    }

    public static int BitsToInt(Bit[] bits)
    {
        var result = 0;

        for (var i = bits.Length - 1; i >= 0; i--)
        {
            result <<= 1;
            result += bits[i];
        }

        return result;
    }

    public static Bit[] IntToBits(int val, int length)
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
    public static string ToByteString(this Bit[]? bits)
    {
        return Convert.ToHexString(BitStream.BitArrayToBytes(bits));
    }

    public static string RemainderToByteString(this BitStream bitStream)
    {
        return Convert.ToHexString(bitStream.GetStreamDataFromCurrentOffsetAndBit());
    }

    public static string BitArrayToString(Bit[] bits)
    {
        var sb = new StringBuilder();

        foreach (var bit in bits)
        {
            sb.Append((int) bit);
        }

        return sb.ToString();
    }
    
    public static bool ByteArrayCompare(byte[] what, byte[] to, int fromIndex = 0)
    {
        if (to.Length > what.Length - fromIndex)
        {
            return false;
        }

        for (var i = 0; i < to.Length; i++)
        {
            if (what[i + fromIndex] != to[i])
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsItemPacket(byte[] test)
    {
        return ((test[0] & 0b1111) is 0x8 or 0x9)// or 0xF9 or 0x08 or 0xF8 or 0x78 or 0x98)
                && ((test[1] >> 4) is 0x4 or 0x5) //0 or 0x4F or 0x5F or 0x5E or 0x5C or 0x58 or 0x47 or 0x50)
                && (test[2] & 0b1111) is 0x1
                && (test[3] is 0x44 or 0x45);
    }
}

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
    public int X;
    public int Y;
    public int Z;
    public int T;
    public ushort GameId;
    public ushort SuffixMod;
    public Bit[] _skip3;
    public ushort BagId;
    public Bit[] _skip4;
    public ushort Count;
    public Bit[] _skip5;
    public bool IsPremium;
    public Bit[] _premiumSkip;

    public bool FourBitShiftedSuffix;
    public ItemPacketEncodingGroup EncodingGroup;
    public byte ItemSeparator => shorterItemSeparator ? (byte) (0x7E >> 1) : (byte) 0x7E;
    // short item bags can have 7F as separator?
    public int ItemSeparatorLength => shorterItemSeparator ? 7 : 8;

    private static readonly byte[] premiumByteMarker = { 0x05, 0x0F, 0x08 };
    private const int premiumIntMarker = 0x050F08;
    private bool shorterItemSeparator =>
        EncodingGroup is ItemPacketEncodingGroup.WeaponArmor or ItemPacketEncodingGroup.FourSlotBag;

    public static ItemPacket FromStream(BitStream stream)
    {
        var result = new ItemPacket
        {
            Id = stream.ReadUInt16(),
            _skip1 = stream.ReadBitsOrRemainder(2),
            Type = stream.ReadUInt16(10),
            _skip2 = stream.ReadBitsOrRemainder(10),
        };
        
        // short item could be anything, but type would be correct
        // TODO: fix short items
        if (BitStreamTools.BitsToInt(result._skip2) == 0x214)
        {
                result._skip3 = stream.ReadBitsOrRemainder(result.Type == 405 ? 91 : 50);
                result.EncodingGroup = result.Type == 405 ? ItemPacketEncodingGroup.FourSlotBag : ItemPacketEncodingGroup.Short4SlotBag;

            return result;
        }
        else
        {
            result.X = stream.ReadInt32();
            result.Y = stream.ReadInt32();
            result.Z = stream.ReadInt32();
            result.T = stream.ReadInt32();
            result.GameId = stream.ReadUInt16(14);
        }

        result.FourBitShiftedSuffix = stream.ReadUInt16(12) >> 8 == 0x4;
        stream.SeekBack(12);

        switch (result.Type)
        {
            case 405: // 4 bag slot
                result.GetStreamDataAs4SlotBag(stream);
                break;
            case 409: // small mantra book
                result.GetStreamDataAsSmallMantraBook(stream);
                break;
            case 411: // mantra book
                result.GetStreamDataAsMantraBook(stream);
                break; 
            case 1000: // white mantra
            case 1001: // black mantra
                result.GetStreamDataAsMantra(stream);
                break;
            case 600: // mineral
            case 601: // plant
            case 602: // metal
            case 453: // powder
            case 455: // aoe powder
            case 472: // trap elixir
            case 473: // castle elixir (TBD)
            case 709: // mob part
                result.GetStreamDataAsMaterialPowderElixir(stream);
                break;
            case 760: // ring
                result.GetStreamDataAsRing(stream);
                break;
            case 552: // ruby ring
            case 653: // food
            case 700: // brushwood
                result.GetStreamDataAsBrushwoodFood(stream);
                break;
            case 8: // token
                result.GetStreamDataAsToken(stream);
                break;
            case 804: // craft formula
                result.GetStreamDataAsCraftFormula(stream);
                break;
            case 0: // TBD: figure out, some chests get there
            case 210: // chest, container
            case 406: // chest
            case 415: // container with different shift
                result.GetStreamDataAsChestContainer(stream);
                break;
            default:
                result.GetStreamDataAsWeaponArmor(stream);
                break;
        }
        
        // no support for seeking forth from the current position?
        try
        {
            var premiumLevelSequence = (stream.ReadByte() << 16) + (stream.ReadByte() << 8) + stream.ReadByte();
            result.IsPremium = premiumLevelSequence == premiumIntMarker;

            if (result.IsPremium)
            {
                result._premiumSkip = stream.ReadBitsOrRemainder(3);
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
        stream.WriteInt32(X);
        stream.WriteInt32(Y);
        stream.WriteInt32(Z);
        stream.WriteInt32(T);
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
            stream.WriteBits(_skip5);
        }

        if (IsPremium)
        {
            stream.WriteBytes(premiumByteMarker, premiumByteMarker.Length);
            stream.WriteBits(_premiumSkip);
        }
    }

    private void GetSuffixModSkip3BagId(BitStream stream)
    {
        SuffixMod = FourBitShiftedSuffix ? stream.ReadUInt16(12) : stream.ReadByte();
        _skip3 = stream.ReadBitsOrRemainder(17);
        BagId = stream.ReadUInt16();
    }

    private void GetStreamDataAsMantra(BitStream stream)
    {
        GetSuffixModSkip3BagId(stream);
        _skip4 = stream.ReadBitsOrRemainder(42);
        EncodingGroup = ItemPacketEncodingGroup.Mantra;
    }

    private void GetStreamDataAsMaterialPowderElixir(BitStream stream)
    {
        GetSuffixModSkip3BagId(stream);
        _skip4 = stream.ReadBitsOrRemainder(86);
        Count = stream.ReadUInt16(13);
        _skip5 = stream.ReadBitsOrRemainder(2);
        EncodingGroup = ItemPacketEncodingGroup.MatPowderEli;
    }

    private void GetStreamDataAsRing(BitStream stream)
    {
        GetSuffixModSkip3BagId(stream);
        _skip4 = stream.ReadBitsOrRemainder(197);
        EncodingGroup = ItemPacketEncodingGroup.Ring;
    }

    private void GetStreamDataAsMantraBook(BitStream stream)
    {
        SuffixMod = stream.ReadByte();
        _skip3 = stream.ReadBitsOrRemainder(6);
        BagId = stream.ReadUInt16();
        _skip4 = stream.ReadBitsOrRemainder(22);
        EncodingGroup = ItemPacketEncodingGroup.MantraBook;
    }

    private void GetStreamDataAsSmallMantraBook(BitStream stream)
    {
        SuffixMod = stream.ReadByte();
        _skip3 = stream.ReadBitsOrRemainder(6);
        BagId = stream.ReadUInt16();
        _skip4 = stream.ReadBitsOrRemainder(93);
        EncodingGroup = ItemPacketEncodingGroup.MantraBook;
    }

    private void GetStreamDataAs4SlotBag(BitStream stream)
    {
        SuffixMod = stream.ReadByte();
        _skip3 = stream.ReadBitsOrRemainder(6);
        BagId = stream.ReadUInt16();
        _skip4 = stream.ReadBitsOrRemainder(134);
        EncodingGroup = ItemPacketEncodingGroup.FourSlotBag;
    }

    private void GetStreamDataAsBrushwoodFood(BitStream stream)
    {
        SuffixMod = stream.ReadByte();
        _skip3 = stream.ReadBitsOrRemainder(6);
        BagId = stream.ReadUInt16();
        _skip4 = stream.ReadBitsOrRemainder(86);
        Count = stream.ReadUInt16(15);
        EncodingGroup = ItemPacketEncodingGroup.BrushwoodFood;
    }

    private void GetStreamDataAsToken(BitStream stream)
    {
        SuffixMod = stream.ReadByte();
        _skip3 = stream.ReadBitsOrRemainder(6);
        BagId = stream.ReadUInt16();
        _skip4 = stream.ReadBitsOrRemainder(86);
        Count = stream.ReadUInt16(15);
        _skip4 = stream.ReadBitsOrRemainder(51); // 58 total
        EncodingGroup = ItemPacketEncodingGroup.Token;
    }

    private void GetStreamDataAsWeaponArmor(BitStream stream)
    {
        GetSuffixModSkip3BagId(stream);
        //         _skip4 = stream.ReadBitsOrRemainder(isPremium ? 86 : 71),
        _skip4 = stream.ReadBitsOrRemainder(67);
        EncodingGroup = ItemPacketEncodingGroup.WeaponArmor;
    }

    private void GetStreamDataAsCraftFormula(BitStream stream)
    {
        SuffixMod = stream.ReadByte();
        _skip3 = stream.ReadBitsOrRemainder(6);
        BagId = stream.ReadUInt16();
        _skip4 = stream.ReadBitsOrRemainder(414);
        EncodingGroup = ItemPacketEncodingGroup.CraftFormula;
    }

    private void GetStreamDataAsChestContainer(BitStream stream)
    {
        SuffixMod = stream.ReadByte(5);

        if (SuffixMod == 0x1A)
        {
            _skip3 = stream.ReadBitsOrRemainder(42);
        }
        else if (SuffixMod == 0xA)
        {
            // happens with Type = 0, TBD
            _skip3 = stream.ReadBitsOrRemainder(37);
        }
        else if (SuffixMod == 0x2)
        {
            // happens with Type = 415, TBD
            _skip3 = stream.ReadBitsOrRemainder(64);
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
        return $"ID: {Id:X4}\tGMID: {GameId:D4}\tType: {Type} ({typeName}){tabs}Suff: {SuffixMod:D4}\tBag: {BagId:X4}\tX: {X:D10}\tY: {Y:D10}\tZ: {Z:D10}\tT: " +
               $"{T:D10}\tCount: {Count}\t PA: {IsPremium}\t{_skip1.ToByteString()}\t{_skip2.ToByteString()}\t" +
               $"{_skip3.ToByteString()}\t{_skip4.ToByteString()}\t{_skip5.ToByteString()}\t{_premiumSkip.ToByteString()}";
    }
}

public static class BitStreamTools
{
    public static List<ItemPacket> GetItemsFromPacket(byte[] packet)
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

        while (true)
        {
            try
            {
                var item = ItemPacket.FromStream(containerStream);
                result.Add(item);
                containerStream.ReadByte(item.ItemSeparatorLength);
            }
            catch (IOException)
            {
                break;
            }
        }

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
        return (ushort) BitsToInt(bitStream.ReadBitsOrRemainder(countBits));
    }

    public static void WriteUInt16(this BitStream bitStream, ushort val, int countBits)
    {
        bitStream.WriteBits(IntToBits(val, countBits));
    }

    public static Bit[] ReadBitsOrRemainder(this BitStream bitStream, int countBits)
    {
        var bits = new List<Bit>();
        for (var i = 0; i < countBits; i++)
        {
            try
            {
                bits.Add(bitStream.ReadBit());
            }
            catch (IOException)
            {
                return bits.ToArray();
            }
        }
        return bits.ToArray();
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

    public static byte[] BitArrayToBytes(Bit[]? bits)
    {
        if (bits == null || bits.Length == 0)
        {
            return new byte[] { };
        }
        var result = new List<byte>();

        var pos = bits.Length - 1;
        var rem = 0;
        byte b = 0;

        while (pos >= 0)
        {
            rem = 7;

            for (; rem >= 0 && pos >= 0; rem--)
            {
                b <<= 1;
                b += bits[pos];
                pos--;
            }
            result.Add(b);
        }

        if (rem > 0)
        {
            result.Add(b);
        }

        return result.ToArray();
    }

    public static string ToByteString(this Bit[]? bits)
    {
        return Convert.ToHexString(BitArrayToBytes(bits));
    }
}
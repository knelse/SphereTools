
using BitStreams;

Console.WindowWidth = 300;

var containerPacket = Convert.FromHexString(
    "61002C0100985F68AEE08B0FA8F22E09D00C1419803EE91840814566D6151560A0751DA0900500FFFFFFFF857750BBFB224B0B6B934B733B9B8101F831BC022F3E0095B92400E497630050066300C515996057548081D67580421600FCFFFFFF03");
    // "2AE8D08B0F185B2E094012F7180043D218409145E607234B010652D7010A5900F0FFFFFFFF3B");
var items = BitStreamTools.GetItemsFromPacket(containerPacket);

foreach (var item in items)
{
    Console.WriteLine(item.ToDebugString());
}

public enum ItemPacketEncodingGroup
{
    MantraBook,
    Mantra,
    MaterialPowderElixir,
    Ring,
    WeaponArmor
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
    public byte ItemSeparator => EncodingGroup is ItemPacketEncodingGroup.WeaponArmor ? (byte) (0x7E >> 1) : (byte) 0x7E;
    public int ItemSeparatorLength => EncodingGroup is ItemPacketEncodingGroup.WeaponArmor ? 7 : 8;

    private static readonly byte[] premiumByteMarker = { 0x05, 0x0F, 0x08 };
    private const int premiumIntMarker = 0x050F08;

    public static ItemPacket FromStream(BitStream stream)
    {
        var result = new ItemPacket
        {
            Id = stream.ReadUInt16(),
            _skip1 = stream.ReadBits(2),
            Type = stream.ReadUInt16(10),
            _skip2 = stream.ReadBits(10),
            X = stream.ReadInt32(),
            Y = stream.ReadInt32(),
            Z = stream.ReadInt32(),
            T = stream.ReadInt32(),
            GameId = stream.ReadUInt16(14)
        };
            
        result.FourBitShiftedSuffix = stream.ReadUInt16(12) >> 8 == 0x4;
        stream.SeekBack(12);

        switch (result.Type)
        {
            case 405: // TODO: bag
                result.GetStreamDataAsMantraBook(stream);
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
                result.GetStreamDataAsMaterialPowderElixir(stream);
                break;
            case 760: // ring
                result.GetStreamDataAsRing(stream);
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
        if (EncodingGroup is ItemPacketEncodingGroup.MaterialPowderElixir)
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
        _skip3 = stream.ReadBits(17);
        BagId = stream.ReadUInt16();
    }

    private void GetStreamDataAsMantra(BitStream stream)
    {
        GetSuffixModSkip3BagId(stream);
        _skip4 = stream.ReadBits(42);
        EncodingGroup = ItemPacketEncodingGroup.Mantra;
    }

    private void GetStreamDataAsMaterialPowderElixir(BitStream stream)
    {
        GetSuffixModSkip3BagId(stream);
        _skip4 = stream.ReadBits(86);
        Count = stream.ReadUInt16(13);
        _skip5 = stream.ReadBits(2);
        EncodingGroup = ItemPacketEncodingGroup.MaterialPowderElixir;
    }

    private void GetStreamDataAsRing(BitStream stream)
    {
        GetSuffixModSkip3BagId(stream);
        _skip4 = stream.ReadBits(197);
        EncodingGroup = ItemPacketEncodingGroup.Ring;
    }

    private void GetStreamDataAsMantraBook(BitStream stream)
    {
        SuffixMod = stream.ReadByte();
        _skip3 = stream.ReadBits(6);
        BagId = stream.ReadUInt16();
        _skip4 = stream.ReadBits(22);
        EncodingGroup = ItemPacketEncodingGroup.MantraBook;
    }

    private void GetStreamDataAsWeaponArmor(BitStream stream)
    {
        GetSuffixModSkip3BagId(stream);
        //         _skip4 = stream.ReadBits(isPremium ? 86 : 71),
        _skip4 = stream.ReadBits(67);
        EncodingGroup = ItemPacketEncodingGroup.WeaponArmor;
    }

    public string ToDebugString()
    {
        return $"ID: {Id:X4}\tGMID: {GameId}\tType: {Type}\tSuff: {SuffixMod}\tBag: {BagId:X4}\tX: {X}\tY: {Y}\tZ: {Z}\tT: " +
               $"{T}\tCount: {Count}\t PA: {IsPremium}\t{_skip1}\t{_skip2}\t{_skip3}\t{_skip4}\t{_skip5}";
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
}
using System.Text;
using BitStreams;
using LiteDB;

public struct ObjectPacket
{
    [BsonId] public int DbId { get; set; }
    public ushort Id { get; set; }
    public Bit[] _skip1 { get; set; }
    public ushort Type { get; set; }
    public Bit[] _skip2 { get; set; }
    public byte[] X { get; set; }
    public byte[] Y { get; set; }
    public byte[] Z { get; set; }
    public byte[] T { get; set; }
    public ushort GameId { get; set; }
    public ushort SuffixMod { get; set; }
    public Bit[] _skip3 { get; set; }
    public ushort BagId { get; set; }
    public Bit[] _skip4 { get; set; }
    public ushort Count { get; set; }
    public bool IsPremium { get; set; }
    public Bit[] _premiumSkip { get; set; }
    public Bit[] _strangeSkip { get; set; }
    public string FriendlyName { get; set; }
    public SphGameObject? GameObject { get; set; }
    public byte[] Packet { get; set; }
    public long BitsRead { get; set; }
    public bool FourBitShiftedSuffix { get; set; }
    public bool IsStrangeSuffix { get; set; }
    public ObjectPacketEncodingGroup EncodingGroup { get; set; }
    public PacketObjectTypes PacketObjectTypes { get; set; }
    public byte ObjectSeparator => shorterObjectSeparator ? (byte)(0x7E >> 1) : (byte)0x7E;
    public int ObjectSeparatorLength => shorterObjectSeparator ? 7 : noObjectSeparator ? 0 : 8;

    private static readonly byte[] premiumByteMarker = { 0x05, 0x0F, 0x08 };
    private const int premiumIntMarker = 0x050F08;

    private bool shorterObjectSeparator =>
        EncodingGroup is ObjectPacketEncodingGroup.WeaponArmor;

    private bool noObjectSeparator => EncodingGroup is ObjectPacketEncodingGroup.FourSlotBag;

    public static ObjectPacket FromStream(BitStream stream, Locale localeForFriendlyName = Locale.Russian)
    {
        var result = new ObjectPacket
        {
            Id = stream.ReadUInt16(),
            _skip1 = stream.ReadBits(2),
            Type = stream.ReadUInt16(10),
            _skip2 = stream.ReadBits(10),
            X = stream.ReadBytes(4, true),
            Y = stream.ReadBytes(4, true),
            Z = stream.ReadBytes(4, true)
        };
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

        result.PacketObjectTypes = PacketObjectTypes.Unknown;
        if (Enum.IsDefined(typeof(PacketObjectTypes), result.Type))
        {
            result.PacketObjectTypes = (PacketObjectTypes)result.Type;
        }

        switch (result.PacketObjectTypes)
        {
            case PacketObjectTypes.Sack: // 4 bag slot
                result.GetStreamDataAs4SlotBag(stream);
                break;
            case PacketObjectTypes.MantraBookSmall:
            case PacketObjectTypes.MantraBookLarge:
            case PacketObjectTypes.MantraBookGreat:
                result.GetStreamDataAsMantraBook(stream);
                break;
            case PacketObjectTypes.MantraWhite:
            case PacketObjectTypes.MantraBlack:
                result.GetStreamDataAsMantra(stream);
                break;
            case PacketObjectTypes.AlchemyMineral:
            case PacketObjectTypes.AlchemyPlant:
            case PacketObjectTypes.AlchemyMetal:
            case PacketObjectTypes.PowderSingleTarget:
            case PacketObjectTypes.PowderAoE:
            case PacketObjectTypes.ElixirCastle:
            case PacketObjectTypes.ElixirTrap:
            case PacketObjectTypes.MonsterPart:
            case PacketObjectTypes.Arrows:
                result.GetStreamDataAsMaterialPowderElixir(stream);
                break;
            case PacketObjectTypes.Ring:
            case PacketObjectTypes.RingGolem:
                result.GetStreamDataAsRing(stream);
                break;
            case PacketObjectTypes.RingRuby:
            case PacketObjectTypes.RingDiamond:
            case PacketObjectTypes.RingGold:
            case PacketObjectTypes.FoodApple:
            case PacketObjectTypes.FoodPear:
            case PacketObjectTypes.FoodMeat:
            case PacketObjectTypes.FoodBread:
            case PacketObjectTypes.FoodFish:
            case PacketObjectTypes.AlchemyBrushwood:
                result.GetStreamDataAsBrushwoodFood(stream);
                break;
            case PacketObjectTypes.Token:
            case PacketObjectTypes.TokenMultiuse:
            case PacketObjectTypes.TokenIsland:
            case PacketObjectTypes.TokenIslandGuest:
            case PacketObjectTypes.TokenTutorialTorweal:
                result.GetStreamDataAsToken(stream);
                break;
            case PacketObjectTypes.Blueprint: // craft formula
                result.GetStreamDataAsCraftFormula(stream);
                break;
            // case 0: // TBD: figure out, some chests get there
            // case 210: // chest, container
            case PacketObjectTypes.Chest: // chest
                // case 415: // container with different shift
                result.GetStreamDataAsChestContainer(stream);
                break;
            // case 407:
            case PacketObjectTypes.ScrollLegend:
            case PacketObjectTypes.ScrollRecipe:
                result.GetStreamDataAsScroll(stream);
                break;
            // case 30: // mut added
            case PacketObjectTypes.ArmorAmulet:
            case PacketObjectTypes.ArmorBelt:
            case PacketObjectTypes.ArmorBoots:
            case PacketObjectTypes.ArmorBracelet:
            case PacketObjectTypes.ArmorChest:
            case PacketObjectTypes.ArmorGloves:
            case PacketObjectTypes.ArmorHelmet:
            case PacketObjectTypes.ArmorPants:
            case PacketObjectTypes.ArmorRobe:
            case PacketObjectTypes.ArmorShield:
            case PacketObjectTypes.QuestArmorChest2:
            case PacketObjectTypes.QuestArmorBelt:
            case PacketObjectTypes.QuestArmorBoots:
            case PacketObjectTypes.QuestArmorBracelet:
            case PacketObjectTypes.QuestArmorChest:
            case PacketObjectTypes.QuestArmorGloves:
            case PacketObjectTypes.QuestArmorHelmet:
            case PacketObjectTypes.QuestArmorPants:
            case PacketObjectTypes.QuestArmorRing:
            case PacketObjectTypes.QuestArmorRobe:
            case PacketObjectTypes.QuestArmorShield:
            case PacketObjectTypes.WeaponAxe:
            case PacketObjectTypes.WeaponCrossbow:
            case PacketObjectTypes.WeaponSword:
            case PacketObjectTypes.QuestWeaponAxe:
            case PacketObjectTypes.QuestWeaponCrossbow:
            case PacketObjectTypes.QuestWeaponSword:
            case PacketObjectTypes.ArmorHelmetPremium:

            case PacketObjectTypes.Unknown:
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

        if (SphObjectDb.GameObjectDataDb.ContainsKey(result.GameId))
        {
            result.FriendlyName = SphObjectDb.GameObjectDataDb[result.GameId].Localisation[localeForFriendlyName];
            result.GameObject = SphObjectDb.GameObjectDataDb[result.GameId];
            var type = result.GameObject.GameObjectType;

            if (GameObjectDataHelper.ObjectTypeToSuffixLocaleMap.TryGetValue(type, out Dictionary<ItemSuffix, SuffixValueWithLocale>? value))
            {
                result.GameObject.Suffix = value.GetSuffixById(result.SuffixMod);
            }
        }
        else
        {
            result.FriendlyName = ObjectPacketTools.GetFriendlyNameByObjectType(result.PacketObjectTypes);
        }

        result.BitsRead = stream.Offset * 8 + stream.Bit;
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
            stream.WriteByte((byte)SuffixMod);
        }

        stream.WriteBits(_skip3);
        stream.WriteUInt16(BagId);
        stream.WriteBits(_skip4);

        if (PacketObjectTypes is PacketObjectTypes.Arrows or PacketObjectTypes.Bead or PacketObjectTypes.Ruby or PacketObjectTypes.Token
            or PacketObjectTypes.AlchemyBrushwood or PacketObjectTypes.AlchemyMetal or PacketObjectTypes.AlchemyMineral
            or PacketObjectTypes.AlchemyPlant or PacketObjectTypes.ElixirCastle or PacketObjectTypes.ElixirTrap or PacketObjectTypes.FoodApple
            or PacketObjectTypes.FoodBread or PacketObjectTypes.FoodFish or PacketObjectTypes.FoodMeat or PacketObjectTypes.FoodPear
            or PacketObjectTypes.MantraBlack or PacketObjectTypes.MantraWhite or PacketObjectTypes.MonsterPart or PacketObjectTypes.PowderAmilus
            or PacketObjectTypes.PowderFinale or PacketObjectTypes.PowderSingleTarget or PacketObjectTypes.RingDiamond or PacketObjectTypes.RingRuby
            or PacketObjectTypes.SeedCastle or PacketObjectTypes.TokenIsland or PacketObjectTypes.PowderAoE)
        {
            stream.WriteUInt16(Count);
        }

        // ignore for now, ideally forever
        // if (IsPremium)
        // {
        //     stream.WriteBytes(premiumByteMarker, premiumByteMarker.Length);
        //     stream.WriteBits(_premiumSkip);
        // }
    }

    private void GetSuffixModSkip3BagId(BitStream stream)
    {
        SuffixMod = FourBitShiftedSuffix ? stream.ReadUInt16(12) : stream.ReadByte();

        _strangeSkip = stream.ReadBits(SuffixMod switch
        {
            0x11 => 28,
            0xD1 => 22,
            0xBA => 22,
            0xC2 => 27,
            _ => 0
        });

        if (_strangeSkip.Length != 0)
        {
            var suffix = stream.ReadUInt16(12) >> 8;
            FourBitShiftedSuffix = suffix == 0x4;
            stream.SeekBack(12);
            SuffixMod = FourBitShiftedSuffix ? stream.ReadUInt16(12) : stream.ReadByte();
        }

        _skip3 = stream.ReadBits(21);
        BagId = stream.ReadUInt16();
    }

    private void GetStreamDataAsMantra(BitStream stream)
    {
        GetSuffixModSkip3BagId(stream);
        _skip4 = stream.ReadBits(27);
        Count = stream.ValidPosition ? stream.ReadUInt16() : (ushort)1;
        EncodingGroup = ObjectPacketEncodingGroup.Mantra;
    }

    private void GetStreamDataAsMaterialPowderElixir(BitStream stream)
    {
        GetSuffixModSkip3BagId(stream);
        _skip4 = stream.ReadBits(86);
        Count = stream.ReadUInt16(16);

        if ((ushort)(Count & 0xFFF) == 0xFFF)
        {
            Count = 1;
        }
        else if (Count >> 15 == 1)
        {
            Count &= 0b111111111111;
            stream.SeekBack(12);
        }
        else
        {
            stream.SeekBack(1);
        }

        EncodingGroup = ObjectPacketEncodingGroup.MatPowderEli;
    }

    private void GetStreamDataAsRing(BitStream stream)
    {
        GetSuffixModSkip3BagId(stream);
        _skip4 = stream.ReadBits(197);
        EncodingGroup = ObjectPacketEncodingGroup.Ring;
    }

    private void GetStreamDataAsMantraBook(BitStream stream)
    {
        SuffixMod = stream.ReadByte();
        _skip3 = stream.ReadBits(6);
        BagId = stream.ReadUInt16();
        _skip4 = stream.ReadBits(34); //93
        EncodingGroup = ObjectPacketEncodingGroup.MantraBook;
    }

    private void GetStreamDataAs4SlotBag(BitStream stream)
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
        EncodingGroup = ObjectPacketEncodingGroup.FourSlotBag;
        _skip4 = stream.ReadBits(int.MaxValue);
    }

    private void GetStreamDataAsBrushwoodFood(BitStream stream)
    {
        SuffixMod = stream.ReadByte();
        _skip3 = stream.ReadBits(6);
        BagId = stream.ReadUInt16();
        _skip4 = stream.ReadBits(86);
        Count = stream.ReadUInt16(15);
        EncodingGroup = ObjectPacketEncodingGroup.BrushwoodFood;
    }

    private void GetStreamDataAsToken(BitStream stream)
    {
        SuffixMod = stream.ReadByte();
        _skip3 = stream.ReadBits(6);
        BagId = stream.ReadUInt16();
        _skip4 = stream.ReadBits(86);
        Count = stream.ReadUInt16(15);
        _skip4 = stream.ReadBits(51); // 58 total
        EncodingGroup = ObjectPacketEncodingGroup.Token;
    }

    private void GetStreamDataAsWeaponArmor(BitStream stream, bool hasLongTail)
    {
        GetSuffixModSkip3BagId(stream);
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

        EncodingGroup = ObjectPacketEncodingGroup.WeaponArmor;
    }

    private void GetStreamDataAsCraftFormula(BitStream stream)
    {
        SuffixMod = stream.ReadByte();
        _skip3 = stream.ReadBits(6);
        BagId = stream.ReadUInt16();
        _skip4 = stream.ReadBits(414);
        EncodingGroup = ObjectPacketEncodingGroup.CraftFormula;
    }

    private void GetStreamDataAsChestContainer(BitStream stream)
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

        EncodingGroup = ObjectPacketEncodingGroup.ChestContainer;
    }

    private void GetStreamDataAsScroll(BitStream stream)
    {
        SuffixMod = stream.ReadByte();
        _skip3 = stream.ReadBits(6);
        BagId = stream.ReadUInt16();
        // _skip4 = stream.ReadBits(414);
        EncodingGroup = ObjectPacketEncodingGroup.Scroll;
    }

    public string ToDebugString()
    {
        var typeName = $"({Enum.GetName(PacketObjectTypes)!})";
        string tier;

        if (GameObject?.GameObjectType == GameObjectType.Ring)
        {
            tier = GameObject.TitleMinusOne > 0
                ? $"{GameObject.TitleMinusOne + 1}т"
                : GameObject.DegreeMinusOne > 0
                    ? $"{GameObject.DegreeMinusOne + 1}с"
                    : GameObject.ToRomanTierLiteral();
        }
        else
        {
            tier = GameObject?.ToRomanTierLiteral() ?? string.Empty;
        }

        var suffix = string.Empty;

        try
        {
            suffix = (GameObject?.Suffix ?? ItemSuffix.None) == ItemSuffix.None
                ? string.Empty
                : GameObjectDataHelper.ObjectTypeToSuffixLocaleMap.TryGetValue(GameObject!.GameObjectType, out Dictionary<ItemSuffix, SuffixValueWithLocale>? value) &&
value.ContainsKey(GameObject!.Suffix)
                    ? @$" {value[GameObject!.Suffix]
                        .localization[Locale.Russian]}"
                    : string.Empty;
        }
        catch
        {
            Console.WriteLine($"No suffix for {GameObject.GameObjectType} and ID {GameObject?.Suffix}");
        }

        var count = Count > 1 ? $" ({Count})" : string.Empty;
        var name = $"{FriendlyName}" + suffix + (string.IsNullOrEmpty(tier) ? tier : $" {tier}") + count;
        return $"{name,-44}ID: {Id:X4}  GMID: {GameId.ToString(),5}  " +
               $"Type: {Type.ToString(),4} {typeName,-24} Suff: {SuffixMod.ToString(),4}  Bag: {BagId:X4}  " +
               $"X: {Convert.ToHexString(X)}  Y: {Convert.ToHexString(Y)}  Z: {Convert.ToHexString(Z)}  T: " +
               $"{Convert.ToHexString(T)}  PA: {IsPremium.ToString(),5} {_skip1.ToByteString()}\t{_skip2.ToByteString()}\t" +
               $"{_skip3.ToByteString()}\t{_skip4.ToByteString()}\tPA: {_premiumSkip.ToByteString()}";
    }
}

public static class ObjectPacketTools
{
    public static List<ObjectPacket> GetObjectsFromPacket(byte[] packet)
    {
        byte[] trimmedPacket;
        if (packet[2] == 0x2C && packet[3] == 0x01 && packet[4] == 0x00)
        {
            // start of network packet
            trimmedPacket = new byte[packet.Length - 7];
            Array.Copy(packet, 7, trimmedPacket, 0, packet.Length - 7);
        }
        else
        {
            trimmedPacket = packet;
        }

        var containerStream = new BitStream(trimmedPacket);
        var result = new List<ObjectPacket>();
        var offsets = new List<long>();

        try
        {
            while (containerStream.ValidPosition)
            {
                var test = containerStream.ReadBytes(4, true);

                if (!containerStream.ValidPosition)
                {
                    break;
                }

                containerStream.SeekBack(32);

                if (IsObjectPacket(test))
                {
                    var pos = (containerStream.Offset - 16) * 8 + containerStream.Bit;
                    var currentOffset = containerStream.Offset;
                    var currentBit = containerStream.Bit;

                    containerStream.Seek(currentOffset - 14, currentBit);
                    containerStream.ReadBits(2);
                    var typeCheck = containerStream.ReadUInt16(10);

                    if (Enum.IsDefined(typeof(PacketObjectTypes), typeCheck))
                    {
                        offsets.Add(pos);
                    }

                    // Console.WriteLine($"Unknown: {typeCheck} at {currentOffset} {currentBit}");
                    containerStream.Seek(currentOffset, currentBit);
                }

                containerStream.ReadBit();
            }
        }
        catch (IOException)
        {
        }
        finally
        {
            if (offsets.Count > 0)
            {
                containerStream.Seek(offsets[0] / 8, (int)(offsets[0] % 8));
            }
        }

        offsets.Add(containerStream.Length * 8);

        var packets = new List<byte[]>();

        for (var i = 1; i < offsets.Count; i++)
        {
            var bitLength = offsets[i] - offsets[i - 1];
            var bits = containerStream.ReadBits(bitLength);
            var objectPacket = BitStream.BitArrayToBytes(bits);
            packets.Add(objectPacket);
        }

        foreach (var objectPacket in packets)
        {
            var packetStream = new BitStream(objectPacket);
            var obj = ObjectPacket.FromStream(packetStream);
            obj.Packet = objectPacket;
            result.Add(obj);
        }

        containerStream.GetStream().Dispose();

        return result;
    }

    public static string GetTextOutput(List<ObjectPacket> objectPackets, bool writePacketsToConsole = false)
    {
        if (objectPackets.Count == 0)
        {
            return string.Empty;
        }

        objectPackets.Sort((a, b) => a.BagId.CompareTo(b.BagId));

        var currentBagId = -1;
        var sb = new StringBuilder();

        foreach (var objectPacket in objectPackets)
        {
            if (objectPacket.BagId != currentBagId)
            {
                currentBagId = objectPacket.BagId;

                if (writePacketsToConsole)
                {
                    Console.WriteLine($"[{currentBagId:X4}]");
                }

                sb.AppendLine($"[{currentBagId:X4}]");
            }

            if (writePacketsToConsole)
            {
                Console.WriteLine(objectPacket.BitsRead + " " + Convert.ToHexString(objectPacket.Packet));
            }

            sb.AppendLine(objectPacket.ToDebugString());
        }

        return sb.ToString();
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
        return (ushort)BitsToInt(bitStream.ReadBits(countBits));
    }

    public static uint ReadUInt32(this BitStream bitStream, int countBits)
    {
        return (uint)BitsToInt(bitStream.ReadBits(countBits));
    }

    public static void WriteUInt16(this BitStream bitStream, ushort val, int countBits)
    {
        bitStream.WriteBits(IntToBits(val, countBits));
    }

    public static void WriteUInt32(this BitStream bitStream, uint val, int countBits)
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

    public static Bit[] IntToBits(uint val, int length)
    {
        var result = new List<Bit>();

        while (val > 0)
        {
            result.Add((int)val & 0b1);
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
            sb.Append((int)bit);
        }

        return sb.ToString();
    }

    public static bool IsObjectPacket(byte[] test)
    {
        return (test[0] & 0b1111) is 0x8 or 0x9 or 0x0 // or 0xF9 or 0x08 or 0xF8 or 0x78 or 0x98)
               && test[1] >> 4 is 0x4 or 0x5 //0 or 0x4F or 0x5F or 0x5E or 0x5C or 0x58 or 0x47 or 0x50)
               && (test[2] & 0b1) is 0x1
               && test[3] is 0x44 or 0x45;
    }

    public static string GetFriendlyNameByObjectType(PacketObjectTypes packetObjectTypes)
    {
        return (packetObjectTypes switch
        {
            PacketObjectTypes.Arrows => "Стрелы",
            PacketObjectTypes.Bead => "Бусинка",
            PacketObjectTypes.Blueprint => "Формула",
            PacketObjectTypes.Ear => "Ухо",
            PacketObjectTypes.Firecracker => "Петарда",
            PacketObjectTypes.Firework => "Фейерверк",
            PacketObjectTypes.Inkpot => "Чернильница",
            PacketObjectTypes.Key => "Ключ",
            PacketObjectTypes.Sack => "Мешочек",
            PacketObjectTypes.Token => "Жетон телепортации",
            PacketObjectTypes.AlchemyBrushwood => "Хворост",
            PacketObjectTypes.AlchemyPot => "Алхимический котелок",
            PacketObjectTypes.BackpackLarge => "Большая торба",
            PacketObjectTypes.BackpackSmall => "Малая торба",
            PacketObjectTypes.EarString => "Нитка для ушей",
            PacketObjectTypes.FoodApple => "Яблоко",
            PacketObjectTypes.FoodBread => "Хлебная лепешка",
            PacketObjectTypes.FoodFish => "Сушеная рыба",
            PacketObjectTypes.FoodMeat => "Вяленое мясо",
            PacketObjectTypes.FoodPear => "Груша",
            PacketObjectTypes.KeyBarn => "Ключ от амбара",
            PacketObjectTypes.MapBook => "Книга карт",
            PacketObjectTypes.RecipeBook => "Книга рецептов",
            PacketObjectTypes.ScrollLegend => "Свиток (легенда)",
            PacketObjectTypes.ScrollRecipe => "Свиток, рецепт",
            PacketObjectTypes.SeedCastle => "Замковое семя",
            PacketObjectTypes.SpecialGuild => "Гильдия",
            PacketObjectTypes.SpecialAbility => "Спецспособность",
            PacketObjectTypes.TokenIsland => "Жетон телепортации на ЛО",
            PacketObjectTypes.TokenMultiuse => "Жетон телепортации",
            PacketObjectTypes.TradeLicense => "Торговая лицензия",
            PacketObjectTypes.MantraBookGreat => "Великая книга мантр",
            PacketObjectTypes.MantraBookLarge => "Большая книга мантр",
            PacketObjectTypes.MantraBookSmall => "Малая книга мантр",
            PacketObjectTypes.TokenIslandGuest => "Гостевой жетон на ЛО",
            PacketObjectTypes.XpPillDegree => "Пилюля опыта (степень)",
            PacketObjectTypes.RingDiamond => "Кольцо с алмазом",
            PacketObjectTypes.RingGold => "Золотое кольцо",
            PacketObjectTypes.RingRuby => "Кольцо с рубином",
            PacketObjectTypes.Ruby => "Рубин",
            PacketObjectTypes.Mutator => "Мутатор",
            PacketObjectTypes.PowderAmilus => "Порошок Амилуса",
            PacketObjectTypes.PowderFinale => "Порошок Файналя",
            _ => Enum.GetName(packetObjectTypes)
        })!;
    }

    public static bool IsQuestItem(PacketObjectTypes packetObjectTypes)
    {
        return packetObjectTypes is
            PacketObjectTypes.QuestArmorChest2
            or PacketObjectTypes.QuestArmorBelt
            or PacketObjectTypes.QuestArmorBoots
            or PacketObjectTypes.QuestArmorChest
            or PacketObjectTypes.QuestArmorGloves
            or PacketObjectTypes.QuestArmorBracelet
            or PacketObjectTypes.QuestArmorHelmet
            or PacketObjectTypes.QuestArmorPants
            or PacketObjectTypes.QuestArmorRobe
            or PacketObjectTypes.QuestArmorShield
            or PacketObjectTypes.QuestWeaponAxe
            or PacketObjectTypes.QuestWeaponCrossbow
            or PacketObjectTypes.QuestWeaponSword;
    }
}
// ReSharper disable UnassignedField.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

public class SphGameObject
{
    public GameObjectKind ObjectKind;
    public int GameId;
    public string SphereType = null!;
    public GameObjectType ObjectType;
    public string ModelNameGround = null!;
    public string ModelNameInventory = null!;
    public int HpCost;
    public int MpCost;
    public int TitleMinusOne;
    public int DegreeMinusOne;
    public KarmaTypes MinKarmaLevel;
    public KarmaTypes MaxKarmaLevel;
    public int StrengthReq;
    public int AgilityReq;
    public int AccuracyReq;
    public int EnduranceReq;
    public int EarthReq;
    public int AirReq;
    public int WaterReq;
    public int FireReq;
    public int PAtkNegative;
    public int MAtkNegativeOrHeal;
    public int MPHeal;
    public int t1;
    public int MaxHpUp;
    public int MaxMpUp;
    public int PAtkUpNegative;
    public int PDefUp;
    public int MDefUp;
    public int StrengthUp;
    public int AgilityUp;
    public int AccuracyUp;
    public int EnduranceUp;
    public int EarthUp;
    public int AirUp;
    public int WaterUp;
    public int FireUp;
    public int MAtkUpNegative;
    public int Weight;
    public int Durability;
    public int _range;
    public int UseTime;
    public int VendorCost;
    public int MutatorId;
    public int _duration;
    public int ReuseDelayHours;
    public int t2;
    public int t3;
    public int t4;
    public int t5;
    public string t6 = null!;
    public string t7 = null!;
    public int Tier;
    public int Range;
    public int Radius;
    /// <summary>
    /// Seconds
    /// </summary>
    public int Duration;
    public ItemSuffix Suffix = ItemSuffix.None;
    public int ItemCount = 1;
    public Dictionary<Locale, string> Localisation = new();

    public static readonly HashSet<GameObjectType> Mantras = new ()
    {
        GameObjectType.MantraBlack,
        GameObjectType.MantraWhite
    };

    public static readonly HashSet<GameObjectType> MaterialsPowdersElixirs = new()
    {
        GameObjectType.Flower, GameObjectType.Metal, GameObjectType.Mineral, GameObjectType.Powder,
        GameObjectType.Powder_Area, GameObjectType.Elixir_Castle, GameObjectType.Elixir_Trap
    };

    public static readonly HashSet<GameObjectType> WeaponsArmor = new()
    {
        GameObjectType.Crossbow, GameObjectType.Axe, GameObjectType.Sword, GameObjectType.Amulet, GameObjectType.Armor,
        GameObjectType.Belt, GameObjectType.Bracelet, GameObjectType.Gloves, GameObjectType.Helmet,
        GameObjectType.Pants, GameObjectType.Shield, GameObjectType.Boots, GameObjectType.Robe
    };

    public string ToDebugString()
    {
        var itemCountStr = ItemCount > 1 ? $"Count: {ItemCount}" : "";
        return $"Kind: {Enum.GetName(typeof(GameObjectKind), ObjectKind)} " +
               $"ID: {GameId} Type: {Enum.GetName(typeof(GameObjectType), ObjectType)} Ground: {ModelNameGround} " +
               $"Inv: {ModelNameInventory} HpCost: {HpCost} MpCost: {MpCost} TitleReq: {TitleMinusOne} " +
               $"DegreeReq: {DegreeMinusOne} KarmaMin: {Enum.GetName(typeof(KarmaTypes), MinKarmaLevel)} " +
               $"KarmaMax: {Enum.GetName(typeof(KarmaTypes), MaxKarmaLevel)} StrengthReq: {StrengthReq} " +
               $"AgilityReq: {AgilityReq} AccuracyReq: {AccuracyReq} EnduranceReq: {EnduranceReq} EarthReq: {EarthReq} " +
               $"AirReq: {AirReq} WaterReq: {WaterReq} FireReq: {FireReq} PA: {PAtkNegative} MA: {MAtkNegativeOrHeal} " +
               $"MPHeal: {MPHeal} T1: {t1} MaxHPUp: {MaxHpUp} MaxMpUp: {MaxMpUp} PAUp: {PAtkUpNegative} PDUp: {PDefUp} " +
               $"MDUp: {MDefUp} StrengthUp: {StrengthUp} AgilityUp: {AgilityUp} AccuracyUp: {AccuracyUp} " +
               $"EnduranceUp: {EnduranceUp} EarthUp: {EarthUp} AirUp: {AirUp} WaterUp: {WaterUp} FireUp: {FireUp} " +
               $"MAUp: {MAtkUpNegative} Weight: {Weight} Durability: {Durability} Range: {Range} Radius: {Radius} " +
               $"UseTime: {UseTime} VendorCost: {VendorCost} MutatorId: {MutatorId} Duration: {Duration} " +
               $"ReuseDelayHours: {ReuseDelayHours} T2: {t2} T3: {t3} T4: {t4} T5: {t5} T6: {t6} T7: {t7} Tier: {Tier} " +
               $"Suffix: {Enum.GetName(typeof(ItemSuffix), Suffix)} {itemCountStr}";
    }
    //
    // public byte[] GetLootItemBytes(ushort bagId, ushort itemId, bool bitShiftForRings = false)
    // {
    //     var objid_1 = (byte) (((GameId & 0b11) << 6) + 0b100110);
    //     var objid_2 = (byte) ((GameId >> 2) & 0b11111111);
    //     var objid_3 = (byte) (((GameId >> 10) & 0b1111) + 0b00010000);
    //     byte bagid_1;
    //     byte bagid_2;
    //     byte bagid_3;
    //
    //     if (!bitShiftForRings)
    //     {
    //         bagid_1 = (byte) (((bagId) & 0b111) << 5);
    //         bagid_2 = (byte) ((bagId >> 3) & 0b11111111);
    //         bagid_3 = (byte) ((bagId >> 11) & 0b11111);
    //     }
    //     else
    //     {
    //         bagid_1 = (byte) (((bagId) & 0b1111111) << 1);
    //         bagid_2 = (byte) ((bagId >> 7) & 0b11111111);
    //         bagid_3 = (byte) ((bagId >> 15) & 0b1);
    //     }
    //
    //     Console.WriteLine(ToDebugString());
    //
    //     if (Mantras.Contains(ObjectType))
    //     {
    //         return new byte[] 
    //         {
    //             MinorByte(itemId), MajorByte(itemId), (byte) (ObjectType == GameObjectType.MantraBlack ? 0xA4 : 0xA0), 
    //             0x8F, 0x0F, 0x80, 0x84, 0x2E, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x91, 0x45, 
    //             objid_1, objid_2, objid_3, 0x15, 0x60, bagid_1, bagid_2, bagid_3, 0xA0, 0xC0, 0x02, 0x01, 0x00
    //         };
    //     }
    //
    //     if (MaterialsPowdersElixirs.Contains(ObjectType))
    //     {
    //         byte typeid_1 = 0;
    //         byte typeid_2 = 0;
    //
    //         switch (ObjectType)
    //         {
    //             case GameObjectType.Flower:
    //                 typeid_1 = 0x64;
    //                 typeid_2 = 0x89;
    //                 break;
    //             case GameObjectType.Metal:
    //                 typeid_1 = 0x68;
    //                 typeid_2 = 0x89;
    //                 break;
    //             case GameObjectType.Mineral:
    //                 typeid_1 = 0x60;
    //                 typeid_2 = 0x89;
    //                 break;
    //             case GameObjectType.Powder:
    //                 typeid_1 = 0x14;
    //                 typeid_2 = 0x87;
    //                 break;
    //             case GameObjectType.Powder_Area:
    //                 typeid_1 = 0x1C;
    //                 typeid_2 = 0x87;
    //                 break;
    //             case GameObjectType.Elixir_Castle:
    //             case GameObjectType.Elixir_Trap:
    //                 typeid_1 = 0x60;
    //                 typeid_2 = 0x87;
    //                 break;
    //         }
    //
    //         return new byte[]
    //         {
    //             MinorByte(itemId), MajorByte(itemId), typeid_1, typeid_2, 0x0F, 0x80, 0x84, 0x2E, 0x09, 0x00, 0x00,
    //             0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x91, 0x45, objid_1, objid_2, objid_3, 0x15, 0x60, bagid_1,
    //             bagid_2, bagid_3, 0xA0, 0x90, 0x05, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x05, 0x16,
    //             (byte) ((ItemCount & 0b11111) << 3), (byte) ((ItemCount >> 5) & 0b11111111), 0x00
    //         };
    //     }
    //
    //     if (WeaponsArmor.Contains(ObjectType))
    //     {
    //         
    //         byte typeid_1 = 0;
    //         byte typeid_2 = 0;
    //         byte typeIdMod_1 = 0x15;
    //         byte typeIdMod_2 = 0x60;
    //         byte itemSuffixMod = 0x10;
    //         
    //         switch (ObjectType)
    //         {
    //             case GameObjectType.Crossbow:
    //                 typeid_1 = 0xD8;
    //                 typeid_2 = 0x87;
    //                 break;
    //             case GameObjectType.Sword:
    //                 typeid_1 = 0xD0;
    //                 typeid_2 = 0x87;
    //                 break;
    //             case GameObjectType.Axe:
    //                 typeid_1 = 0xD4;
    //                 typeid_2 = 0x87;
    //                 break;
    //             case GameObjectType.Amulet:
    //                 typeid_1 = 0xBC;
    //                 typeid_2 = 0x8B;
    //                 break;
    //             case GameObjectType.Armor:
    //                 typeid_1 = 0xB8;
    //                 typeid_2 = 0x8B;
    //                 break;
    //             case GameObjectType.Bracelet:
    //                 typeid_1 = 0xDC;
    //                 typeid_2 = 0x8B;
    //                 break;
    //             case GameObjectType.Belt:
    //                 typeid_1 = 0xCC;
    //                 typeid_2 = 0x8B;
    //                 break;
    //             case GameObjectType.Gloves:
    //                 typeid_1 = 0xC8;
    //                 typeid_2 = 0x8B;
    //                 break;
    //             case GameObjectType.Helmet:
    //                 typeid_1 = 0xD4;
    //                 typeid_2 = 0x8B;
    //                 break;
    //             case GameObjectType.Pants:
    //                 typeid_1 = 0xD8;
    //                 typeid_2 = 0x8B;
    //                 break;
    //             case GameObjectType.Shield:
    //                 typeid_1 = 0xD0;
    //                 typeid_2 = 0x8B;
    //                 break;
    //             case GameObjectType.Boots:
    //                 typeid_1 = 0xC0;
    //                 typeid_2 = 0x8B;
    //                 break;
    //             case GameObjectType.Robe:
    //                 typeid_1 = 0xE4;
    //                 typeid_2 = 0x8B;
    //                 break;
    //         }
    //
    //         if (ObjectType is GameObjectType.Sword or GameObjectType.Axe)
    //         {
    //             switch (Suffix)
    //             {
    //                 case ItemSuffix.None:
    //                     break;
    //                 case ItemSuffix.Exhaustion:
    //                 case ItemSuffix.Ether:
    //                 case ItemSuffix.Valor:
    //                 case ItemSuffix.Fatigue:
    //                     typeIdMod_1 = 0x44;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 case ItemSuffix.Damage:
    //                 case ItemSuffix.Disease:
    //                 case ItemSuffix.Cruelty:
    //                 case ItemSuffix.Instability:
    //                     typeIdMod_1 = 0x45;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 case ItemSuffix.Haste:
    //                 case ItemSuffix.Range:
    //                 case ItemSuffix.Speed:
    //                 case ItemSuffix.Distance:
    //                     typeIdMod_1 = 0x46;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 case ItemSuffix.Disorder:
    //                 case ItemSuffix.Decay:
    //                 case ItemSuffix.Chaos:
    //                 case ItemSuffix.Devastation:
    //                     typeIdMod_1 = 0x47;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 // case ItemSuffix.Exhaustion:
    //                 case ItemSuffix.Weakness:
    //                 // case ItemSuffix.Valor:
    //                 case ItemSuffix.Penetration:
    //                     typeIdMod_1 = 0x48;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 // case ItemSuffix.Damage:
    //                 case ItemSuffix.Interdict:
    //                 // case ItemSuffix.Cruelty:
    //                 case ItemSuffix.Value:
    //                     typeIdMod_1 = 0x49;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                     
    //             }
    //             switch (Suffix)
    //             {
    //                 case ItemSuffix.None:
    //                     break;
    //                 case ItemSuffix.Exhaustion:
    //                 case ItemSuffix.Damage:
    //                 case ItemSuffix.Haste:
    //                 case ItemSuffix.Disorder:
    //                     itemSuffixMod = 0x0;
    //                     break;
    //                 case ItemSuffix.Ether:
    //                 case ItemSuffix.Disease:
    //                 case ItemSuffix.Range:
    //                 case ItemSuffix.Decay:
    //                 case ItemSuffix.Weakness:
    //                 case ItemSuffix.Interdict:
    //                     itemSuffixMod = 0x20;
    //                     break;
    //                 case ItemSuffix.Valor:
    //                 case ItemSuffix.Cruelty:
    //                 case ItemSuffix.Speed:
    //                 case ItemSuffix.Chaos:
    //                     itemSuffixMod = 0x80;
    //                     break;
    //                 case ItemSuffix.Fatigue:
    //                 case ItemSuffix.Instability:
    //                 case ItemSuffix.Distance:
    //                 case ItemSuffix.Devastation:
    //                 case ItemSuffix.Penetration:
    //                 case ItemSuffix.Value:
    //                     itemSuffixMod = 0xA0;
    //                     break;
    //                     
    //             }
    //         }
    //         if (ObjectType is GameObjectType.Crossbow)
    //         {
    //             switch (Suffix)
    //             {
    //                 case ItemSuffix.None:
    //                     break;
    //                 case ItemSuffix.Exhaustion:
    //                 case ItemSuffix.Penetration:
    //                 case ItemSuffix.Valor:
    //                 case ItemSuffix.Instability:
    //                     typeIdMod_1 = 0x44;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 case ItemSuffix.Damage:
    //                 case ItemSuffix.Decay:
    //                 case ItemSuffix.Cruelty:
    //                 case ItemSuffix.Range:
    //                     typeIdMod_1 = 0x45;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 case ItemSuffix.Haste:
    //                 case ItemSuffix.Distance:
    //                 case ItemSuffix.Speed:
    //                 case ItemSuffix.Mastery:
    //                     typeIdMod_1 = 0x46;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 case ItemSuffix.Disorder:
    //                 case ItemSuffix.Fatigue:
    //                 case ItemSuffix.Chaos:
    //                 case ItemSuffix.Ether:
    //                     typeIdMod_1 = 0x47;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 // case ItemSuffix.Exhaustion:
    //                 case ItemSuffix.Radiance:
    //                 // case ItemSuffix.Valor:
    //                 case ItemSuffix.Disease:
    //                     typeIdMod_1 = 0x48;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 // case ItemSuffix.Damage:
    //                 case ItemSuffix.Value:
    //                 // case ItemSuffix.Cruelty:
    //                     typeIdMod_1 = 0x49;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                     
    //             }
    //             switch (Suffix)
    //             {
    //                 case ItemSuffix.None:
    //                     break;
    //                 case ItemSuffix.Exhaustion:
    //                 case ItemSuffix.Damage:
    //                 case ItemSuffix.Haste:
    //                 case ItemSuffix.Disorder:
    //                     itemSuffixMod = 0x0;
    //                     break;
    //                 case ItemSuffix.Penetration:
    //                 case ItemSuffix.Decay:
    //                 case ItemSuffix.Distance:
    //                 case ItemSuffix.Fatigue:
    //                 case ItemSuffix.Radiance:
    //                 case ItemSuffix.Value:
    //                     itemSuffixMod = 0x20;
    //                     break;
    //                 case ItemSuffix.Valor:
    //                 case ItemSuffix.Cruelty:
    //                 case ItemSuffix.Speed:
    //                 case ItemSuffix.Chaos:
    //                     itemSuffixMod = 0x80;
    //                     break;
    //                 case ItemSuffix.Instability:
    //                 case ItemSuffix.Range:
    //                 case ItemSuffix.Mastery:
    //                 case ItemSuffix.Ether:
    //                 case ItemSuffix.Disease:
    //                     itemSuffixMod = 0xA0;
    //                     break;
    //                     
    //             }
    //         }
    //         if (ObjectType is GameObjectType.Robe)
    //         {
    //             switch (Suffix)
    //             {
    //                 case ItemSuffix.None:
    //                     break;
    //                 case ItemSuffix.Value:
    //                 case ItemSuffix.Earth:
    //                 case ItemSuffix.Durability:
    //                 case ItemSuffix.Water:
    //                     typeIdMod_1 = 0x44;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 case ItemSuffix.Deflection:
    //                 case ItemSuffix.Air:
    //                 case ItemSuffix.Safety:
    //                 case ItemSuffix.Fire:
    //                     typeIdMod_1 = 0x45;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 case ItemSuffix.Health:
    //                 case ItemSuffix.Ether:
    //                 case ItemSuffix.Life:
    //                 case ItemSuffix.Eclipse:
    //                     typeIdMod_1 = 0x46;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 case ItemSuffix.Meditation:
    //                 case ItemSuffix.Archmage:
    //                 case ItemSuffix.Prana:
    //                 // case ItemSuffix.Durability:
    //                     typeIdMod_1 = 0x47;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 // TODO: actually, these are different perks with different stat reqs
    //                 // case ItemSuffix.Value:
    //                 // case ItemSuffix.Deflection:
    //                 // case ItemSuffix.Durability:
    //                 // case ItemSuffix.Safety:
    //                 //     typeIdMod_1 = 0x48;
    //                 //     typeIdMod_2 = 0x01;
    //                 //     break;
    //                 // case ItemSuffix.Deflection:
    //                 // case ItemSuffix.Health:
    //                 // case ItemSuffix.Safety:
    //                 // case ItemSuffix.Life:
    //                 //     typeIdMod_1 = 0x49;
    //                 //     typeIdMod_2 = 0x01;
    //                 //     break;
    //                 // case ItemSuffix.Health:
    //                 // case ItemSuffix.Meditation:
    //                 // case ItemSuffix.Life:
    //                 // case ItemSuffix.Prana:
    //                 //     typeIdMod_1 = 0x4A;
    //                 //     typeIdMod_2 = 0x01;
    //                 //     break;
    //                 // case ItemSuffix.Meditation:
    //                 // case ItemSuffix.Ether:
    //                 // case ItemSuffix.Prana:
    //                 case ItemSuffix.Dragon:
    //                     typeIdMod_1 = 0x4B;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //             }
    //             switch (Suffix)
    //             {
    //                 case ItemSuffix.None:
    //                     break;
    //                 case ItemSuffix.Value:
    //                 case ItemSuffix.Deflection:
    //                 case ItemSuffix.Health:
    //                 case ItemSuffix.Meditation:
    //                 // case ItemSuffix.Value:
    //                 // case ItemSuffix.Deflection:
    //                 // case ItemSuffix.Health:
    //                 // case ItemSuffix.Meditation:
    //                     itemSuffixMod = 0x0;
    //                     break;
    //                 case ItemSuffix.Earth:
    //                 case ItemSuffix.Air:
    //                 case ItemSuffix.Ether:
    //                 case ItemSuffix.Archmage:
    //                 // case ItemSuffix.Deflection:
    //                 // case ItemSuffix.Health:
    //                 // case ItemSuffix.Meditation:
    //                 // case ItemSuffix.Ether:
    //                     itemSuffixMod = 0x20;
    //                     break;
    //                 case ItemSuffix.Durability:
    //                 case ItemSuffix.Safety:
    //                 case ItemSuffix.Life:
    //                 case ItemSuffix.Prana:
    //                 // case ItemSuffix.Durability:
    //                 // case ItemSuffix.Safety:
    //                 // case ItemSuffix.Life:
    //                 // case ItemSuffix.Prana:
    //                     itemSuffixMod = 0x80;
    //                     break;
    //                 case ItemSuffix.Water:
    //                 case ItemSuffix.Fire:
    //                 case ItemSuffix.Eclipse:
    //                 // case ItemSuffix.Durability:
    //                 // case ItemSuffix.Safety:
    //                 // case ItemSuffix.Life:
    //                 // case ItemSuffix.Prana:
    //                 case ItemSuffix.Dragon:
    //                     itemSuffixMod = 0xA0;
    //                     break;
    //                     
    //             }
    //         }
    //         if (ObjectType is GameObjectType.Bracelet or GameObjectType.Amulet)
    //         {
    //             switch (Suffix)
    //             {
    //                 case ItemSuffix.None:
    //                     break;
    //                 case ItemSuffix.Durability:
    //                 case ItemSuffix.Radiance:
    //                 case ItemSuffix.Absorption:
    //                 case ItemSuffix.Value:
    //                     typeIdMod_1 = 0x44;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 case ItemSuffix.Deflection:
    //                 case ItemSuffix.Damage:
    //                 case ItemSuffix.Safety:
    //                     typeIdMod_1 = 0x45;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 case ItemSuffix.Health:
    //                 case ItemSuffix.Meditation:
    //                     typeIdMod_1 = 0x46;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 case ItemSuffix.Precision:
    //                 case ItemSuffix.Ether:
    //                     typeIdMod_1 = 0x47;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                     
    //             }
    //             switch (Suffix)
    //             {
    //                 case ItemSuffix.None:
    //                     break;
    //                 case ItemSuffix.Durability:
    //                 case ItemSuffix.Deflection:
    //                 case ItemSuffix.Health:
    //                 case ItemSuffix.Precision:
    //                     itemSuffixMod = 0x0;
    //                     break;
    //                 case ItemSuffix.Radiance:
    //                 case ItemSuffix.Damage:
    //                     itemSuffixMod = 0x20;
    //                     break;
    //                 case ItemSuffix.Absorption:
    //                 case ItemSuffix.Safety:
    //                 case ItemSuffix.Meditation:
    //                 case ItemSuffix.Ether:
    //                     itemSuffixMod = 0x80;
    //                     break;
    //                 case ItemSuffix.Value:
    //                     itemSuffixMod = 0xA0;
    //                     break;
    //             }
    //         }
    //         if (ObjectType is GameObjectType.Helmet or GameObjectType.Gloves or GameObjectType.Belt 
    //             or GameObjectType.Pants or GameObjectType.Boots)
    //         {
    //             switch (Suffix)
    //             {
    //                 case ItemSuffix.None:
    //                     break;
    //                 case ItemSuffix.Durability:
    //                 case ItemSuffix.Absorption:
    //                     typeIdMod_1 = 0x44;
    //                     typeIdMod_2 = 0x60;
    //                     break;
    //                 case ItemSuffix.Safety:
    //                 case ItemSuffix.Health:
    //                     typeIdMod_1 = 0x45;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 case ItemSuffix.Meditation:
    //                 case ItemSuffix.Precision:
    //                     typeIdMod_1 = 0x46;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 case ItemSuffix.Ether:
    //                 case ItemSuffix.Value:
    //                     typeIdMod_1 = 0x47;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //             }
    //             switch (Suffix)
    //             {
    //                 case ItemSuffix.None:
    //                     break;
    //                 case ItemSuffix.Durability:
    //                 case ItemSuffix.Safety:
    //                 case ItemSuffix.Meditation:
    //                 case ItemSuffix.Ether:
    //                     itemSuffixMod = 0x0;
    //                     break;
    //                 case ItemSuffix.Absorption:
    //                 case ItemSuffix.Health:
    //                 case ItemSuffix.Precision:
    //                 case ItemSuffix.Value:
    //                     itemSuffixMod = 0x80;
    //                     break;
    //             }
    //         }
    //         if (ObjectType is GameObjectType.Armor or GameObjectType.Shield)
    //         {
    //             switch (Suffix)
    //             {
    //                 case ItemSuffix.None:
    //                     break;
    //                 case ItemSuffix.Valor:
    //                 case ItemSuffix.Meditation:
    //                 case ItemSuffix.Durability:
    //                 case ItemSuffix.Prana:
    //                     typeIdMod_1 = 0x44;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 case ItemSuffix.Absorption:
    //                 case ItemSuffix.Strength:
    //                 case ItemSuffix.Deflection:
    //                 case ItemSuffix.Agility:
    //                     typeIdMod_1 = 0x45;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 case ItemSuffix.Safety:
    //                 case ItemSuffix.Majesty:
    //                 case ItemSuffix.Invincibility:
    //                 case ItemSuffix.Concentration:
    //                     typeIdMod_1 = 0x46;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 case ItemSuffix.Health:
    //                 case ItemSuffix.Earth:
    //                 case ItemSuffix.Life:
    //                 case ItemSuffix.Water:
    //                     typeIdMod_1 = 0x47;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 // case ItemSuffix.Valor:
    //                 case ItemSuffix.Air:
    //                 // case ItemSuffix.Durability:
    //                 case ItemSuffix.Fire:
    //                     typeIdMod_1 = 0x48;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 // case ItemSuffix.Absorption:
    //                 case ItemSuffix.Elements:
    //                 // case ItemSuffix.Deflection:
    //                 case ItemSuffix.Value:
    //                     typeIdMod_1 = 0x49;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //                 // case ItemSuffix.Safety:
    //                 // case ItemSuffix.Strength:
    //                 // case ItemSuffix.Invincibility:
    //                 // case ItemSuffix.Agility:
    //                 //     typeIdMod_1 = 0x4A;
    //                 //     typeIdMod_2 = 0x01;
    //                 //     break;
    //                 // case ItemSuffix.Health:
    //                 // case ItemSuffix.Majesty:
    //                 // case ItemSuffix.Life:
    //                 // case ItemSuffix.Concentration:
    //                     // typeIdMod_1 = 0x4B;
    //                     // typeIdMod_2 = 0x01;
    //                     // break;
    //                 // case ItemSuffix.Valor:
    //                 case ItemSuffix.Integrity:
    //                 // case ItemSuffix.Durability:
    //                     typeIdMod_1 = 0x4C;
    //                     typeIdMod_2 = 0x01;
    //                     break;
    //             }
    //             switch (Suffix)
    //             {
    //                 case ItemSuffix.None:
    //                     break;
    //                 case ItemSuffix.Valor:
    //                 case ItemSuffix.Absorption:
    //                 case ItemSuffix.Safety:
    //                 case ItemSuffix.Health:
    //                 // case ItemSuffix.Valor:
    //                 // case ItemSuffix.Absorption:
    //                 // case ItemSuffix.Safety:
    //                 // case ItemSuffix.Health:
    //                 // case ItemSuffix.Valor:
    //                     itemSuffixMod = 0x0;
    //                     break;
    //                 case ItemSuffix.Meditation:
    //                 case ItemSuffix.Strength:
    //                 case ItemSuffix.Majesty:
    //                 case ItemSuffix.Earth:
    //                 case ItemSuffix.Air:
    //                 case ItemSuffix.Elements:
    //                 // case ItemSuffix.Strength:
    //                 // case ItemSuffix.Majesty:
    //                 case ItemSuffix.Integrity:
    //                     itemSuffixMod = 0x20;
    //                     break;
    //                 case ItemSuffix.Durability:
    //                 case ItemSuffix.Deflection:
    //                 case ItemSuffix.Invincibility:
    //                 case ItemSuffix.Life:
    //                 // case ItemSuffix.Durability:
    //                 // case ItemSuffix.Deflection:
    //                 // case ItemSuffix.Invincibility:
    //                 // case ItemSuffix.Life:
    //                 // case ItemSuffix.Durability:
    //                     itemSuffixMod = 0x80;
    //                     break;
    //                 case ItemSuffix.Prana:
    //                 case ItemSuffix.Agility:
    //                 case ItemSuffix.Concentration:
    //                 case ItemSuffix.Water:
    //                 case ItemSuffix.Fire:
    //                 case ItemSuffix.Value:
    //                 // case ItemSuffix.Agility:
    //                 // case ItemSuffix.Concentration:
    //                     itemSuffixMod = 0xA0;
    //                     break;
    //             }
    //         }
    //             
    //         objid_3 = (byte) (((GameId >> 10) & 0b1111) + itemSuffixMod);
    //             
    //         return new byte[]
    //         {
    //             MinorByte(itemId), MajorByte(itemId), typeid_1, typeid_2, 0x0F, 0x80, 0x84, 0x2E, 0x09, 0x00, 0x00,
    //             0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x91, 0x45, objid_1, objid_2, objid_3, typeIdMod_1,
    //             typeIdMod_2, bagid_1, bagid_2, bagid_3, //0x01, 0x0A, 0x59, 0x00, 0xF0, 0xFF, 0xFF, 0xFF, 0x0F 
    //             0xA0, 0x90, 0x05, 0x00, 0xFF, 0xFF, 0xFF, 0xFF
    //         };
    //     }
    //
    //     if (ObjectType == GameObjectType.Ring)
    //     {
    //         byte ringTypeId_1 = 0;
    //         byte ringTypeId_2 = 0;
    //         byte itemSuffixMod = 0;
    //
    //         switch (Suffix)
    //         {
    //             case ItemSuffix.Durability:
    //             case ItemSuffix.Precision:
    //             case ItemSuffix.Absorption:
    //             case ItemSuffix.Strength:
    //                 ringTypeId_1 = 0x44;
    //                 ringTypeId_2 = 0x01;
    //
    //                 break;
    //             case ItemSuffix.Accuracy:
    //             case ItemSuffix.Agility:
    //             case ItemSuffix.Safety:
    //             case ItemSuffix.Health:
    //                 ringTypeId_1 = 0x45;
    //                 ringTypeId_2 = 0x01;
    //
    //                 break;
    //             case ItemSuffix.Earth:
    //             case ItemSuffix.Endurance:
    //             case ItemSuffix.Life:
    //             case ItemSuffix.Meditation:
    //                 ringTypeId_1 = 0x46;
    //                 ringTypeId_2 = 0x01;
    //
    //                 break;
    //             case ItemSuffix.Air:
    //             case ItemSuffix.Water:
    //             case ItemSuffix.Ether:
    //                 ringTypeId_1 = 0x47;
    //                 ringTypeId_2 = 0x01;
    //
    //                 break;
    //             case ItemSuffix.Fire:
    //             case ItemSuffix.Value:
    //                 // case ItemSuffix.Absorption:
    //                 // case ItemSuffix.Durability:
    //                 ringTypeId_1 = 0x48;
    //                 ringTypeId_2 = 0x01;
    //
    //                 break;
    //             
    //             case ItemSuffix.Prana:
    //                 ringTypeId_1 = 0x17;
    //                 ringTypeId_2 = 0x60;
    //                 break;
    //                 
    //             default:
    //                 Console.WriteLine($"Wrong suffix {Enum.GetName(typeof(ItemSuffix), Suffix)}");
    //
    //                 break;
    //         }
    //
    //         switch (Suffix)
    //         {
    //             case ItemSuffix.Durability:
    //             case ItemSuffix.Safety:
    //             case ItemSuffix.Life:
    //             case ItemSuffix.Prana:
    //                 itemSuffixMod = 0x0;
    //
    //                 break;
    //             case ItemSuffix.Precision:
    //             case ItemSuffix.Agility:
    //             case ItemSuffix.Endurance:
    //             case ItemSuffix.Water:
    //             case ItemSuffix.Fire:
    //                 itemSuffixMod = 0x20;
    //
    //                 break;
    //             case ItemSuffix.Absorption:
    //             case ItemSuffix.Health:
    //             case ItemSuffix.Meditation:
    //             case ItemSuffix.Ether:
    //                 itemSuffixMod = 0x80;
    //
    //                 break;
    //             case ItemSuffix.Strength:
    //             case ItemSuffix.Accuracy:
    //             case ItemSuffix.Earth:
    //             case ItemSuffix.Air:
    //             case ItemSuffix.Value:
    //                 itemSuffixMod = 0xA0;
    //
    //                 break;
    //         }
    //
    //         objid_3 = (byte) (((GameId >> 10) & 0b1111) + itemSuffixMod);
    //         var isFullStat = Suffix is ItemSuffix.Strength or ItemSuffix.Agility or ItemSuffix.Accuracy
    //             or ItemSuffix.Endurance or ItemSuffix.Earth or ItemSuffix.Water or ItemSuffix.Air or ItemSuffix.Fire;
    //
    //         // technically, live server has different values per suffix group for 0x98 0x1A at the end but these
    //         // seem to be safe to ignore
    //         if (isFullStat && !bitShiftForRings)
    //         {
    //             bagid_1 = (byte) (bagid_1 + 0b00110);
    //         }
    //
    //         var fullStatRingsBytes = new byte[]
    //         {
    //             0x00, 0x0A, 0x59, 0x00, 0xF0, 0xFF, 0xFF, 0xFF, 0x5F, 0x78, 0x07, 0xB5, 0xBB, 0x2F, 0xB2, 0xB4, 0xB0,
    //             0x36, 0xB9, 0x34, 0xB7, 0xB3
    //         };
    //         var halfStatRingBytes = new byte[] // shifted by 4 bits
    //         {
    //             0xA0, 0x90, 0x05, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x85, 0x77, 0x50, 0xBB, 0xFB, 0x22, 0x4B, 0x0B, 0x6B,
    //             0x93, 0x4B, 0x73, 0x3B, 0x83
    //         };
    //         var result = new List<byte>(new byte[]
    //         {
    //             MinorByte(itemId), MajorByte(itemId), 0xE0, 0x8B, 0x0F, 0x80, 0x84, 0x2E, 0x09, 0x00, 0x00, 0x00, 0x00,
    //             0x00, 0x00, 0x00, 0x00, 0x40, 0x91, 0x45, objid_1, objid_2, objid_3, ringTypeId_1, ringTypeId_2,
    //             bagid_1, bagid_2, bagid_3
    //         });
    //
    //         result.AddRange(isFullStat
    //             ? fullStatRingsBytes
    //             : halfStatRingBytes);
    //
    //         var safety = new byte[] { 0x91, 0x01, 0x00 };
    //         var water = new byte[] { 0x18, 0x1A, 0x00 };
    //         var prana = new byte[] { 0xB1, 0x01, 0x00 };
    //         var health = new byte[] { 0x99, 0x01, 0x00 };
    //
    //         result.AddRange(Suffix == ItemSuffix.Prana ? prana : health);
    //         return result.ToArray();
    //     }
    //
    //     Console.WriteLine($"Unhandled game object: {ToDebugString()}");
    //
    //     return Array.Empty<byte>();
    // }

    public bool IsTierVisible()
    {
        return ObjectKind is GameObjectKind.Armor or GameObjectKind.Axe or GameObjectKind.Guild
            or GameObjectKind.Magical or GameObjectKind.Powder or GameObjectKind.Quest or GameObjectKind.Sword
            or GameObjectKind.Unique or GameObjectKind.Armor_New or GameObjectKind.Armor_Old or GameObjectKind.Axe_New
            or GameObjectKind.Crossbow_New or GameObjectKind.Magical_New or GameObjectKind.MantraBlack
            or GameObjectKind.MantraWhite or GameObjectKind.Sword_New
            && ObjectType is not GameObjectType.Ear;
    }
}
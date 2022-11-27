public static class GameObjectDataHelper
{
    // public static bool firstTypeRolled = false;
    public static HashSet<ItemSuffix> RingSuffixes = new ()
    {
        ItemSuffix.Health,
        ItemSuffix.Ether,
        ItemSuffix.Accuracy,
        ItemSuffix.Air,
        ItemSuffix.Durability,
        ItemSuffix.Life,
        ItemSuffix.Precision,
        ItemSuffix.Endurance,
        ItemSuffix.Fire,
        ItemSuffix.Absorption,
        ItemSuffix.Meditation,
        ItemSuffix.Strength,
        ItemSuffix.Earth,
        ItemSuffix.Value,
        ItemSuffix.Safety,
        ItemSuffix.Prana,
        ItemSuffix.Agility,
        ItemSuffix.Water,
    };
    public static GameObjectKind GetKindBySphereName(string sphName)
    {
        switch (sphName)
        {
            case "alch": return GameObjectKind.Alchemy;
            case "arbs": return GameObjectKind.Crossbow;
            case "arbs_n": return GameObjectKind.Crossbow_New;
            case "armor": return GameObjectKind.Armor;
            case "armor_n": return GameObjectKind.Armor_New;
            case "armor_o": return GameObjectKind.Armor_Old;
            case "axes": return GameObjectKind.Axe;
            case "axes_n": return GameObjectKind.Axe_New;
            case "fb": return GameObjectKind.Powder;
            case "guilds": return GameObjectKind.Guild;
            case "magdef": return GameObjectKind.Magical;
            case "magdef_n": return GameObjectKind.Magical_New;
            case "mantrab": return GameObjectKind.MantraBlack;
            case "mantraw": return GameObjectKind.MantraWhite;
            case "maps": return GameObjectKind.Map;
            case "monst": return GameObjectKind.Monster;
            case "quest": return GameObjectKind.Quest;
            case "swords": return GameObjectKind.Sword;
            case "swords_n": return GameObjectKind.Sword_New;
            case "unique": return GameObjectKind.Unique;
            case "pref": return GameObjectKind.Pref;
            default:
                Console.WriteLine($"Unknown game object type: {sphName}");
                return GameObjectKind.Unknown;
        }
    }

    public static GameObjectType GetTypeBySphereName(string sphName)
    {
        switch (sphName)
        {
            case "A": return GameObjectType.Pref_A;
            case "al_flower": return GameObjectType.Flower; 
            case "al_metal": return GameObjectType.Metal;
            case "al_mineral": return GameObjectType.Mineral;
            case "ar_amulet": return GameObjectType.Amulet;
            case "ar_amuletu": return GameObjectType.Amulet_Unique;
            case "ar_armor": return GameObjectType.Armor;
            case "ar_armor2": return GameObjectType.Robe;
            case "ar_armor2f": return GameObjectType.Robe_Quest;
            case "ar_armor2u": return GameObjectType.Robe_Unique;
            case "ar_armorf": return GameObjectType.Armor_Quest;
            case "ar_armoru": return GameObjectType.Armor_Unique;
            case "ar_belt": return GameObjectType.Belt;
            case "ar_beltf": return GameObjectType.Belt_Quest;
            case "ar_beltu": return GameObjectType.Belt_Unique;
            case "ar_bracelet": return GameObjectType.Bracelet;
            case "ar_braceletu": return GameObjectType.Bracelet_Unique;
            case "ar_gloves": return GameObjectType.Gloves;
            case "ar_glovesf": return GameObjectType.Gloves_Quest;
            case "ar_glovesu": return GameObjectType.Gloves_Unique;
            case "ar_helm": return GameObjectType.Helmet;
            case "ar_helm_pr": return GameObjectType.Helmet_Premium;
            case "ar_helmf": return GameObjectType.Helmet_Quest;
            case "ar_helmu": return GameObjectType.Helmet_Unique;
            case "ar_pants": return GameObjectType.Pants;
            case "ar_pantsf": return GameObjectType.Pants_Quest;
            case "ar_pantsu": return GameObjectType.Pants_Unique;
            case "ar_ring": return GameObjectType.Ring;
            case "ar_ring_s": return GameObjectType.Ring_Special;
            case "ar_ringu": return GameObjectType.Ring_Unique;
            case "ar_shield": return GameObjectType.Shield;
            case "ar_shieldf": return GameObjectType.Shield_Quest;
            case "ar_shieldu": return GameObjectType.Shield_Unique;
            case "ar_shoes": return GameObjectType.Shoes;
            case "ar_shoesf": return GameObjectType.Shoes_Quest;
            case "ar_shoesu": return GameObjectType.Shoes_Unique;
            case "B": return GameObjectType.Pref_B;
            case "C": return GameObjectType.Pref_C;
            case "crystal": return GameObjectType.Castle_Crystal;
            case "cs_guard": return GameObjectType.Castle_Stone;
            case "ct_bagd": return GameObjectType.Guild_Bag;
            case "D": return GameObjectType.Pref_D;
            case "E": return GameObjectType.Pref_E;
            case "F": return GameObjectType.Pref_F;
            case "flag": return GameObjectType.Flag;
            case "G": return GameObjectType.Pref_G;
            case "guild": return GameObjectType.Guild;
            case "H": return GameObjectType.Pref_H;
            case "I": return GameObjectType.Pref_I;
            case "item_letter": return GameObjectType.Letter;
            case "lottery": return GameObjectType.Lottery;
            case "mg_mantrab": return GameObjectType.MantraBlack;
            case "mg_mantraw": return GameObjectType.MantraWhite;
            case "monster": return GameObjectType.Monster;
            case "monsterc": return GameObjectType.Monster_Castle_Stone;
            case "monsterd": return GameObjectType.Monster_Event;
            case "monsterdf": return GameObjectType.Monster_Event_Flying;
            case "monsterf": return GameObjectType.Monster_Flying;
            case "monsterh": return GameObjectType.Monster_Tower_Spirit;
            case "monsters": return GameObjectType.Monster_Castle_Spirit;
            case "pw_elixir": return GameObjectType.Elixir_Castle;
            case "pw_elixir1": return GameObjectType.Elixir_Trap;
            case "pw_fb02": return GameObjectType.Powder;
            case "pw_fb03": return GameObjectType.Powder_Area;
            case "pw_fb04": return GameObjectType.Powder_Event;
            case "pw_fb05": return GameObjectType.Powder_Guild;
            case "scroll": return GameObjectType.Scroll;
            case "specab": return GameObjectType.Special;
            case "specab_ba": return GameObjectType.Special_BA;
            case "specab_ca": return GameObjectType.Special_CA;
            case "specab_ea": return GameObjectType.Special_EA;
            case "specab_ga": return GameObjectType.Special_GA;
            case "specab_gb": return GameObjectType.Special_GB;
            case "specab_ha": return GameObjectType.Special_HA;
            case "specab_ic": return GameObjectType.Special_IC;
            case "specab_ma": return GameObjectType.Special_MA;
            case "specab_mb": return GameObjectType.Special_MB;
            case "specab_mc": return GameObjectType.Special_MC;
            case "specab_na": return GameObjectType.Special_NA;
            case "specab_nb": return GameObjectType.Special_NB;
            case "specab_nc": return GameObjectType.Special_NC;
            case "st_key2": return GameObjectType.Key;
            case "st_map": return GameObjectType.Map;
            case "st_string": return GameObjectType.Ear_String;
            case "vn_crystq": return GameObjectType.Crystal;
            case "wp_arbalest1": return GameObjectType.Crossbow;
            case "wp_arbalest1f": return GameObjectType.Crossbow_Quest;
            case "wp_axe1": return GameObjectType.Axe;
            case "wp_axe1f": return GameObjectType.Axe_Quest;
            case "wp_sword1": return GameObjectType.Sword;
            case "wp_sword1f": return GameObjectType.Sword_Quest;
            case "wp_sword1u": return GameObjectType.Sword_Unique;
            case "x2_degree": return GameObjectType.X2_Degree;
            case "x2_tit_degr": return GameObjectType.X2_Both;
            case "x2_titul": return GameObjectType.X2_Title;
            case "Z": return GameObjectType.Pref_Z;
            case "st_loot": return GameObjectType.Ear;
            case "item_bead": return GameObjectType.Bead;
            case "packet": return GameObjectType.Packet;
            default:
                Console.WriteLine($"Unknown GameObjectType: {sphName}");
                return GameObjectType.Unknown;
        }
    }

    public static string ToRomanTierLiteral(this SphGameObject gameObject)
    {
        if (!gameObject.IsTierVisible())
        {
            return string.Empty;
        }
        return gameObject.Tier switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "IV",
            5 => "V",
            6 => "VI",
            7 => "VII",
            8 => "VIII",
            9 => "IX",
            10 => "X",
            11 => "XI",
            12 => "XII",
            13 => "XIII",
            14 => "XIV",
            15 => "XV",
            _ => string.Empty
        };
    }
    //
    // public static SphGameObject GetRandomObjectData(int titleLevelMinusOne, int gameIdOverride = -1)
    // {
    //     SphGameObject item;
    //     if (gameIdOverride != -1)
    //     {
    //         item = MainServer.GameObjectDataDb.First(x => x.Value.GameId == gameIdOverride).Value;
    //     }
    //     else
    //     {
    //         var tierFilter = Math.Min(titleLevelMinusOne, 74) / 5 + 1;
    //         var typeFilter = new HashSet<GameObjectType>
    //         {
    //             GameObjectType.Flower,
    //             GameObjectType.Metal,
    //             GameObjectType.Mineral,
    //             GameObjectType.Amulet,
    //             GameObjectType.Armor,
    //             GameObjectType.Robe,
    //             GameObjectType.Belt,
    //             GameObjectType.Bracelet,
    //             GameObjectType.Gloves,
    //             GameObjectType.Helmet,
    //             GameObjectType.Pants,
    //             GameObjectType.Ring,
    //             GameObjectType.Shield,
    //             GameObjectType.Shoes,
    //             // Flag,
    //             // Guild,
    //             GameObjectType.MantraBlack,
    //             GameObjectType.MantraWhite,
    //             GameObjectType.Elixir_Castle,
    //             GameObjectType.Elixir_Trap,
    //             GameObjectType.Powder,
    //             GameObjectType.Powder_Area,
    //             GameObjectType.Crossbow,
    //             GameObjectType.Axe,
    //             GameObjectType.Sword,
    //         };
    //         var overrideFilter = new HashSet<GameObjectType>
    //         {
    //             GameObjectType.Ring,
    //             GameObjectType.MantraBlack,
    //             GameObjectType.MantraWhite,
    //             GameObjectType.Mineral
    //         };
    //
    //         if (firstTypeRolled)
    //         {
    //             // overrideFilter = new HashSet<GameObjectType>
    //             // {
    //             //     GameObjectType.Mineral
    //             // };
    //         }
    //
    //         firstTypeRolled = !firstTypeRolled;
    //
    //         typeFilter = overrideFilter.Count > 0 ? overrideFilter : typeFilter;
    //
    //         var kindFilter = new HashSet<GameObjectKind>
    //         {
    //             GameObjectKind.Alchemy,
    //             GameObjectKind.Crossbow_New,
    //             GameObjectKind.Armor_New,
    //             GameObjectKind.Armor_Old, // "Old" robes only
    //             GameObjectKind.Axe_New,
    //             GameObjectKind.Powder,
    //             GameObjectKind.Magical_New,
    //             GameObjectKind.MantraBlack,
    //             GameObjectKind.MantraWhite,
    //             GameObjectKind.Sword_New,
    //         };
    //
    //         var tierAgnosticTypes = new HashSet<GameObjectType>
    //         {
    //             GameObjectType.Flower,
    //             GameObjectType.Metal,
    //             GameObjectType.Mineral,
    //         };
    //
    //         var lootPool = MainServer.GameObjectDataDb
    //             .Where(x =>
    //                 kindFilter.Contains(x.Value.ObjectKind) && typeFilter.Contains(x.Value.ObjectType)
    //                                                         && (x.Value.Tier == tierFilter
    //                                                             || tierAgnosticTypes.Contains(x.Value.ObjectType)))
    //             .Select(y => y.Value)
    //             .ToList();
    //
    //         var random = MainServer.Rng.RandiRange(0, lootPool.Count - 1);
    //         item = lootPool.ElementAt(random);
    //     }
    //
    //     var noSuffix = false;
    //     var suffixFilter = new SortedSet<ItemSuffix> { ItemSuffix.None };
    //         
    //     if (item.ObjectType is GameObjectType.Ring)
    //     { 
    //         suffixFilter = new SortedSet<ItemSuffix>
    //         {
    //             ItemSuffix.Health,
    //             ItemSuffix.Accuracy,
    //             ItemSuffix.Air,
    //             ItemSuffix.Durability,
    //             ItemSuffix.Life,
    //             ItemSuffix.Endurance,
    //             ItemSuffix.Fire,
    //             ItemSuffix.Absorption,
    //             ItemSuffix.Meditation,
    //             ItemSuffix.Strength,
    //             ItemSuffix.Earth,
    //             ItemSuffix.Safety,
    //             ItemSuffix.Prana,
    //             ItemSuffix.Agility,
    //             ItemSuffix.Water,
    //             ItemSuffix.Value,
    //             ItemSuffix.Precision,
    //             ItemSuffix.Ether,
    //         };
    //         item.Suffix = suffixFilter.ElementAt(MainServer.Rng.RandiRange(0, suffixFilter.Count - 1));
    //     }
    //
    //     // Rings should always have a suffix
    //     if (noSuffix)
    //     {
    //         return item;
    //     }
    //     if (item.ObjectType is GameObjectType.Sword or GameObjectType.Axe)
    //     {
    //         suffixFilter = new SortedSet<ItemSuffix>
    //         {
    //             ItemSuffix.Cruelty,
    //             ItemSuffix.Chaos,
    //             ItemSuffix.Instability,
    //             ItemSuffix.Devastation,
    //             ItemSuffix.Value,
    //             ItemSuffix.Exhaustion,
    //             ItemSuffix.Haste,
    //             ItemSuffix.Ether,
    //             ItemSuffix.Range,
    //             ItemSuffix.Weakness,
    //             ItemSuffix.Valor,
    //             ItemSuffix.Speed,
    //             ItemSuffix.Fatigue,
    //             ItemSuffix.Distance,
    //             ItemSuffix.Penetration,
    //             ItemSuffix.Damage,
    //             ItemSuffix.Disorder,
    //             ItemSuffix.Disease,
    //             ItemSuffix.Decay,
    //             ItemSuffix.Interdict,
    //         };
    //     }
    //     if (item.ObjectType is GameObjectType.Crossbow)
    //     {
    //         suffixFilter = new SortedSet<ItemSuffix>
    //         {
    //             ItemSuffix.Cruelty,
    //             ItemSuffix.Chaos,
    //             ItemSuffix.Instability,
    //             ItemSuffix.Value,
    //             ItemSuffix.Exhaustion,
    //             ItemSuffix.Haste,
    //             ItemSuffix.Ether,
    //             ItemSuffix.Range,
    //             ItemSuffix.Valor,
    //             ItemSuffix.Speed,
    //             ItemSuffix.Fatigue,
    //             ItemSuffix.Distance,
    //             ItemSuffix.Penetration,
    //             ItemSuffix.Damage,
    //             ItemSuffix.Disorder,
    //             ItemSuffix.Disease,
    //             ItemSuffix.Decay,
    //             ItemSuffix.Mastery,
    //             ItemSuffix.Radiance
    //         };
    //     }
    //     if (item.ObjectType is GameObjectType.Robe)
    //     {
    //         suffixFilter = new SortedSet<ItemSuffix>
    //         {
    //             ItemSuffix.Safety,
    //             ItemSuffix.Prana,
    //             ItemSuffix.Fire,
    //             ItemSuffix.Durability,
    //             ItemSuffix.Life,
    //             ItemSuffix.Dragon,
    //             ItemSuffix.Value,
    //             ItemSuffix.Health,
    //             ItemSuffix.Earth,
    //             ItemSuffix.Ether,
    //             ItemSuffix.Deflection,
    //             ItemSuffix.Meditation,
    //             ItemSuffix.Water,
    //             ItemSuffix.Eclipse,
    //             ItemSuffix.Air,
    //             ItemSuffix.Archmage,
    //             // ItemSuffix.Durability_Old,
    //             // ItemSuffix.Life_Old,
    //             // ItemSuffix.Safety_Old,
    //             // ItemSuffix.Prana_Old,
    //             // ItemSuffix.Deflection_Old,
    //             // ItemSuffix.Meditation_Old,
    //             // ItemSuffix.Health_Old,
    //             // ItemSuffix.Ether_Old,
    //         };
    //     }
    //     if (item.ObjectType is GameObjectType.Bracelet or GameObjectType.Amulet)
    //     {
    //         suffixFilter = new SortedSet<ItemSuffix>
    //         {
    //             ItemSuffix.Safety,
    //             ItemSuffix.Ether,
    //             ItemSuffix.Durability,
    //             ItemSuffix.Health,
    //             ItemSuffix.Radiance,
    //             ItemSuffix.Absorption,
    //             ItemSuffix.Meditation,
    //             ItemSuffix.Value,
    //             ItemSuffix.Deflection,
    //             ItemSuffix.Precision,
    //             ItemSuffix.Damage,
    //         };
    //     }
    //     if (item.ObjectType is GameObjectType.Helmet or GameObjectType.Gloves or GameObjectType.Belt 
    //         or GameObjectType.Pants or GameObjectType.Shoes)
    //     {
    //         suffixFilter = new SortedSet<ItemSuffix>
    //         {
    //             ItemSuffix.Health,
    //             ItemSuffix.Value,
    //             ItemSuffix.Durability,
    //             ItemSuffix.Meditation,
    //             ItemSuffix.Absorption,
    //             ItemSuffix.Precision,
    //             ItemSuffix.Safety,
    //             ItemSuffix.Ether,
    //         };
    //     }
    //     if (item.ObjectType is GameObjectType.Armor or GameObjectType.Shield)
    //     {
    //         suffixFilter = new SortedSet<ItemSuffix>
    //         {// for shields Elements is "Old" (e.g. +68 at 12 rank)
    //             ItemSuffix.Deflection,
    //             ItemSuffix.Life,
    //             ItemSuffix.Agility,
    //             ItemSuffix.Water,
    //             ItemSuffix.Value,
    //             ItemSuffix.Concentration,
    //             ItemSuffix.Valor,
    //             ItemSuffix.Safety,
    //             ItemSuffix.Meditation,
    //             ItemSuffix.Air,
    //             ItemSuffix.Strength,
    //             ItemSuffix.Integrity,
    //             ItemSuffix.Durability,
    //             ItemSuffix.Invincibility,
    //             ItemSuffix.Prana,
    //             ItemSuffix.Fire,
    //             ItemSuffix.Agility,
    //             ItemSuffix.Absorption,
    //             ItemSuffix.Health,
    //             ItemSuffix.Strength,
    //             ItemSuffix.Earth,
    //             ItemSuffix.Elements,
    //             ItemSuffix.Majesty,
    //             // ItemSuffix.Concentration_Old,
    //             // ItemSuffix.Majesty_Old,
    //             // ItemSuffix.Agility_Old,
    //             // ItemSuffix.Strength_Old,
    //             // ItemSuffix.Elements_Old,
    //             // ItemSuffix.Elements_New,
    //         };
    //     }
    //
    //     if (item.ObjectType is GameObjectType.Powder or GameObjectType.Powder_Area or GameObjectType.Elixir_Castle 
    //         or GameObjectType.Elixir_Trap)
    //     {
    //         item.ItemCount =  MainServer.Rng.RandiRange(3, 19);
    //     }
    //
    //     item.Suffix = suffixFilter.ElementAt(MainServer.Rng.RandiRange(0, suffixFilter.Count - 1));
    //     return item;
    // }
}
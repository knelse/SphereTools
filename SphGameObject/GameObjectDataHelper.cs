using SuffixToLocaleMap = System.Collections.Generic.Dictionary<ItemSuffix, SuffixValueWithLocale>;

public static class GameObjectDataHelper
{
    public static readonly HashSet<GameObjectType> MaterialsPowdersElixirs = new()
    {
        GameObjectType.Flower, 
        GameObjectType.Metal, 
        GameObjectType.Mineral, 
        GameObjectType.Powder,
        GameObjectType.Powder_Area, 
        GameObjectType.Elixir_Castle, 
        GameObjectType.Elixir_Trap
    };

    public static readonly HashSet<GameObjectType> RegularWeaponsAndArmor = new()
    {
        GameObjectType.Crossbow, GameObjectType.Axe, GameObjectType.Sword, GameObjectType.Amulet, GameObjectType.Armor,
        GameObjectType.Belt, GameObjectType.Bracelet, GameObjectType.Gloves, GameObjectType.Helmet,
        GameObjectType.Pants, GameObjectType.Shield, GameObjectType.Boots, GameObjectType.Robe
    };
    public static readonly HashSet<GameObjectType> WeaponsAndArmor = new()
    {
        GameObjectType.Amulet,
        GameObjectType.Armor,
        GameObjectType.Axe,
        GameObjectType.Belt,
        GameObjectType.Boots,
        GameObjectType.Bracelet,
        GameObjectType.Crossbow,
        GameObjectType.Gloves,
        GameObjectType.Helmet,
        GameObjectType.Pants,
        GameObjectType.Robe,
        GameObjectType.Shield,
        GameObjectType.Sword,
        GameObjectType.Amulet_Unique,
        GameObjectType.Armor_Unique,
        GameObjectType.Belt_Unique,
        GameObjectType.Boots_Unique,
        GameObjectType.Bracelet_Unique,
        GameObjectType.Gloves_Unique,
        GameObjectType.Helmet_Premium,
        GameObjectType.Helmet_Unique,
        GameObjectType.Pants_Unique,
        GameObjectType.Robe_Unique,
        GameObjectType.Shield_Unique,
        GameObjectType.Sword_Unique
    };

    public static readonly HashSet<GameObjectType> Mantras = new()
    {
        GameObjectType.MantraBlack,
        GameObjectType.MantraWhite
    };

    public static readonly HashSet<GameObjectType> AlchemyMaterials = new()
    {
        GameObjectType.Metal,
        GameObjectType.Flower,
        GameObjectType.Mineral
    };

    public static readonly HashSet<GameObjectType> Powders = new()
    {
        GameObjectType.Powder,
        GameObjectType.Powder_Area,
        GameObjectType.Powder_Event,
        GameObjectType.Powder_Guild,
        GameObjectType.Elixir_Castle,
        GameObjectType.Elixir_Trap
    };
    
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
            case "ar_shoes": return GameObjectType.Boots;
            case "ar_shoesf": return GameObjectType.Boots_Quest;
            case "ar_shoesu": return GameObjectType.Boots_Unique;
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

    public static readonly Dictionary<ItemSuffix, SuffixValueWithLocale> SuffixesAmuletBracelet = new()
    {
        [ItemSuffix.Durability] =
            new SuffixValueWithLocale(64, new Dictionary<Locale, string> { [Locale.Russian] = "прочности" }),
        [ItemSuffix.Radiance] =
            new SuffixValueWithLocale(1090, new Dictionary<Locale, string> { [Locale.Russian] = "сияния" }),
        [ItemSuffix.Absorption] =
            new SuffixValueWithLocale(72, new Dictionary<Locale, string> { [Locale.Russian] = "поглощения" }),
        [ItemSuffix.Value] =
            new SuffixValueWithLocale(1098, new Dictionary<Locale, string> { [Locale.Russian] = "ценности" }),
        [ItemSuffix.Deflection] =
            new SuffixValueWithLocale(80, new Dictionary<Locale, string> { [Locale.Russian] = "отклонения" }),
        [ItemSuffix.Damage] =
            new SuffixValueWithLocale(1106, new Dictionary<Locale, string> { [Locale.Russian] = "урона" }),
        [ItemSuffix.Safety] =
            new SuffixValueWithLocale(88, new Dictionary<Locale, string> { [Locale.Russian] = "безопасности" }),
        [ItemSuffix.Health] =
            new SuffixValueWithLocale(96, new Dictionary<Locale, string> { [Locale.Russian] = "здоровья" }),
        [ItemSuffix.Meditation] =
            new SuffixValueWithLocale(104, new Dictionary<Locale, string> { [Locale.Russian] = "медитации" }),
        [ItemSuffix.Precision] =
            new SuffixValueWithLocale(112, new Dictionary<Locale, string> { [Locale.Russian] = "точности" }),
        [ItemSuffix.Ether] =
            new SuffixValueWithLocale(120, new Dictionary<Locale, string> { [Locale.Russian] = "эфира" }),
    };

    public static readonly Dictionary<ItemSuffix, SuffixValueWithLocale> SuffixesSwordAxes = new()
    {
        [ItemSuffix.Exhaustion] =
            new SuffixValueWithLocale(64, new Dictionary<Locale, string> { [Locale.Russian] = "истощения" }),
        [ItemSuffix.Ether] =
            new SuffixValueWithLocale(1090, new Dictionary<Locale, string> { [Locale.Russian] = "эфира" }),
        [ItemSuffix.Valor] =
            new SuffixValueWithLocale(72, new Dictionary<Locale, string> { [Locale.Russian] = "доблести" }),
        [ItemSuffix.Fatigue] =
            new SuffixValueWithLocale(1098, new Dictionary<Locale, string> { [Locale.Russian] = "усталости" }),
        [ItemSuffix.Damage] =
            new SuffixValueWithLocale(80, new Dictionary<Locale, string> { [Locale.Russian] = "урона" }),
        [ItemSuffix.Disease] =
            new SuffixValueWithLocale(1106, new Dictionary<Locale, string> { [Locale.Russian] = "болезни" }),
        [ItemSuffix.Cruelty] =
            new SuffixValueWithLocale(88, new Dictionary<Locale, string> { [Locale.Russian] = "жестокости" }),
        [ItemSuffix.Instability] =
            new SuffixValueWithLocale(1114, new Dictionary<Locale, string> { [Locale.Russian] = "неустойчивости" }),
        [ItemSuffix.Haste] =
            new SuffixValueWithLocale(96, new Dictionary<Locale, string> { [Locale.Russian] = "спешки" }),
        [ItemSuffix.Range] =
            new SuffixValueWithLocale(1122, new Dictionary<Locale, string> { [Locale.Russian] = "расстояния" }),
        [ItemSuffix.Speed] =
            new SuffixValueWithLocale(104, new Dictionary<Locale, string> { [Locale.Russian] = "скорости" }),
        [ItemSuffix.Distance] =
            new SuffixValueWithLocale(1130, new Dictionary<Locale, string> { [Locale.Russian] = "дистанции" }),
        [ItemSuffix.Disorder] =
            new SuffixValueWithLocale(112, new Dictionary<Locale, string> { [Locale.Russian] = "беспорядка" }),
        [ItemSuffix.Decay] =
            new SuffixValueWithLocale(1138, new Dictionary<Locale, string> { [Locale.Russian] = "разложения" }),
        [ItemSuffix.Chaos] =
            new SuffixValueWithLocale(120, new Dictionary<Locale, string> { [Locale.Russian] = "хаоса" }),
        [ItemSuffix.Devastation] =
            new SuffixValueWithLocale(1146, new Dictionary<Locale, string> { [Locale.Russian] = "опустошения" }),
        // [ItemSuffix.Exhaustion] =
        //     new SuffixValueWithLocale(128, new Dictionary<Locale, string> { [Locale.Russian] = "истощения" }),
        [ItemSuffix.Weakness] =
            new SuffixValueWithLocale(1154, new Dictionary<Locale, string> { [Locale.Russian] = "слабости" }),
        // [ItemSuffix.Valor] =
        //     new SuffixValueWithLocale(136, new Dictionary<Locale, string> { [Locale.Russian] = "доблести" }),
        [ItemSuffix.Penetration] =
            new SuffixValueWithLocale(1162, new Dictionary<Locale, string> { [Locale.Russian] = "проникновения" }),
        // [ItemSuffix.Damage] =
        //     new SuffixValueWithLocale(144, new Dictionary<Locale, string> { [Locale.Russian] = "урона" }),
        [ItemSuffix.Interdict] =
            new SuffixValueWithLocale(1170, new Dictionary<Locale, string> { [Locale.Russian] = "эапрета" }),
        // [ItemSuffix.Cruelty] =
        //     new SuffixValueWithLocale(152, new Dictionary<Locale, string> { [Locale.Russian] = "жестокости" }),
        [ItemSuffix.Value] =
            new SuffixValueWithLocale(1178, new Dictionary<Locale, string> { [Locale.Russian] = "ценности" }),
    };

    public static readonly Dictionary<ItemSuffix, SuffixValueWithLocale> SuffixesCrossbows = new()
    {
        [ItemSuffix.Exhaustion] =
            new SuffixValueWithLocale(64, new Dictionary<Locale, string> { [Locale.Russian] = "истощения" }),
        [ItemSuffix.Penetration] =
            new SuffixValueWithLocale(1090, new Dictionary<Locale, string> { [Locale.Russian] = "проникновения" }),
        [ItemSuffix.Valor] =
            new SuffixValueWithLocale(72, new Dictionary<Locale, string> { [Locale.Russian] = "доблести" }),
        [ItemSuffix.Instability] =
            new SuffixValueWithLocale(1098, new Dictionary<Locale, string> { [Locale.Russian] = "неустойчивости" }),
        [ItemSuffix.Damage] =
            new SuffixValueWithLocale(80, new Dictionary<Locale, string> { [Locale.Russian] = "урона" }),
        [ItemSuffix.Decay] =
            new SuffixValueWithLocale(1106, new Dictionary<Locale, string> { [Locale.Russian] = "разложения" }),
        [ItemSuffix.Cruelty] =
            new SuffixValueWithLocale(88, new Dictionary<Locale, string> { [Locale.Russian] = "жестокости" }),
        [ItemSuffix.Range] =
            new SuffixValueWithLocale(1114, new Dictionary<Locale, string> { [Locale.Russian] = "расстояния" }),
        [ItemSuffix.Haste] =
            new SuffixValueWithLocale(96, new Dictionary<Locale, string> { [Locale.Russian] = "спешки" }),
        [ItemSuffix.Distance] =
            new SuffixValueWithLocale(1122, new Dictionary<Locale, string> { [Locale.Russian] = "дистанции" }),
        [ItemSuffix.Speed] =
            new SuffixValueWithLocale(104, new Dictionary<Locale, string> { [Locale.Russian] = "скорости" }),
        [ItemSuffix.Mastery] =
            new SuffixValueWithLocale(1130, new Dictionary<Locale, string> { [Locale.Russian] = "мастерства" }),
        [ItemSuffix.Disorder] =
            new SuffixValueWithLocale(112, new Dictionary<Locale, string> { [Locale.Russian] = "беспорядка" }),
        [ItemSuffix.Fatigue] =
            new SuffixValueWithLocale(1138, new Dictionary<Locale, string> { [Locale.Russian] = "усталости" }),
        [ItemSuffix.Chaos] =
            new SuffixValueWithLocale(120, new Dictionary<Locale, string> { [Locale.Russian] = "хаоса" }),
        [ItemSuffix.Ether] =
            new SuffixValueWithLocale(1146, new Dictionary<Locale, string> { [Locale.Russian] = "эфира" }),
        // [ItemSuffix.Exhaustion] =
        //     new SuffixValueWithLocale(128, new Dictionary<Locale, string> { [Locale.Russian] = "истощения" }),
        [ItemSuffix.Radiance] =
            new SuffixValueWithLocale(1154, new Dictionary<Locale, string> { [Locale.Russian] = "сияния" }),
        // [ItemSuffix.Valor] =
        //     new SuffixValueWithLocale(136, new Dictionary<Locale, string> { [Locale.Russian] = "доблести" }),
        [ItemSuffix.Disease] =
            new SuffixValueWithLocale(1162, new Dictionary<Locale, string> { [Locale.Russian] = "болезни" }),
        // [ItemSuffix.Damage] =
        //     new SuffixValueWithLocale(144, new Dictionary<Locale, string> { [Locale.Russian] = "урона" }),
        [ItemSuffix.Value] =
            new SuffixValueWithLocale(1170, new Dictionary<Locale, string> { [Locale.Russian] = "ценности" }),
        // [ItemSuffix.Cruelty] =
        //     new SuffixValueWithLocale(152, new Dictionary<Locale, string> { [Locale.Russian] = "жестокости" }),
    };

    public static readonly Dictionary<ItemSuffix, SuffixValueWithLocale> SuffixesBootsGlovesBeltsHelmetsPants = new()
    {
        [ItemSuffix.Durability] =
            new SuffixValueWithLocale(64, new Dictionary<Locale, string> { [Locale.Russian] = "прочности" }),
        [ItemSuffix.Absorption] =
            new SuffixValueWithLocale(72, new Dictionary<Locale, string> { [Locale.Russian] = "поглощения" }),
        [ItemSuffix.Safety] =
            new SuffixValueWithLocale(80, new Dictionary<Locale, string> { [Locale.Russian] = "безопасности" }),
        [ItemSuffix.Health] =
            new SuffixValueWithLocale(88, new Dictionary<Locale, string> { [Locale.Russian] = "здоровья" }),
        [ItemSuffix.Meditation] =
            new SuffixValueWithLocale(96, new Dictionary<Locale, string> { [Locale.Russian] = "медитации" }),
        [ItemSuffix.Precision] =
            new SuffixValueWithLocale(104, new Dictionary<Locale, string> { [Locale.Russian] = "точности" }),
        [ItemSuffix.Ether] =
            new SuffixValueWithLocale(112, new Dictionary<Locale, string> { [Locale.Russian] = "эфира" }),
        [ItemSuffix.Value] =
            new SuffixValueWithLocale(120, new Dictionary<Locale, string> { [Locale.Russian] = "ценности" }),
    };

    public static readonly Dictionary<ItemSuffix, SuffixValueWithLocale> SuffixesChestplatesShields = new()
    {
        [ItemSuffix.Valor] =
            new SuffixValueWithLocale(64, new Dictionary<Locale, string> { [Locale.Russian] = "доблести" }),
        [ItemSuffix.Meditation] =
            new SuffixValueWithLocale(1090, new Dictionary<Locale, string> { [Locale.Russian] = "медитации" }),
        [ItemSuffix.Durability] =
            new SuffixValueWithLocale(72, new Dictionary<Locale, string> { [Locale.Russian] = "прочности" }),
        [ItemSuffix.Prana] =
            new SuffixValueWithLocale(1098, new Dictionary<Locale, string> { [Locale.Russian] = "праны" }),
        [ItemSuffix.Absorption] =
            new SuffixValueWithLocale(80, new Dictionary<Locale, string> { [Locale.Russian] = "поглощения" }),
        [ItemSuffix.Strength] =
            new SuffixValueWithLocale(1106, new Dictionary<Locale, string> { [Locale.Russian] = "силы" }),
        [ItemSuffix.Deflection] =
            new SuffixValueWithLocale(88, new Dictionary<Locale, string> { [Locale.Russian] = "отклонения" }),
        [ItemSuffix.Agility] =
            new SuffixValueWithLocale(1114, new Dictionary<Locale, string> { [Locale.Russian] = "ловкости" }),
        [ItemSuffix.Safety] =
            new SuffixValueWithLocale(96, new Dictionary<Locale, string> { [Locale.Russian] = "безопасности" }),
        // [ItemSuffix.Majesty] =
        //     new SuffixValueWithLocale(1122, new Dictionary<Locale, string> { [Locale.Russian] = "величия" }),
        [ItemSuffix.Invincibility] =
            new SuffixValueWithLocale(104, new Dictionary<Locale, string> { [Locale.Russian] = "неуязвимости" }),
        [ItemSuffix.Concentration] =
            new SuffixValueWithLocale(1130, new Dictionary<Locale, string> { [Locale.Russian] = "концентрации" }),
        [ItemSuffix.Health] =
            new SuffixValueWithLocale(112, new Dictionary<Locale, string> { [Locale.Russian] = "здоровья" }),
        [ItemSuffix.Earth] =
            new SuffixValueWithLocale(1138, new Dictionary<Locale, string> { [Locale.Russian] = "земли" }),
        [ItemSuffix.Life] =
            new SuffixValueWithLocale(120, new Dictionary<Locale, string> { [Locale.Russian] = "жизни" }),
        [ItemSuffix.Water] =
            new SuffixValueWithLocale(1146, new Dictionary<Locale, string> { [Locale.Russian] = "воды" }),
        // [ItemSuffix.Valor] =
        //     new SuffixValueWithLocale(128, new Dictionary<Locale, string> { [Locale.Russian] = "доблести" }),
        [ItemSuffix.Air] =
            new SuffixValueWithLocale(1154, new Dictionary<Locale, string> { [Locale.Russian] = "воздуха" }),
        // [ItemSuffix.Durability] =
        //     new SuffixValueWithLocale(136, new Dictionary<Locale, string> { [Locale.Russian] = "прочности" }),
        [ItemSuffix.Fire] =
            new SuffixValueWithLocale(1162, new Dictionary<Locale, string> { [Locale.Russian] = "огня" }),
        // [ItemSuffix.Absorption] =
        //     new SuffixValueWithLocale(144, new Dictionary<Locale, string> { [Locale.Russian] = "поглощения" }),
        [ItemSuffix.Elements] =
            new SuffixValueWithLocale(1170, new Dictionary<Locale, string> { [Locale.Russian] = "стихий" }),
        // [ItemSuffix.Deflection] =
        //     new SuffixValueWithLocale(152, new Dictionary<Locale, string> { [Locale.Russian] = "отклонения" }),
        [ItemSuffix.Value] =
            new SuffixValueWithLocale(1178, new Dictionary<Locale, string> { [Locale.Russian] = "ценности" }),
        // [ItemSuffix.Safety] =
        //     new SuffixValueWithLocale(160, new Dictionary<Locale, string> { [Locale.Russian] = "безопасности" }),
        // [ItemSuffix.Strength] =
        //     new SuffixValueWithLocale(1186, new Dictionary<Locale, string> { [Locale.Russian] = "силы" }),
        // [ItemSuffix.Invincibility] =
        //     new SuffixValueWithLocale(168, new Dictionary<Locale, string> { [Locale.Russian] = "неуязвимости" }),
        // [ItemSuffix.Agility] =
        //     new SuffixValueWithLocale(1194, new Dictionary<Locale, string> { [Locale.Russian] = "ловкости" }),
        // [ItemSuffix.Health] =
        //     new SuffixValueWithLocale(176, new Dictionary<Locale, string> { [Locale.Russian] = "здоровья" }),
        [ItemSuffix.Majesty] =
            new SuffixValueWithLocale(1202, new Dictionary<Locale, string> { [Locale.Russian] = "величия" }),
        // [ItemSuffix.Life] =
        //     new SuffixValueWithLocale(184, new Dictionary<Locale, string> { [Locale.Russian] = "жизни" }),
        // [ItemSuffix.Concentration] =
        //     new SuffixValueWithLocale(1210, new Dictionary<Locale, string> { [Locale.Russian] = "концентрации" }),
        // [ItemSuffix.Valor] =
        //     new SuffixValueWithLocale(192, new Dictionary<Locale, string> { [Locale.Russian] = "доблести" }),
        [ItemSuffix.Integrity] =
            new SuffixValueWithLocale(1218, new Dictionary<Locale, string> { [Locale.Russian] = "цельности" }),
        [ItemSuffix.IntegrityOther] =
            new SuffixValueWithLocale(1213, new Dictionary<Locale, string> { [Locale.Russian] = "цельности" }),
        // [ItemSuffix.Durability] =
        //     new SuffixValueWithLocale(200, new Dictionary<Locale, string> { [Locale.Russian] = "прочности" }),
        [ItemSuffix.Elements_New] =
            new SuffixValueWithLocale(1226, new Dictionary<Locale, string> { [Locale.Russian] = "цельности" }),
    };

    public static readonly Dictionary<ItemSuffix, SuffixValueWithLocale> SuffixesRobes = new()
    {
        [ItemSuffix.Value] =
            new SuffixValueWithLocale(64, new Dictionary<Locale, string> { [Locale.Russian] = "ценности" }),
        [ItemSuffix.Earth] =
            new SuffixValueWithLocale(1090, new Dictionary<Locale, string> { [Locale.Russian] = "земли" }),
        [ItemSuffix.Absorption] =
            new SuffixValueWithLocale(72, new Dictionary<Locale, string> { [Locale.Russian] = "поглощения" }),
        [ItemSuffix.Water] =
            new SuffixValueWithLocale(1098, new Dictionary<Locale, string> { [Locale.Russian] = "воды" }),
        [ItemSuffix.Deflection] =
            new SuffixValueWithLocale(80, new Dictionary<Locale, string> { [Locale.Russian] = "отклонения" }),
        [ItemSuffix.Air] =
            new SuffixValueWithLocale(1106, new Dictionary<Locale, string> { [Locale.Russian] = "воздуха" }),
        [ItemSuffix.Safety] =
            new SuffixValueWithLocale(88, new Dictionary<Locale, string> { [Locale.Russian] = "безопасности" }),
        [ItemSuffix.Fire] =
            new SuffixValueWithLocale(1114, new Dictionary<Locale, string> { [Locale.Russian] = "огня" }),
        [ItemSuffix.Health] =
            new SuffixValueWithLocale(96, new Dictionary<Locale, string> { [Locale.Russian] = "здоровья" }),
        [ItemSuffix.Ether] =
            new SuffixValueWithLocale(1122, new Dictionary<Locale, string> { [Locale.Russian] = "эфира" }),
        [ItemSuffix.Life] =
            new SuffixValueWithLocale(104, new Dictionary<Locale, string> { [Locale.Russian] = "жизни" }),
        [ItemSuffix.Eclipse] =
            new SuffixValueWithLocale(1130, new Dictionary<Locale, string> { [Locale.Russian] = "затмения" }),
        [ItemSuffix.Meditation] =
            new SuffixValueWithLocale(112, new Dictionary<Locale, string> { [Locale.Russian] = "медитации" }),
        [ItemSuffix.Archmage] =
            new SuffixValueWithLocale(1138, new Dictionary<Locale, string> { [Locale.Russian] = "архимага" }),
        [ItemSuffix.Prana] =
            new SuffixValueWithLocale(120, new Dictionary<Locale, string> { [Locale.Russian] = "праны" }),
        // [ItemSuffix.Durability] =
        //     new SuffixValueWithLocale(1146, new Dictionary<Locale, string> { [Locale.Russian] = "прочности" }),
        // [ItemSuffix.Value] =
        //     new SuffixValueWithLocale(128, new Dictionary<Locale, string> { [Locale.Russian] = "ценности" }),
        // [ItemSuffix.Deflection] =
        //     new SuffixValueWithLocale(1154, new Dictionary<Locale, string> { [Locale.Russian] = "отклонения" }),
        [ItemSuffix.Durability] =
            new SuffixValueWithLocale(136, new Dictionary<Locale, string> { [Locale.Russian] = "прочности" }),
        // [ItemSuffix.Safety] =
        //     new SuffixValueWithLocale(1162, new Dictionary<Locale, string> { [Locale.Russian] = "безопасности" }),
        // [ItemSuffix.Deflection] =
        //     new SuffixValueWithLocale(144, new Dictionary<Locale, string> { [Locale.Russian] = "отклонения" }),
        // [ItemSuffix.Health] =
        //     new SuffixValueWithLocale(1170, new Dictionary<Locale, string> { [Locale.Russian] = "здоровья" }),
        // [ItemSuffix.Safety] =
        //     new SuffixValueWithLocale(152, new Dictionary<Locale, string> { [Locale.Russian] = "безопасности" }),
        // [ItemSuffix.Life] =
        //     new SuffixValueWithLocale(1178, new Dictionary<Locale, string> { [Locale.Russian] = "жизни" }),
        // [ItemSuffix.Health] =
        //     new SuffixValueWithLocale(160, new Dictionary<Locale, string> { [Locale.Russian] = "здоровья" }),
        // [ItemSuffix.Meditation] =
        //     new SuffixValueWithLocale(1186, new Dictionary<Locale, string> { [Locale.Russian] = "медитации" }),
        // [ItemSuffix.Life] =
        //     new SuffixValueWithLocale(168, new Dictionary<Locale, string> { [Locale.Russian] = "жизни" }),
        // [ItemSuffix.Prana] =
        //     new SuffixValueWithLocale(1194, new Dictionary<Locale, string> { [Locale.Russian] = "праны" }),
        // [ItemSuffix.Meditation] =
        //     new SuffixValueWithLocale(176, new Dictionary<Locale, string> { [Locale.Russian] = "медитации" }),
        // [ItemSuffix.Ether] =
        //     new SuffixValueWithLocale(1202, new Dictionary<Locale, string> { [Locale.Russian] = "эфира" }),
        // [ItemSuffix.Prana] =
        //     new SuffixValueWithLocale(184, new Dictionary<Locale, string> { [Locale.Russian] = "праны" }),
        [ItemSuffix.Dragon] =
            new SuffixValueWithLocale(1210, new Dictionary<Locale, string> { [Locale.Russian] = "дракона" }),
        [ItemSuffix.DragonOther] =
            new SuffixValueWithLocale(1214, new Dictionary<Locale, string> { [Locale.Russian] = "дракона" }),
    };

    public static readonly Dictionary<ItemSuffix, SuffixValueWithLocale> SuffixesRings = new()
    {
        [ItemSuffix.Durability] =
            new SuffixValueWithLocale(64, new Dictionary<Locale, string> { [Locale.Russian] = "прочности" }),
        [ItemSuffix.Precision] =
            new SuffixValueWithLocale(1090, new Dictionary<Locale, string> { [Locale.Russian] = "точности" }),
        [ItemSuffix.Absorption] =
            new SuffixValueWithLocale(72, new Dictionary<Locale, string> { [Locale.Russian] = "поглощения" }),
        [ItemSuffix.Strength] =
            new SuffixValueWithLocale(1098, new Dictionary<Locale, string> { [Locale.Russian] = "силы" }),
        [ItemSuffix.Safety] =
            new SuffixValueWithLocale(80, new Dictionary<Locale, string> { [Locale.Russian] = "безопасности" }),
        [ItemSuffix.Agility] =
            new SuffixValueWithLocale(1106, new Dictionary<Locale, string> { [Locale.Russian] = "ловкости" }),
        [ItemSuffix.Health] =
            new SuffixValueWithLocale(88, new Dictionary<Locale, string> { [Locale.Russian] = "здоровья" }),
        [ItemSuffix.Accuracy] =
            new SuffixValueWithLocale(1114, new Dictionary<Locale, string> { [Locale.Russian] = "меткости" }),
        [ItemSuffix.Life] =
            new SuffixValueWithLocale(96, new Dictionary<Locale, string> { [Locale.Russian] = "жизни" }),
        [ItemSuffix.Endurance] =
            new SuffixValueWithLocale(1122, new Dictionary<Locale, string> { [Locale.Russian] = "выносливости" }),
        [ItemSuffix.Meditation] =
            new SuffixValueWithLocale(104, new Dictionary<Locale, string> { [Locale.Russian] = "медитации" }),
        [ItemSuffix.Earth] =
            new SuffixValueWithLocale(1130, new Dictionary<Locale, string> { [Locale.Russian] = "земли" }),
        [ItemSuffix.Prana] =
            new SuffixValueWithLocale(112, new Dictionary<Locale, string> { [Locale.Russian] = "праны" }),
        [ItemSuffix.Water] =
            new SuffixValueWithLocale(1138, new Dictionary<Locale, string> { [Locale.Russian] = "воды" }),
        [ItemSuffix.Ether] =
            new SuffixValueWithLocale(120, new Dictionary<Locale, string> { [Locale.Russian] = "эфира" }),
        [ItemSuffix.Air] =
            new SuffixValueWithLocale(1146, new Dictionary<Locale, string> { [Locale.Russian] = "воздуха" }),
        // [ItemSuffix.Absorption] =
        //     new SuffixValueWithLocale(128, new Dictionary<Locale, string> { [Locale.Russian] = "поглощения" }),
        [ItemSuffix.Fire] =
            new SuffixValueWithLocale(1154, new Dictionary<Locale, string> { [Locale.Russian] = "огня" }),
        // [ItemSuffix.Durability] =
        //     new SuffixValueWithLocale(136, new Dictionary<Locale, string> { [Locale.Russian] = "прочности" }),
        [ItemSuffix.Value] =
            new SuffixValueWithLocale(1162, new Dictionary<Locale, string> { [Locale.Russian] = "ценности" }),
    };
    
    public static readonly Dictionary<GameObjectType, SuffixToLocaleMap> ObjectTypeToSuffixLocaleMap = new ()
    {
        [GameObjectType.Amulet] = SuffixesAmuletBracelet,
        [GameObjectType.Bracelet] = SuffixesAmuletBracelet,
        [GameObjectType.Sword] = SuffixesSwordAxes,
        [GameObjectType.Axe] = SuffixesSwordAxes,
        [GameObjectType.Crossbow] = SuffixesCrossbows,
        [GameObjectType.Boots] = SuffixesBootsGlovesBeltsHelmetsPants,
        [GameObjectType.Gloves] = SuffixesBootsGlovesBeltsHelmetsPants,
        [GameObjectType.Belt] = SuffixesBootsGlovesBeltsHelmetsPants,
        [GameObjectType.Helmet] = SuffixesBootsGlovesBeltsHelmetsPants,
        [GameObjectType.Pants] = SuffixesBootsGlovesBeltsHelmetsPants,
        [GameObjectType.Armor] = SuffixesChestplatesShields,
        [GameObjectType.Shield] = SuffixesChestplatesShields,
        [GameObjectType.Robe] = SuffixesRobes,
        [GameObjectType.Ring] = SuffixesRings,
    };

    public static ItemSuffix GetSuffixById(this SuffixToLocaleMap map, int val)
    {
        return map.All(x => x.Value.value != val) 
            ? ItemSuffix.None 
            : map.First(x => x.Value.value == val).Key;
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
}
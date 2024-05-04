using System.Text;

namespace SphServer.Helpers;

public enum EntityActionType
{
    SET_POSITION = 0x06,
    FULL_SPAWN = 0x7C,
    FULL_SPAWN_2 = 0x7D,
    ATTACK = 0x2A,
    INTERACT = 0xA,
    UNKNOWN = 0x14,
    UNDEF
}

public enum EntityInteractionType
{
    DEATH = 0x040D,
    OPEN_CONTAINER = 0x0103,
    UNDEF
}

public enum OptionalPacketFields : byte
{
    COUNT = 12,
    PA = 14,
    NAME = 15,
    MADE_BY = 46,
    UNKNOWN = 0xFF
}

public static class PacketPartMapping
{
    public static HashSet<ObjectType> ItemObjectTypes =
    [
        ObjectType.Token,
        ObjectType.Mutator,
        ObjectType.SeedCastle,
        ObjectType.XpPillDegree,
        ObjectType.TokenMultiuse,
        ObjectType.TradeLicense,
        ObjectType.ScrollLegend,
        ObjectType.ScrollRecipe,
        ObjectType.Mission,
        ObjectType.TokenIsland,
        ObjectType.TokenIslandGuest,
        ObjectType.Bead,
        ObjectType.BackpackLarge,
        ObjectType.BackpackSmall,
        ObjectType.Sack,
        ObjectType.MantraBookSmall,
        ObjectType.RecipeBook,
        ObjectType.MantraBookLarge,
        ObjectType.MantraBookGreat,
        ObjectType.MapBook,
        ObjectType.KeyBarn,
        ObjectType.PowderFinale,
        ObjectType.PowderSingleTarget,
        ObjectType.PowderAmilus,
        ObjectType.PowderAoE,
        ObjectType.ElixirCastle,
        ObjectType.ElixirTrap,
        ObjectType.WeaponSword,
        ObjectType.WeaponAxe,
        ObjectType.WeaponCrossbow,
        ObjectType.Arrows,
        ObjectType.RingDiamond,
        ObjectType.RingRuby,
        ObjectType.Ruby,
        ObjectType.RingGold,
        ObjectType.AlchemyMineral,
        ObjectType.AlchemyPlant,
        ObjectType.AlchemyMetal,
        ObjectType.FoodApple,
        ObjectType.FoodPear,
        ObjectType.FoodMeat,
        ObjectType.FoodBread,
        ObjectType.FoodFish,
        ObjectType.AlchemyBrushwood,
        ObjectType.Key,
        ObjectType.Map,
        ObjectType.Inkpot,
        ObjectType.Firecracker,
        ObjectType.Ear,
        ObjectType.EarString,
        ObjectType.MonsterPart,
        ObjectType.Firework,
        ObjectType.InkpotBroken,
        ObjectType.ArmorChest,
        ObjectType.ArmorAmulet,
        ObjectType.ArmorBoots,
        ObjectType.ArmorGloves,
        ObjectType.ArmorBelt,
        ObjectType.ArmorShield,
        ObjectType.ArmorHelmet,
        ObjectType.ArmorPants,
        ObjectType.ArmorBracelet,
        ObjectType.Ring,
        ObjectType.ArmorRobe,
        ObjectType.RingGolem,
        ObjectType.AlchemyPot,
        ObjectType.AlchemyFurnace,
        ObjectType.Blueprint,
        ObjectType.QuestArmorChest,
        ObjectType.QuestArmorAmulet,
        ObjectType.QuestArmorBoots,
        ObjectType.QuestArmorGloves,
        ObjectType.QuestArmorBelt,
        ObjectType.QuestArmorShield,
        ObjectType.QuestArmorHelmet,
        ObjectType.QuestArmorPants,
        ObjectType.QuestArmorBracelet,
        ObjectType.QuestArmorRing,
        ObjectType.QuestArmorRobe,
        ObjectType.QuestWeaponSword,
        ObjectType.QuestWeaponAxe,
        ObjectType.QuestWeaponCrossbow,
        ObjectType.SpecialGuild,
        ObjectType.SpecialAbility,
        ObjectType.SpecialAbilitySteal,
        ObjectType.ArmorHelmetPremium,
        ObjectType.MantraWhite,
        ObjectType.MantraBlack
    ];

    public static HashSet<ObjectType> EntityObjectTypes =
    [
        ObjectType.Token,
        ObjectType.Mutator,
        ObjectType.SeedCastle,
        ObjectType.XpPillDegree,
        ObjectType.DoorEntrance,
        ObjectType.DoorExit,
        ObjectType.TokenMultiuse,
        ObjectType.TradeLicense,
        ObjectType.MobSpawner,
        ObjectType.Monster,
        ObjectType.MonsterFlyer,
        ObjectType.NpcBanker,
        ObjectType.NpcTrade,
        ObjectType.NpcQuestDegree,
        ObjectType.NpcQuestTitle,
        ObjectType.SackMobLoot,
        ObjectType.ChestInDungeon,
        ObjectType.Chest,
        ObjectType.ScrollLegend,
        ObjectType.ScrollRecipe,
        ObjectType.Mission,
        ObjectType.TokenIsland,
        ObjectType.TokenIslandGuest,
        ObjectType.Bead,
        ObjectType.BackpackLarge,
        ObjectType.BackpackSmall,
        ObjectType.Sack,
        ObjectType.MantraBookSmall,
        ObjectType.RecipeBook,
        ObjectType.MantraBookLarge,
        ObjectType.MantraBookGreat,
        ObjectType.MapBook,
        ObjectType.KeyBarn,
        ObjectType.PowderFinale,
        ObjectType.PowderSingleTarget,
        ObjectType.PowderAmilus,
        ObjectType.PowderAoE,
        ObjectType.ElixirCastle,
        ObjectType.ElixirTrap,
        ObjectType.WeaponSword,
        ObjectType.WeaponAxe,
        ObjectType.WeaponCrossbow,
        ObjectType.Arrows,
        ObjectType.RingDiamond,
        ObjectType.RingRuby,
        ObjectType.Ruby,
        ObjectType.RingGold,
        ObjectType.AlchemyMineral,
        ObjectType.AlchemyPlant,
        ObjectType.AlchemyMetal,
        ObjectType.FoodApple,
        ObjectType.FoodPear,
        ObjectType.FoodMeat,
        ObjectType.FoodBread,
        ObjectType.FoodFish,
        ObjectType.AlchemyBrushwood,
        ObjectType.Key,
        ObjectType.Map,
        ObjectType.Inkpot,
        ObjectType.Firecracker,
        ObjectType.Ear,
        ObjectType.EarString,
        ObjectType.MonsterPart,
        ObjectType.Firework,
        ObjectType.InkpotBroken,
        ObjectType.ArmorChest,
        ObjectType.ArmorAmulet,
        ObjectType.ArmorBoots,
        ObjectType.ArmorGloves,
        ObjectType.ArmorBelt,
        ObjectType.ArmorShield,
        ObjectType.ArmorHelmet,
        ObjectType.ArmorPants,
        ObjectType.ArmorBracelet,
        ObjectType.Ring,
        ObjectType.ArmorRobe,
        ObjectType.RingGolem,
        ObjectType.AlchemyPot,
        ObjectType.AlchemyFurnace,
        ObjectType.Blueprint,
        ObjectType.QuestArmorChest,
        ObjectType.QuestArmorAmulet,
        ObjectType.QuestArmorBoots,
        ObjectType.QuestArmorGloves,
        ObjectType.QuestArmorBelt,
        ObjectType.QuestArmorShield,
        ObjectType.QuestArmorHelmet,
        ObjectType.QuestArmorPants,
        ObjectType.QuestArmorBracelet,
        ObjectType.QuestArmorRing,
        ObjectType.QuestArmorRobe,
        ObjectType.QuestWeaponSword,
        ObjectType.QuestWeaponAxe,
        ObjectType.QuestWeaponCrossbow,
        ObjectType.SpecialGuild,
        ObjectType.SpecialAbility,
        ObjectType.SpecialAbilitySteal,
        ObjectType.ArmorHelmetPremium,
        ObjectType.MantraWhite,
        ObjectType.MantraBlack
    ];

    public static HashSet<ObjectType> ItemBagObjectTypes =
    [
        ObjectType.BackpackLarge,
        ObjectType.BackpackSmall,
        ObjectType.MantraBookSmall,
        ObjectType.MantraBookLarge,
        ObjectType.MantraBookGreat,
        ObjectType.MapBook,
        ObjectType.AlchemyPot,
        ObjectType.Sack
    ];

    public static HashSet<ObjectType> ItemRecipeBagObjectTypes =
    [
        ObjectType.RecipeBook
    ];

    public static HashSet<ObjectType> EquippableItemTypes =
    [
        ObjectType.WeaponSword,
        ObjectType.WeaponAxe,
        ObjectType.WeaponCrossbow,
        ObjectType.ArmorChest,
        ObjectType.ArmorAmulet,
        ObjectType.ArmorBoots,
        ObjectType.ArmorGloves,
        ObjectType.ArmorBelt,
        ObjectType.ArmorShield,
        ObjectType.ArmorHelmet,
        ObjectType.ArmorPants,
        ObjectType.ArmorBracelet,
        ObjectType.Ring,
        ObjectType.ArmorRobe,
        ObjectType.QuestArmorChest,
        ObjectType.QuestArmorAmulet,
        ObjectType.QuestArmorBoots,
        ObjectType.QuestArmorGloves,
        ObjectType.QuestArmorBelt,
        ObjectType.QuestArmorShield,
        ObjectType.QuestArmorHelmet,
        ObjectType.QuestArmorPants,
        ObjectType.QuestArmorBracelet,
        ObjectType.QuestArmorRing,
        ObjectType.QuestArmorRobe,
        ObjectType.QuestWeaponSword,
        ObjectType.QuestWeaponAxe,
        ObjectType.QuestWeaponCrossbow
    ];

    public static Tuple<string, string, bool> GetPacketPartName (ObjectType objectType, EntityActionType actionType,
        EntityInteractionType interactionType, ushort entId, bool hasGameId, List<OptionalPacketFields> optionalFields)
    {
        var entityNameForComment = CamelCaseToUpperWithSpaces(objectType.ToString());
        var packetName = string.Empty;
        var success = true;
        var comment = (string?) null;
        var genericItemPacket = false;
        var shouldHaveOptionalFields = false;
        switch (actionType)
        {
            case EntityActionType.SET_POSITION:
                packetName = "entity_move";
                comment = $"ENTITY MOVES [{entId:X4}])";
                break;
            case EntityActionType.ATTACK:
                packetName = "change_target_health";
                comment = $"ENTITY DEALS DAMAGE [{entId:X4}]";
                break;
            case EntityActionType.INTERACT:
                switch (interactionType)
                {
                    case EntityInteractionType.DEATH:
                        packetName = "entity_killed";
                        comment = $"ENTITY KILLED [{entId:X4}]";
                        break;
                    case EntityInteractionType.OPEN_CONTAINER:
                        success = false;
                        packetName = "header_with_action_type";
                        comment = $"CONTAINER OPEN [{entId:X4}]";
                        break;
                    case EntityInteractionType.UNDEF:
                        packetName = "header_with_action_type";
                        success = false;
                        break;
                    default:
                        success = false;
                        break;
                }

                break;
            case EntityActionType.UNKNOWN:
                packetName = "action_0x14";
                comment = $"ENTITY DOING 0x14 [{entId:X4}]";
                break;
            // assuming full
            default:
            {
                switch (objectType)
                {
                    case ObjectType.Monster:
                    case ObjectType.MonsterFlyer:
                        packetName = "monster_full";
                        break;
                    case ObjectType.MobSpawner:
                        packetName = "mob_spawner";
                        break;
                    case ObjectType.NpcTrade:
                        packetName = "npc_trade";
                        break;
                    case ObjectType.NpcQuestTitle:
                    case ObjectType.NpcQuestDegree:
                        packetName = "npc_quest_title";
                        break;
                    case ObjectType.DoorEntrance:
                    case ObjectType.DoorExit:
                        packetName = "door_test";
                        break;
                    case ObjectType.ChestInDungeon:
                        packetName = "chest_in_dungeon";
                        break;
                    case ObjectType.SackMobLoot:
                        packetName = "sack_mob_loot";
                        break;
                    case ObjectType.TutorialMessage:
                        packetName = "tutorial_message";
                        break;
                    case ObjectType.Teleport:
                        packetName = "teleport";
                        break;
                    case ObjectType.Key:
                    case ObjectType.KeyBarn:
                        packetName = "item_key";
                        break;
                    case ObjectType.Ring:
                        packetName = "item_ring";
                        shouldHaveOptionalFields = true;
                        break;
                    case ObjectType.AlchemyPot:
                        packetName = "item_alchemypot";
                        break;
                    case ObjectType.Firecracker:
                    case ObjectType.Firework:
                        packetName = "item_firework";
                        break;
                    case ObjectType.MantraBlack:
                    case ObjectType.MantraWhite:
                        packetName = "item_mantra_counted";
                        break;
                    case ObjectType.ScrollLegend:
                    case ObjectType.ScrollRecipe:
                        packetName = "item_scroll";
                        shouldHaveOptionalFields = true;
                        break;
                    case ObjectType.Sack:
                        packetName = "item_sack";
                        break;
                    case ObjectType.EarString:
                        packetName = "item_earstring";
                        break;
                    case ObjectType.Token:
                        packetName = "item_token";
                        break;
                    case ObjectType.TokenMultiuse:
                        packetName = "item_token_multiuse";
                        break;
                    case ObjectType.MantraBookGreat:
                        packetName = "item_mantrabook_great";
                        break;
                    case ObjectType.TokenIslandGuest:
                        packetName = "item_token_island_guest";
                        break;
                    default:
                        if (ItemRecipeBagObjectTypes.Contains(objectType))
                        {
                            packetName = "item_recipebook";
                        }
                        else if (ItemBagObjectTypes.Contains(objectType))
                        {
                            packetName = "item_bag";
                        }
                        else if (ItemObjectTypes.Contains(objectType))
                        {
                            packetName = "item";
                            genericItemPacket = true;
                        }
                        else
                        {
                            success = false;
                        }

                        break;
                }

                if (genericItemPacket)
                {
                    if (hasGameId)
                    {
                        packetName += "_with_gameid";
                    }

                    shouldHaveOptionalFields = true;
                }

                if (shouldHaveOptionalFields)
                {
                    foreach (var field in optionalFields)
                    {
                        switch (field)
                        {
                            case OptionalPacketFields.PA:
                                packetName += "_pa";
                                break;
                            case OptionalPacketFields.COUNT:
                                packetName += "_counted";
                                break;
                            case OptionalPacketFields.NAME:
                                packetName += "_named";
                                break;
                            case OptionalPacketFields.MADE_BY:
                                packetName += "_made";
                                break;
                        }
                    }
                }

                break;
            }
        }

        comment ??= $"NEW ENTITY -- {entityNameForComment} [{entId:X4}]";

        return new Tuple<string, string, bool>(packetName, comment, success);
    }

    private static string CamelCaseToUpperWithSpaces (string s)
    {
        var sb = new StringBuilder();
        foreach (var c in s)
        {
            if (char.IsUpper(c))
            {
                sb.Append(' ');
            }

            sb.Append(char.ToUpper(c));
        }

        return sb.ToString();
    }
}

public static class ObjectTypeToPacketNameMap
{
    public static Dictionary<ObjectType, string> Mapping = new ()
    {
        [ObjectType.Despawn] = "despawn",
        [ObjectType.UpdateState] = "",
        [ObjectType.Player] = "",
        [ObjectType.Token] = "item_token",
        [ObjectType.Mutator] = "",
        [ObjectType.SeedCastle] = "",
        [ObjectType.XpPillDegree] = "",
        [ObjectType.DoorEntrance] = "",
        [ObjectType.DoorExit] = "",
        [ObjectType.Teleport] = "teleport",
        [ObjectType.TokenMultiuse] = "item_token_multiuse",
        [ObjectType.TradeLicense] = "",
        [ObjectType.MobSpawner] = "",
        [ObjectType.TutorialMessage] = "tutorial_message",
        [ObjectType.ScrollLegend] = "", // item_scroll or item_scroll_counted
        [ObjectType.ScrollRecipe] = "", // item_scroll or item_scroll_counted
        [ObjectType.Mission] = "",
        [ObjectType.TokenIsland] = "",
        [ObjectType.TokenIslandGuest] = "item_token_island_guest",
        [ObjectType.NpcQuestTitle] = "npc_quest_title",
        [ObjectType.NpcQuestDegree] = "",
        [ObjectType.Monster] = "",
        [ObjectType.MonsterFlyer] = "",
        [ObjectType.NpcTrade] = "npc_trade",
        [ObjectType.NpcBanker] = "",
        [ObjectType.Bead] = "",
        [ObjectType.BackpackLarge] = "item_backpack",
        [ObjectType.BackpackSmall] = "item_backpack",
        [ObjectType.Sack] = "item_sack",
        [ObjectType.Chest] = "",
        [ObjectType.SackMobLoot] = "sack_mob_loot",
        [ObjectType.MantraBookSmall] = "item_mantrabook",
        [ObjectType.RecipeBook] = "item_recipebook",
        [ObjectType.MantraBookLarge] = "item_mantrabook",
        [ObjectType.MantraBookGreat] = "item_mantrabook_great",
        [ObjectType.MapBook] = "",
        [ObjectType.ChestInDungeon] = "chest_in_dungeon",
        [ObjectType.KeyBarn] = "item_key",
        [ObjectType.PowderFinale] = "", //item_powder_counted
        [ObjectType.PowderSingleTarget] = "", //item_powder_counted
        [ObjectType.PowderAmilus] = "", //item_powder_counted
        [ObjectType.PowderAoE] = "", //item_powder_counted
        [ObjectType.ElixirCastle] = "", // item_elixir_counted
        [ObjectType.ElixirTrap] = "", // item_elixir_counted
        [ObjectType.WeaponSword] = "item_sword",
        [ObjectType.WeaponAxe] = "item_axe",
        [ObjectType.WeaponCrossbow] = "item_crossbow",
        [ObjectType.Arrows] = "item_arrows_counted",
        [ObjectType.RingDiamond] = "", //item_ring_diamond_counted
        [ObjectType.RingRuby] = "",
        [ObjectType.Ruby] = "",
        [ObjectType.RingGold] = "", //item_ring_gold_counted
        [ObjectType.AlchemyMineral] = "", // item_alchemy_counted
        [ObjectType.AlchemyPlant] = "", // item_alchemy_counted
        [ObjectType.AlchemyMetal] = "", // item_alchemy_counted
        [ObjectType.FoodApple] = "", // item_food_counted
        [ObjectType.FoodPear] = "", // item_food_counted
        [ObjectType.FoodMeat] = "", // item_food_counted
        [ObjectType.FoodBread] = "", // item_food_counted
        [ObjectType.FoodFish] = "", // item_food_counted
        [ObjectType.AlchemyBrushwood] = "",
        [ObjectType.Key] = "item_key",
        [ObjectType.Map] = "item_map",
        [ObjectType.Inkpot] = "item_inkpot",
        [ObjectType.Firecracker] = "item_firework",
        [ObjectType.Ear] = "",
        [ObjectType.EarString] = "",
        [ObjectType.MonsterPart] = "",
        [ObjectType.Firework] = "item_firework",
        [ObjectType.InkpotBroken] = "",
        [ObjectType.ArmorChest] = "", //item_armor_integrity_pa
        [ObjectType.ArmorAmulet] = "",
        [ObjectType.ArmorBoots] = "",
        [ObjectType.ArmorGloves] = "",
        [ObjectType.ArmorBelt] = "",
        [ObjectType.ArmorShield] = "",
        [ObjectType.ArmorHelmet] = "",
        [ObjectType.ArmorPants] = "item_pants",
        [ObjectType.ArmorBracelet] = "",
        [ObjectType.Ring] = "item_ring_named",
        [ObjectType.ArmorRobe] = "", // item_robe_dragon_pa
        [ObjectType.RingGolem] = "",
        [ObjectType.AlchemyPot] = "item_alchemypot",
        [ObjectType.AlchemyFurnace] = "",
        [ObjectType.Blueprint] = "",
        [ObjectType.QuestArmorChest] = "",
        [ObjectType.QuestArmorAmulet] = "",
        [ObjectType.QuestArmorBoots] = "item_quest_boots",
        [ObjectType.QuestArmorGloves] = "",
        [ObjectType.QuestArmorBelt] = "",
        [ObjectType.QuestArmorShield] = "item_quest_shield",
        [ObjectType.QuestArmorHelmet] = "item_quest_helmet",
        [ObjectType.QuestArmorPants] = "",
        [ObjectType.QuestArmorBracelet] = "",
        [ObjectType.QuestArmorRing] = "",
        [ObjectType.QuestArmorRobe] = "item_quest_robe",
        [ObjectType.QuestWeaponSword] = "",
        [ObjectType.QuestWeaponAxe] = "",
        [ObjectType.QuestWeaponCrossbow] = "item_quest_crossbow",
        [ObjectType.SpecialGuild] = "",
        [ObjectType.SpecialAbility] = "",
        [ObjectType.SpecialAbilitySteal] = "",
        [ObjectType.ArmorHelmetPremium] = "",
        [ObjectType.MantraWhite] = "", //item_mantra_counted
        [ObjectType.MantraBlack] = "" //item_mantra_counted
        //Unknown
    };
}
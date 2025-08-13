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
    public static HashSet<PacketObjectTypes> ItemObjectTypes =
    [
        PacketObjectTypes.Token,
        PacketObjectTypes.Mutator,
        PacketObjectTypes.SeedCastle,
        PacketObjectTypes.XpPillDegree,
        PacketObjectTypes.TokenMultiuse,
        PacketObjectTypes.TradeLicense,
        PacketObjectTypes.ScrollLegend,
        PacketObjectTypes.ScrollRecipe,
        PacketObjectTypes.Mission,
        PacketObjectTypes.TokenIsland,
        PacketObjectTypes.TokenIslandGuest,
        PacketObjectTypes.TokenTutorialTorweal,
        PacketObjectTypes.Bead,
        PacketObjectTypes.BackpackLarge,
        PacketObjectTypes.BackpackSmall,
        PacketObjectTypes.Sack,
        PacketObjectTypes.MantraBookSmall,
        PacketObjectTypes.RecipeBook,
        PacketObjectTypes.MantraBookLarge,
        PacketObjectTypes.MantraBookGreat,
        PacketObjectTypes.MapBook,
        PacketObjectTypes.KeyBarn,
        PacketObjectTypes.PowderFinale,
        PacketObjectTypes.PowderSingleTarget,
        PacketObjectTypes.PowderAmilus,
        PacketObjectTypes.PowderAoE,
        PacketObjectTypes.ElixirCastle,
        PacketObjectTypes.ElixirTrap,
        PacketObjectTypes.WeaponSword,
        PacketObjectTypes.WeaponStartingSword,
        PacketObjectTypes.WeaponAxe,
        PacketObjectTypes.WeaponCrossbow,
        PacketObjectTypes.Arrows,
        PacketObjectTypes.RingDiamond,
        PacketObjectTypes.RingRuby,
        PacketObjectTypes.Ruby,
        PacketObjectTypes.RingGold,
        PacketObjectTypes.AlchemyMineral,
        PacketObjectTypes.AlchemyPlant,
        PacketObjectTypes.AlchemyMetal,
        PacketObjectTypes.FoodApple,
        PacketObjectTypes.FoodPear,
        PacketObjectTypes.FoodMeat,
        PacketObjectTypes.FoodBread,
        PacketObjectTypes.FoodFish,
        PacketObjectTypes.AlchemyBrushwood,
        PacketObjectTypes.Key,
        PacketObjectTypes.Map,
        PacketObjectTypes.Inkpot,
        PacketObjectTypes.Firecracker,
        PacketObjectTypes.Ear,
        PacketObjectTypes.EarString,
        PacketObjectTypes.MonsterPart,
        PacketObjectTypes.Firework,
        PacketObjectTypes.InkpotBroken,
        PacketObjectTypes.ArmorChest,
        PacketObjectTypes.ArmorAmulet,
        PacketObjectTypes.ArmorBoots,
        PacketObjectTypes.ArmorGloves,
        PacketObjectTypes.ArmorBelt,
        PacketObjectTypes.ArmorShield,
        PacketObjectTypes.ArmorHelmet,
        PacketObjectTypes.ArmorPants,
        PacketObjectTypes.ArmorBracelet,
        PacketObjectTypes.Ring,
        PacketObjectTypes.ArmorRobe,
        PacketObjectTypes.RingGolem,
        PacketObjectTypes.AlchemyPot,
        PacketObjectTypes.AlchemyFurnace,
        PacketObjectTypes.Blueprint,
        PacketObjectTypes.QuestArmorChest,
        PacketObjectTypes.QuestArmorChest2,
        PacketObjectTypes.QuestArmorBoots,
        PacketObjectTypes.QuestArmorGloves,
        PacketObjectTypes.QuestArmorBelt,
        PacketObjectTypes.QuestArmorShield,
        PacketObjectTypes.QuestArmorHelmet,
        PacketObjectTypes.QuestArmorPants,
        PacketObjectTypes.QuestArmorBracelet,
        PacketObjectTypes.QuestArmorRing,
        PacketObjectTypes.QuestArmorRobe,
        PacketObjectTypes.QuestWeaponSword,
        PacketObjectTypes.QuestWeaponAxe,
        PacketObjectTypes.QuestWeaponCrossbow,
        PacketObjectTypes.SpecialGuild,
        PacketObjectTypes.SpecialAbility,
        PacketObjectTypes.SpecialAbilitySteal,
        PacketObjectTypes.ArmorHelmetPremium,
        PacketObjectTypes.MantraWhite,
        PacketObjectTypes.MantraBlack
    ];

    public static HashSet<PacketObjectTypes> EntityObjectTypes =
    [
        PacketObjectTypes.Token,
        PacketObjectTypes.Mutator,
        PacketObjectTypes.Dungeon,
        PacketObjectTypes.SeedCastle,
        PacketObjectTypes.XpPillDegree,
        PacketObjectTypes.DoorEntrance,
        PacketObjectTypes.DoorExit,
        PacketObjectTypes.Teleport,
        PacketObjectTypes.TeleportWithTarget,
        PacketObjectTypes.DungeonEntrance,
        PacketObjectTypes.TutorialMessage,
        PacketObjectTypes.TeleportWild,
        PacketObjectTypes.TokenMultiuse,
        PacketObjectTypes.TokenIsland,
        PacketObjectTypes.TokenTutorialTorweal,
        PacketObjectTypes.TradeLicense,
        PacketObjectTypes.MobSpawner,
        PacketObjectTypes.TournamentTeleport,
        PacketObjectTypes.Monster,
        PacketObjectTypes.MonsterFlyer,
        PacketObjectTypes.NpcBanker,
        PacketObjectTypes.NpcTrade,
        PacketObjectTypes.NpcQuestDegree,
        PacketObjectTypes.NpcQuestKarma,
        PacketObjectTypes.NpcQuestTitle,
        PacketObjectTypes.NpcGuilder,
        PacketObjectTypes.NpcGuide,
        PacketObjectTypes.NpcTradeRandomName,
        PacketObjectTypes.SackMobLoot,
        PacketObjectTypes.ChestInDungeon,
        PacketObjectTypes.NewPlayerDungeonStartPoint,
        PacketObjectTypes.Chest,
        PacketObjectTypes.ScrollLegend,
        PacketObjectTypes.ScrollRecipe,
        PacketObjectTypes.Mission,
        PacketObjectTypes.TokenIslandGuest,
        PacketObjectTypes.Bead,
        PacketObjectTypes.BackpackLarge,
        PacketObjectTypes.BackpackSmall,
        PacketObjectTypes.Sack,
        PacketObjectTypes.MantraBookSmall,
        PacketObjectTypes.RecipeBook,
        PacketObjectTypes.MantraBookLarge,
        PacketObjectTypes.MantraBookGreat,
        PacketObjectTypes.MapBook,
        PacketObjectTypes.KeyBarn,
        PacketObjectTypes.PowderFinale,
        PacketObjectTypes.PowderSingleTarget,
        PacketObjectTypes.PowderAmilus,
        PacketObjectTypes.PowderAoE,
        PacketObjectTypes.ElixirCastle,
        PacketObjectTypes.ElixirTrap,
        PacketObjectTypes.WeaponSword,
        PacketObjectTypes.WeaponStartingSword,
        PacketObjectTypes.WeaponAxe,
        PacketObjectTypes.WeaponCrossbow,
        PacketObjectTypes.Arrows,
        PacketObjectTypes.RingDiamond,
        PacketObjectTypes.RingRuby,
        PacketObjectTypes.Ruby,
        PacketObjectTypes.RingGold,
        PacketObjectTypes.AlchemyMineral,
        PacketObjectTypes.AlchemyPlant,
        PacketObjectTypes.AlchemyMetal,
        PacketObjectTypes.FoodApple,
        PacketObjectTypes.FoodPear,
        PacketObjectTypes.FoodMeat,
        PacketObjectTypes.FoodBread,
        PacketObjectTypes.FoodFish,
        PacketObjectTypes.AlchemyBrushwood,
        PacketObjectTypes.Key,
        PacketObjectTypes.Map,
        PacketObjectTypes.Inkpot,
        PacketObjectTypes.Firecracker,
        PacketObjectTypes.Ear,
        PacketObjectTypes.EarString,
        PacketObjectTypes.MonsterPart,
        PacketObjectTypes.Firework,
        PacketObjectTypes.InkpotBroken,
        PacketObjectTypes.ArmorChest,
        PacketObjectTypes.ArmorAmulet,
        PacketObjectTypes.ArmorBoots,
        PacketObjectTypes.ArmorGloves,
        PacketObjectTypes.ArmorBelt,
        PacketObjectTypes.ArmorShield,
        PacketObjectTypes.ArmorHelmet,
        PacketObjectTypes.ArmorPants,
        PacketObjectTypes.ArmorBracelet,
        PacketObjectTypes.Ring,
        PacketObjectTypes.ArmorRobe,
        PacketObjectTypes.RingGolem,
        PacketObjectTypes.AlchemyPot,
        PacketObjectTypes.AlchemyFurnace,
        PacketObjectTypes.Blueprint,
        PacketObjectTypes.Workshop,
        PacketObjectTypes.QuestArmorChest,
        PacketObjectTypes.QuestArmorChest2,
        PacketObjectTypes.QuestArmorBoots,
        PacketObjectTypes.QuestArmorGloves,
        PacketObjectTypes.QuestArmorBelt,
        PacketObjectTypes.QuestArmorShield,
        PacketObjectTypes.QuestArmorHelmet,
        PacketObjectTypes.QuestArmorPants,
        PacketObjectTypes.QuestArmorBracelet,
        PacketObjectTypes.QuestArmorRing,
        PacketObjectTypes.QuestArmorRobe,
        PacketObjectTypes.QuestWeaponSword,
        PacketObjectTypes.QuestWeaponAxe,
        PacketObjectTypes.QuestWeaponCrossbow,
        PacketObjectTypes.SpecialGuild,
        PacketObjectTypes.SpecialAbility,
        PacketObjectTypes.SpecialAbilitySteal,
        PacketObjectTypes.ArmorHelmetPremium,
        PacketObjectTypes.MantraWhite,
        PacketObjectTypes.MantraBlack
    ];

    public static HashSet<PacketObjectTypes> ItemBagObjectTypes =
    [
        PacketObjectTypes.BackpackLarge,
        PacketObjectTypes.BackpackSmall,
        PacketObjectTypes.MantraBookSmall,
        PacketObjectTypes.MantraBookLarge,
        PacketObjectTypes.MantraBookGreat,
        PacketObjectTypes.MapBook,
        PacketObjectTypes.AlchemyPot,
        PacketObjectTypes.Sack
    ];

    public static HashSet<PacketObjectTypes> ItemRecipeBagObjectTypes =
    [
        PacketObjectTypes.RecipeBook
    ];

    public static HashSet<PacketObjectTypes> EquippableItemTypes =
    [
        PacketObjectTypes.WeaponSword,
        PacketObjectTypes.WeaponStartingSword,
        PacketObjectTypes.WeaponAxe,
        PacketObjectTypes.WeaponCrossbow,
        PacketObjectTypes.ArmorChest,
        PacketObjectTypes.ArmorAmulet,
        PacketObjectTypes.ArmorBoots,
        PacketObjectTypes.ArmorGloves,
        PacketObjectTypes.ArmorBelt,
        PacketObjectTypes.ArmorShield,
        PacketObjectTypes.ArmorHelmet,
        PacketObjectTypes.ArmorPants,
        PacketObjectTypes.ArmorBracelet,
        PacketObjectTypes.Ring,
        PacketObjectTypes.ArmorRobe,
        PacketObjectTypes.QuestArmorChest,
        PacketObjectTypes.QuestArmorChest2,
        PacketObjectTypes.QuestArmorBoots,
        PacketObjectTypes.QuestArmorGloves,
        PacketObjectTypes.QuestArmorBelt,
        PacketObjectTypes.QuestArmorShield,
        PacketObjectTypes.QuestArmorHelmet,
        PacketObjectTypes.QuestArmorPants,
        PacketObjectTypes.QuestArmorBracelet,
        PacketObjectTypes.QuestArmorRing,
        PacketObjectTypes.QuestArmorRobe,
        PacketObjectTypes.QuestWeaponSword,
        PacketObjectTypes.QuestWeaponAxe,
        PacketObjectTypes.QuestWeaponCrossbow
    ];

    public static Dictionary<PacketObjectTypes, string> WorldObjectsToTrack = new ()
    {
        [PacketObjectTypes.Teleport] = "teleports",
        [PacketObjectTypes.TeleportWild] = "teleport_wild",
        [PacketObjectTypes.TournamentTeleport] = "teleport_tournament",
        [PacketObjectTypes.AlchemyMineral] = "alchemy_minerals",
        [PacketObjectTypes.AlchemyPlant] = "alchemy_plants",
        [PacketObjectTypes.AlchemyMetal] = "alchemy_metals",
        [PacketObjectTypes.DungeonEntrance] = "dungeon_entrance",
        [PacketObjectTypes.Workshop] = "workshop"
    };

    public static Tuple<string, string, bool> GetPacketPartName (PacketObjectTypes packetObjectTypes, EntityActionType actionType,
        EntityInteractionType interactionType, ushort entId, bool hasGameId, List<OptionalPacketFields> optionalFields)
    {
        var entityNameForComment = CamelCaseToUpperWithSpaces(packetObjectTypes.ToString());
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
                switch (packetObjectTypes)
                {
                    case PacketObjectTypes.Monster:
                    case PacketObjectTypes.MonsterFlyer:
                        packetName = "entity_monster";
                        break;
                    case PacketObjectTypes.MobSpawner:
                        packetName = "mob_spawner";
                        break;
                    case PacketObjectTypes.NpcTrade:
                        packetName = "npc_trade";
                        break;
                    case PacketObjectTypes.NpcBanker:
                        packetName = "npc_banker";
                        break;
                    case PacketObjectTypes.NpcQuestTitle:
                    case PacketObjectTypes.NpcQuestDegree:
                    case PacketObjectTypes.NpcQuestKarma:
                        packetName = "npc_quest_title";
                        break;
                    case PacketObjectTypes.NpcGuilder:
                        packetName = "npc_guilder";
                        break;
                    case PacketObjectTypes.NpcGuide:
                        packetName = "npc_guide";
                        break;
                    case PacketObjectTypes.NpcTradeRandomName:
                        packetName = "npc_trade_random_name";
                        break;
                    case PacketObjectTypes.ChestInDungeon:
                        packetName = "chest_in_dungeon";
                        break;
                    case PacketObjectTypes.SackMobLoot:
                        packetName = "sack_mob_loot";
                        break;
                    case PacketObjectTypes.TutorialMessage:
                        packetName = "tutorial_message";
                        break;
                    case PacketObjectTypes.Teleport:
                        packetName = "teleport";
                        break;
                    case PacketObjectTypes.Key:
                    case PacketObjectTypes.KeyBarn:
                        packetName = "item_key";
                        break;
                    case PacketObjectTypes.Ring:
                        packetName = "item_ring";
                        shouldHaveOptionalFields = true;
                        break;
                    case PacketObjectTypes.AlchemyPot:
                        packetName = "item_alchemypot";
                        break;
                    case PacketObjectTypes.Firecracker:
                    case PacketObjectTypes.Firework:
                        packetName = "item_firework";
                        break;
                    case PacketObjectTypes.MantraBlack:
                    case PacketObjectTypes.MantraWhite:
                        packetName = "item_mantra_counted";
                        break;
                    case PacketObjectTypes.ScrollLegend:
                    case PacketObjectTypes.ScrollRecipe:
                        packetName = "item_scroll";
                        shouldHaveOptionalFields = true;
                        break;
                    case PacketObjectTypes.Sack:
                        packetName = "item_sack";
                        break;
                    case PacketObjectTypes.EarString:
                        packetName = "item_earstring";
                        break;
                    case PacketObjectTypes.Token:
                        packetName = "item_token";
                        break;
                    case PacketObjectTypes.TokenTutorialTorweal:
                        packetName = "item_token_tutorial";
                        break;
                    case PacketObjectTypes.TokenMultiuse:
                        packetName = "item_token_multiuse";
                        break;
                    case PacketObjectTypes.MantraBookGreat:
                        packetName = "item_mantrabook_great";
                        break;
                    case PacketObjectTypes.TokenIsland:
                        packetName = "item_token_island";
                        break;
                    case PacketObjectTypes.TokenIslandGuest:
                        packetName = "item_token_island_guest";
                        break;
                    case PacketObjectTypes.TradeLicense:
                        packetName = "item_license_trade";
                        break;
                    case PacketObjectTypes.AlchemyFurnace:
                        packetName = "entity_alchemyfurnace";
                        break;
                    case PacketObjectTypes.DoorEntrance:
                    case PacketObjectTypes.DoorExit:
                        packetName = "door_entrance";
                        break;
                    case PacketObjectTypes.DungeonEntrance:
                        packetName = "dungeon_entrance";
                        break;
                    case PacketObjectTypes.TeleportWithTarget:
                        packetName = "teleport_with_target";
                        break;
                    case PacketObjectTypes.TournamentTeleport:
                        packetName = "tournament_teleport";
                        break;
                    case PacketObjectTypes.Workshop:
                        packetName = "workshop";
                        break;
                    case PacketObjectTypes.Dungeon:
                        packetName = "new_player_dungeon";
                        break;
                    case PacketObjectTypes.WeaponStartingSword:
                        packetName = "weapon_starting_sword";
                        break;
                    case PacketObjectTypes.NewPlayerDungeonStartPoint:
                        packetName = "new_player_dungeon_start";
                        break;
                    default:
                        if (ItemRecipeBagObjectTypes.Contains(packetObjectTypes))
                        {
                            packetName = "item_recipebook";
                        }
                        else if (ItemBagObjectTypes.Contains(packetObjectTypes))
                        {
                            packetName = "item_bag";
                        }
                        else if (ItemObjectTypes.Contains(packetObjectTypes))
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
    public static Dictionary<PacketObjectTypes, string> Mapping = new ()
    {
        [PacketObjectTypes.Despawn] = "despawn",
        [PacketObjectTypes.UpdateState] = "",
        [PacketObjectTypes.Player] = "",
        [PacketObjectTypes.Token] = "item_token",
        [PacketObjectTypes.Mutator] = "",
        [PacketObjectTypes.SeedCastle] = "",
        [PacketObjectTypes.XpPillDegree] = "",
        [PacketObjectTypes.DoorEntrance] = "door_entrance",
        [PacketObjectTypes.DoorExit] = "door_entrance",
        [PacketObjectTypes.DungeonEntrance] = "dungeon_entrance",
        [PacketObjectTypes.Teleport] = "teleport",
        [PacketObjectTypes.TeleportWithTarget] = "teleport_with_target",
        [PacketObjectTypes.TokenMultiuse] = "item_token_multiuse",
        [PacketObjectTypes.TradeLicense] = "item_license_trade",
        [PacketObjectTypes.MobSpawner] = "mob_spawner",
        [PacketObjectTypes.TournamentTeleport] = "tournament_teleport",
        [PacketObjectTypes.TutorialMessage] = "tutorial_message",
        [PacketObjectTypes.ScrollLegend] = "item_scroll_counted", // item_scroll or item_scroll_counted
        [PacketObjectTypes.ScrollRecipe] = "item_scroll_counted", // item_scroll or item_scroll_counted
        [PacketObjectTypes.Mission] = "",
        [PacketObjectTypes.TokenIsland] = "item_token_island",
        [PacketObjectTypes.TokenIslandGuest] = "item_token_island_guest",
        [PacketObjectTypes.NpcQuestTitle] = "npc_quest_title",
        [PacketObjectTypes.NpcQuestDegree] = "",
        [PacketObjectTypes.NpcQuestKarma] = "npc_quest_karma",
        [PacketObjectTypes.Monster] = "monster_full",
        [PacketObjectTypes.MonsterFlyer] = "",
        [PacketObjectTypes.NpcTrade] = "npc_trade",
        [PacketObjectTypes.NpcBanker] = "npc_banker",
        [PacketObjectTypes.Bead] = "",
        [PacketObjectTypes.NpcGuilder] = "npc_guilder",
        [PacketObjectTypes.BackpackLarge] = "item_backpack",
        [PacketObjectTypes.BackpackSmall] = "item_backpack",
        [PacketObjectTypes.Sack] = "item_sack",
        [PacketObjectTypes.Chest] = "",
        [PacketObjectTypes.SackMobLoot] = "sack_mob_loot",
        [PacketObjectTypes.MantraBookSmall] = "item_mantrabook",
        [PacketObjectTypes.RecipeBook] = "item_recipebook",
        [PacketObjectTypes.MantraBookLarge] = "item_mantrabook",
        [PacketObjectTypes.MantraBookGreat] = "item_mantrabook_great",
        [PacketObjectTypes.MapBook] = "",
        [PacketObjectTypes.ChestInDungeon] = "chest_in_dungeon",
        [PacketObjectTypes.KeyBarn] = "item_key",
        [PacketObjectTypes.PowderFinale] = "item_powder_counted", //item_powder_counted
        [PacketObjectTypes.PowderSingleTarget] = "item_powder_counted", //item_powder_counted
        [PacketObjectTypes.PowderAmilus] = "item_powder_counted", //item_powder_counted
        [PacketObjectTypes.PowderAoE] = "item_powder_counted", //item_powder_counted
        [PacketObjectTypes.ElixirCastle] = "item_elixir_counted", // item_elixir_counted
        [PacketObjectTypes.ElixirTrap] = "item_elixir_counted", // item_elixir_counted
        [PacketObjectTypes.WeaponSword] = "item_amulet",
        [PacketObjectTypes.WeaponAxe] = "item_amulet",
        [PacketObjectTypes.WeaponCrossbow] = "item_amulet",
        [PacketObjectTypes.Arrows] = "item_arrows_counted",
        [PacketObjectTypes.RingDiamond] = "item_ring_diamond_counted", //item_ring_diamond_counted
        [PacketObjectTypes.RingRuby] = "",
        [PacketObjectTypes.Ruby] = "",
        [PacketObjectTypes.RingGold] = "", //item_ring_gold_counted
        [PacketObjectTypes.AlchemyMineral] = "alchemy_resource_ground", // item_alchemy_counted
        [PacketObjectTypes.AlchemyPlant] = "alchemy_resource_ground", // item_alchemy_counted
        [PacketObjectTypes.AlchemyMetal] = "alchemy_resource_ground", // item_alchemy_counted
        [PacketObjectTypes.FoodApple] = "item_food_counted", // item_food_counted
        [PacketObjectTypes.FoodPear] = "item_food_counted", // item_food_counted
        [PacketObjectTypes.FoodMeat] = "item_food_counted", // item_food_counted
        [PacketObjectTypes.FoodBread] = "item_food_counted", // item_food_counted
        [PacketObjectTypes.FoodFish] = "item_food_counted", // item_food_counted
        [PacketObjectTypes.AlchemyBrushwood] = "",
        [PacketObjectTypes.Key] = "item_key",
        [PacketObjectTypes.Map] = "item_map",
        [PacketObjectTypes.Inkpot] = "item_inkpot",
        [PacketObjectTypes.Firecracker] = "item_firework",
        [PacketObjectTypes.Ear] = "",
        [PacketObjectTypes.EarString] = "item_earstring",
        [PacketObjectTypes.MonsterPart] = "",
        [PacketObjectTypes.Firework] = "item_firework",
        [PacketObjectTypes.InkpotBroken] = "",
        [PacketObjectTypes.ArmorChest] = "item_amulet", // generic item packet
        [PacketObjectTypes.ArmorAmulet] = "item_amulet", // generic item packet
        [PacketObjectTypes.ArmorBoots] = "item_amulet", // generic item packet
        [PacketObjectTypes.ArmorGloves] = "item_amulet", // generic item packet
        [PacketObjectTypes.ArmorBelt] = "item_amulet", // generic item packet
        [PacketObjectTypes.ArmorShield] = "item_amulet", // generic item packet
        [PacketObjectTypes.ArmorHelmet] = "item_amulet", // generic item packet
        [PacketObjectTypes.ArmorPants] = "item_amulet",
        [PacketObjectTypes.ArmorBracelet] = "item_amulet", // generic item packet
        [PacketObjectTypes.Ring] = "item_ring_half",
        [PacketObjectTypes.ArmorRobe] = "item_amulet", // item_robe_dragon_pa
        [PacketObjectTypes.RingGolem] = "",
        [PacketObjectTypes.AlchemyPot] = "item_alchemypot",
        [PacketObjectTypes.AlchemyFurnace] = "",
        [PacketObjectTypes.Blueprint] = "",
        [PacketObjectTypes.Workshop] = "workshop",
        [PacketObjectTypes.QuestArmorChest] = "", // generic item packet
        [PacketObjectTypes.QuestArmorChest2] = "", // generic item packet
        [PacketObjectTypes.QuestArmorBoots] = "item_quest_boots", // generic item packet
        [PacketObjectTypes.QuestArmorGloves] = "", // generic item packet
        [PacketObjectTypes.QuestArmorBelt] = "", // generic item packet
        [PacketObjectTypes.QuestArmorShield] = "item_quest_shield", // generic item packet
        [PacketObjectTypes.QuestArmorHelmet] = "item_quest_helmet", // generic item packet
        [PacketObjectTypes.QuestArmorPants] = "", // generic item packet
        [PacketObjectTypes.QuestArmorBracelet] = "", // generic item packet
        [PacketObjectTypes.QuestArmorRing] = "", // generic item packet
        [PacketObjectTypes.QuestArmorRobe] = "item_quest_robe", // generic item packet
        [PacketObjectTypes.QuestWeaponSword] = "", // generic item packet
        [PacketObjectTypes.QuestWeaponAxe] = "", // generic item packet
        [PacketObjectTypes.QuestWeaponCrossbow] = "item_quest_crossbow", // generic item packet
        [PacketObjectTypes.SpecialGuild] = "", // item_guild
        [PacketObjectTypes.SpecialAbility] = "",
        [PacketObjectTypes.SpecialAbilitySteal] = "",
        [PacketObjectTypes.ArmorHelmetPremium] = "", // generic item packet
        [PacketObjectTypes.MantraWhite] = "", //item_mantra_counted
        [PacketObjectTypes.MantraBlack] = "" //item_mantra_counted
        //Unknown
    };
}
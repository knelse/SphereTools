using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BitStreams;
using PacketLogViewer.Models;
using PacketLogViewer.Models.PacketAnalyzeData;
using SphereHelpers.Extensions;
using SpherePacketVisualEditor;
using SphServer.Helpers.Enums;

namespace PacketLogViewer;

public enum PacketTypes
{
    /*Originating from client*/
    CLIENT_LOGIN_DATA,
    CLIENT_SELECT_CHARACTER,
    CLIENT_DELETE_CHARACTER,
    CLIENT_CREATE_CHARACTER,
    CLIENT_PING,
    CLIENT_ATTACK_TARGET,
    CLIENT_SEND_CHAT_MESSAGE,
    CLIENT_MOVE_ITEM,

    /*Originating from server*/
    SERVER_CONNECTION_ACCEPTED,
    SERVER_RECONNECT_ATTEMPT,
    SERVER_TRANSMISSION_END,
    SERVER_CREDENTIALS,
    SERVER_CHARACTER_SELECT_SCREEN_INIT,
    SERVER_CHARACTER_SELECT_SCREEN_CONTENTS,
    SERVER_ENTER_GAME_WORLD_INIT,
    SERVER_ENTER_GAME_WORLD_CONTENTS,
    SERVER_CREATE_NEW_CHARACTER,
    SERVER_NAME_CHECK_OK,
    SERVER_ERROR_ACCOUNT_OUTDATED,
    SERVER_ERROR_NAME_EXISTS,
    SERVER_ERROR_ACCOUNT_IN_USE,
    SERVER_MOVE_INVENTORY_ITEM,
    SERVER_NEW_OBJECT,
    SERVER_SET_PLAYER_INVULNERABLE,
    SERVER_PING_6_SEC,
    SERVER_PING_15_SEC,
    SERVER_MOVE_ENTITY,
    SERVER_DESPAWN_ENTITY,
    SERVER_NEW_INSTANCED_ZONE,
    SERVER_TELEPORT_PLAYER,

    UNKNOWN
}

public enum PacketAnalyzeState
{
    UNDEF,
    NONE,
    PARTIAL,
    UNDEF_TYPE,
    FULL
}

public enum EntityActionType
{
    SET_POSITION = 0x06,
    FULL_SPAWN = 0x7C,
    ATTACK = 0x2A,
    DEATH = 0xA,
    UNKNOWN = 0x14,
    UNDEF
}

public static class PacketPartNames
{
    public const string ID = "entity_id";
    public const string EntityType = "entity_type";
    public const string ObjectType = "object_type";
    public const string MobType = "mob_type";
    public const string ActionType = "action_type";
    public const string CoordX = "x";
    public const string CoordY = "y";
    public const string CoordZ = "z";
    public const string Angle = "angle";
    public const string Delimiter = "delimiter";
    public const string CurrentHP = "current_hp";
    public const string MaxHP = "max_hp";
    public const string Level = "level";
    public const string NameID = "name_id";
    public const string TypeNameLength = "entity_type_name_length";
    public const string TypeName = "entity_type_name";
    public const string IconNameLength = "entity_name_length";
    public const string IconName = "entity_name";
}

internal class SubpacketBytesWithOffset
{
    public readonly byte[] Content;
    public readonly int ByteOffsetFromFullContentStart;
    public readonly byte[]? Header;

    public SubpacketBytesWithOffset (byte[] content, int byteOffsetFromFullContentStart, byte[]? header = null)
    {
        Content = content;
        ByteOffsetFromFullContentStart = byteOffsetFromFullContentStart;
        Header = header;
    }
}

internal static class PacketAnalyzer
{
    private static readonly byte[] packet_04_00_4F_01 = { 0x04, 0x00, 0xF4, 0x01 };
    private static readonly byte[] ok_mark = { 0x2c, 0x01, 0x00 };
    private static readonly Dictionary<string, Subpacket> SubpacketCache = new ();
    private static readonly Dictionary<string, PacketDefinition> PacketDefinitionCache = new ();

    public static readonly List<Func<byte[], bool>> ServerPacketHideRules = new ()
    {
        c => c.HasEqualElementsAs(packet_04_00_4F_01),
        c => c[0] == 0x08 && (c.Length < 8 || (c[6] == 0xF4 && c[7] == 0x01)),
        c => c[0] == 0x0C && (c.Length < 12 || (c[10] == 0x0D && c[11] == 0xE2)),
        c => c[0] == 0x12 && (c.Length < 17 || (c[14] == 0x1B && c[15] == 0x01 && c[16] == 0x60)),
        c => c[0] == 0x10 && (c.Length < 16 || (c[14] == 0x52 && c[15] == 0x09)),
        c => c[0] == 0x17 || c[0] == 0x1D || c[0] == 0x2D || c[0] == 0x22 || c[0] == 0x12 || c[0] == 0x0D,
        c => c[0] == 0x11 && (c.Length < 12 || (c[9] == 0x08 && c[10] == 0x40 && c[11] == 0x63)),
        c => c[0] == 0x0F && (c.Length < 14 || (c[12] == 0x84 && c[13] == 0x20)),
        c => c[0] == 0x10 && (c.Length < 9 || (c[7] == 0x00 && c[8] == 0x00)),
        c => c[0] == 0x76 && (c.Length < 12 || (c[9] == 0x08 && c[10] == 0x40 && c[11] == 0x63)) // file check
    };

    public static readonly List<Func<byte[], bool>> ClientPacketHideRules = new ()
    {
        c => c[0] == 0x26 || c[0] == 0x08 || c[0] == 0x0C,
        c => c[0] == 0x69 && c[13] == 0x08 && c[14] == 0x40 && c[15] == 0x63
    };

    internal static bool ShouldBeHiddenByDefault (StoredPacket storedPacket)
    {
        return storedPacket.Source switch
        {
            PacketSource.CLIENT => ShouldBeHiddenByDefaultClient(storedPacket),
            PacketSource.SERVER => ShouldBeHiddenByDefaultServer(storedPacket),
            _ => false
        };
    }

    private static bool ShouldBeHiddenByDefaultServer (StoredPacket storedPacket)
    {
        var content = storedPacket.ContentBytes;

        return ServerPacketHideRules.Any(ruleFunc => ruleFunc(content));
    }

    private static bool ShouldBeHiddenByDefaultClient (StoredPacket storedPacket)
    {
        var content = storedPacket.ContentBytes;

        return ClientPacketHideRules.Any(ruleFunc => ruleFunc(content));
    }

    public static bool IsClientPingPacket (StoredPacket storedPacket)
    {
        return storedPacket.Source == PacketSource.CLIENT && storedPacket.ContentBytes[0] == 0x26;
    }

    public static List<SubpacketBytesWithOffset> SplitContentIntoPackets (StoredPacket storedPacket,
        bool includeTrivialPackets = false)
    {
        var bytesWithoutHeaders = new List<SubpacketBytesWithOffset>();
        var offset = 0;
        while (offset < storedPacket.ContentBytes.Length)
        {
            if (storedPacket.ContentBytes.HasEqualElementsAs(packet_04_00_4F_01, offset))
            {
                if (includeTrivialPackets)
                {
                    bytesWithoutHeaders.Add(new SubpacketBytesWithOffset(packet_04_00_4F_01, offset));
                }

                offset += 4;
                continue;
            }

            if (!storedPacket.ContentBytes.HasEqualElementsAs(ok_mark, 2))
            {
                // already without header or something is wrong
                bytesWithoutHeaders.Add(new SubpacketBytesWithOffset(storedPacket.ContentBytes[offset..], offset + 7));
                break;
            }

            var subspanTotalLength = BitConverter.ToInt16(storedPacket.ContentBytes, offset);
            var start = offset + 7;
            var end = offset + subspanTotalLength;

            var header = storedPacket.ContentBytes[offset..start];

            bytesWithoutHeaders.Add(new SubpacketBytesWithOffset(storedPacket.ContentBytes[start..end], start, header));
            offset = end;
        }

        return bytesWithoutHeaders;
    }

    public static List<byte[]> SplitPacketIntoParts (StoredPacket storedPacket)
    {
        var bytesWithoutHeaders = SplitContentIntoPackets(storedPacket);

        var subspans = new List<byte[]>();
        var previousEntityId = 0;
        var previousEntityType = 0;
        var combinedList = new List<byte[]>();
        var writeStream = new BitStream(new MemoryStream())
        {
            AutoIncreaseStream = true
        };
        foreach (var subPacket in bytesWithoutHeaders)
        {
            var readStream = new BitStream(subPacket.Content);
            var entityId = readStream.ReadUInt16();
            readStream.ReadByte(2);
            var entityType = readStream.ReadUInt16(10);
            readStream.ReadByte(2);
            if (entityId == previousEntityId && entityType == previousEntityType)
            {
                // continuing the same data packet
                while (readStream.ValidPosition)
                {
                    writeStream.WriteBit(readStream.ReadBit());
                }
            }
            else
            {
                writeStream.Seek(0, 0);
                var writeResult = writeStream.GetStreamData();
                combinedList.Add(writeResult);
                writeStream = new BitStream(new MemoryStream())
                {
                    AutoIncreaseStream = true
                };
                writeStream.WriteUInt16(entityId);
                writeStream.WriteByte(0, 2);
                writeStream.WriteUInt16(entityType, 10);
                writeStream.WriteByte(0, 2);
                while (readStream.ValidPosition)
                {
                    writeStream.WriteBit(readStream.ReadBit());
                }

                previousEntityId = entityId;
                previousEntityType = entityType;
            }
        }

        writeStream.Seek(0, 0);
        var lastResult = writeStream.GetStreamData();
        combinedList.Add(lastResult);

        var tradeEntities = new HashSet<int>
        {
            (int) ObjectType.NpcTrade
        };
        var containerEntities = new HashSet<int>
        {
            (int) ObjectType.Chest,
            (int) ObjectType.Sack,
            (int) ObjectType.SackMobLoot,
            (int) ObjectType.MantraBookSmall,
            (int) ObjectType.MantraBookLarge,
            (int) ObjectType.MantraBookGreat,
            (int) ObjectType.AlchemyPot,
            (int) ObjectType.BackpackLarge,
            (int) ObjectType.BackpackSmall,
            (int) ObjectType.MapBook,
            (int) ObjectType.RecipeBook
        };
        foreach (var subPacket in combinedList)
        {
            var stream = new BitStream(subPacket);
            stream.ReadUInt16();
            stream.ReadByte(2);
            var entityType = stream.ReadUInt16(10);
            stream.ReadByte(2);
            if (tradeEntities.Contains(entityType) || containerEntities.Contains(entityType))
            {
                // don't split these, we (mostly) know the structure, just find the end
                var offsetAfterItems = (long) 0;
                var bitAfterItems = 0;
                var separatorLength = tradeEntities.Contains(entityType) ? 15 : 23;
                var separator = tradeEntities.Contains(entityType) ? 0x600A : 0x40105;
                while (stream.ValidPosition)
                {
                    var separatorTest = stream.ReadUInt32(separatorLength);
                    if (!stream.ValidPosition)
                    {
                        break;
                    }

                    if (separatorTest == separator)
                    {
                        offsetAfterItems = stream.Offset;
                        bitAfterItems = stream.Bit;
                    }
                    else
                    {
                        stream.SeekBack(separatorLength - 1);
                    }
                }

                if (offsetAfterItems != 0)
                {
                    stream.Seek(offsetAfterItems, bitAfterItems);
                    stream.ReadBits(tradeEntities.Contains(entityType) ? 96 : 64);
                    offsetAfterItems = stream.Offset;
                    bitAfterItems = stream.Bit;
                    var subspan = stream.GetStreamDataBetween(0, 0, offsetAfterItems, bitAfterItems);
                    subspans.Add(subspan);
                    if (stream.Length - stream.Offset < 5)
                    {
                        continue;
                    }

                    stream.SeekBack(8);
                }
                else
                {
                    stream.Seek(0, 0);
                }
            }
            else
            {
                stream.Seek(0, 0);
            }

            var previousOffset = stream.Offset;
            var previousBit = stream.Bit;
            while (stream.ValidPosition)
            {
                var splitTest1 = stream.ReadByte();
                if (!stream.ValidPosition)
                {
                    break;
                }

                if (splitTest1 == 0x00)
                {
                    var splitTest2 = stream.ReadByte(7);
                    if (!stream.ValidPosition)
                    {
                        break;
                    }

                    if (splitTest2 != 0x3F && splitTest2 != 0x7E)
                    {
                        stream.SeekBack(14);
                        continue;
                    }

                    stream.SeekBack(15);
                    if (stream.Offset != previousOffset)
                    {
                        var subspan =
                            stream.GetStreamDataBetween(previousOffset, previousBit, stream.Offset, stream.Bit);
                        subspans.Add(subspan);
                    }

                    stream.ReadBits(15);
                    previousOffset = stream.Offset;
                    previousBit = stream.Bit;
                }
                else if (splitTest1 == 0xFF)
                {
                    var splitTest2 = stream.ReadBytes(31);
                    if (!stream.ValidPosition)
                    {
                        break;
                    }

                    if (splitTest2[0] != 0xFF || splitTest2[1] != 0xFF || splitTest2[2] != 0xFF ||
                        splitTest2[3] != 0x3F)
                    {
                        stream.SeekBack(30);
                        continue;
                    }

                    var newOffset = stream.Offset - 5;
                    var newBit = stream.Bit + 1;
                    if (newBit >= 8)
                    {
                        newBit = 0;
                        newOffset++;
                    }

                    var subspan = stream.GetStreamDataBetween(previousOffset, previousBit, newOffset, newBit);
                    subspans.Add(subspan);
                    previousOffset = stream.Offset;
                    previousBit = stream.Bit;
                }
                else
                {
                    stream.SeekBack(7);
                }
            }

            if (previousOffset < stream.Length)
            {
                var subspan = stream.GetStreamDataBetween(previousOffset, previousBit, int.MaxValue, 0);
                subspans.Add(subspan);
            }
        }

        return subspans;
    }

    internal static List<byte[]> SplitIntoItemSlots (BitStream stream, int separator, int separatorBitCount)
    {
        var results = new List<byte[]>();
        var previousOffset = (long) 0;
        var previousBit = 0;
        stream.Seek(0, 0);

        while (stream.ValidPosition)
        {
            var test = stream.ReadUInt32(separatorBitCount);
            if (!stream.ValidPosition)
            {
                break;
            }

            if (test != separator)
            {
                stream.SeekBack(separatorBitCount - 1);
                continue;
            }

            if (previousOffset == 0)
            {
                previousOffset = stream.Offset;
                previousBit = stream.Bit;
                continue;
            }

            stream.SeekBack(separatorBitCount);
            results.Add(stream.GetStreamDataBetween(previousOffset, previousBit, stream.Offset, stream.Bit));
            stream.ReadBytes(separatorBitCount);
            previousOffset = stream.Offset;
            previousBit = stream.Bit;
        }

        if (results.Any())
        {
            // last item won't be added
            stream.Seek(previousOffset, previousBit);
            var bitCount = separator == 0x600A ? 96 : 64;
            results.Add(stream.ReadBytes(bitCount));
        }

        return results;
    }

    internal static string GetTextOutputForPacket (byte[] contents)
    {
        if (contents.Length < 5)
        {
            return string.Empty;
        }

        if (contents.HasEqualElementsAs(ok_mark, 2))
        {
            // len_1 len_2 2c 01 00 sync_1 sync_2
            contents = contents[7..];
        }

        var stream = new BitStream(contents);
        var analyzeResult = new List<Dictionary<string, object>>();

        var entityId = stream.ReadUInt16();
        stream.ReadByte(2);
        var entityType = stream.ReadUInt16(10);
        var entityTypeName = Enum.GetName(typeof (ObjectType), entityType) ?? "(undef)";
        var tradeEntities = new HashSet<int>
        {
            (int) ObjectType.NpcTrade
        };
        var containerEntities = new HashSet<int>
        {
            (int) ObjectType.Chest,
            (int) ObjectType.Sack,
            (int) ObjectType.SackMobLoot,
            (int) ObjectType.MantraBookSmall,
            (int) ObjectType.MantraBookLarge,
            (int) ObjectType.MantraBookGreat,
            (int) ObjectType.AlchemyPot,
            (int) ObjectType.BackpackLarge,
            (int) ObjectType.BackpackSmall,
            (int) ObjectType.MapBook,
            (int) ObjectType.RecipeBook
        };
        stream.ReadByte(2);
        var output = new StringBuilder("\n");
        if (tradeEntities.Contains(entityType))
        {
            var splittedBySeparator = SplitIntoItemSlots(stream, 0x600A, 15);
            if (!splittedBySeparator.Any())
            {
                output.AppendLine("[EMPTY]");
            }

            foreach (var splitted in splittedBySeparator)
            {
                var splitStream = new BitStream(splitted);
                var itemSlot = splitStream.ReadByte();
                var itemId = splitStream.ReadUInt16();
                var skip = splitStream.ReadByte();
                var weight = splitStream.ReadUInt32();
                var cost = splitStream.ReadUInt32();
                analyzeResult.Add(new Dictionary<string, object>
                {
                    ["ItemId"] = itemId,
                    ["ItemSlot"] = itemSlot,
                    ["Weight"] = weight,
                    ["Skip"] = skip,
                    ["Cost"] = cost
                });
                output.AppendLine($"{itemSlot:0#}: {itemId:X4} ({cost}t), {weight} u");
            }
        }
        else if (containerEntities.Contains(entityType))
        {
            var splittedBySeparator = SplitIntoItemSlots(stream, 0x40105, 23);
            if (!splittedBySeparator.Any())
            {
                output.AppendLine("[EMPTY]");
            }

            foreach (var splitted in splittedBySeparator)
            {
                var splitStream = new BitStream(splitted);
                var itemSlot = splitStream.ReadByte();
                var itemId = splitStream.ReadUInt16();
                var skip = splitStream.ReadByte();
                var weight = splitStream.ReadUInt32();
                analyzeResult.Add(new Dictionary<string, object>
                {
                    ["ItemId"] = itemId,
                    ["ItemSlot"] = itemSlot,
                    ["Weight"] = weight,
                    ["Skip"] = skip
                });
                output.AppendLine($"{itemSlot:0#}: {itemId:X4}, {weight} u");
            }
        }

        return $"ID: {entityId:X4} ({entityType}, {entityTypeName})\n{output}";
    }

    public static StoredPacket AppendAnalyticsData (this StoredPacket storedPacket)
    {
        var stream = new BitStream(storedPacket.ContentBytes);
        var id = stream.ReadUInt16();
        storedPacket.TargetId = id;
        stream.ReadByte(2);
        var objectTypeVal = stream.ReadUInt16(10);
        var objectType = Enum.IsDefined(typeof (ObjectType), objectTypeVal)
            ? (ObjectType?) objectTypeVal
            : null;
        storedPacket.ObjectType = objectType;
        storedPacket.PacketType = objectTypeVal switch
        {
            0 => PacketTypes.SERVER_DESPAWN_ENTITY,
            _ => null
        };
        if (storedPacket.PacketType == PacketTypes.SERVER_DESPAWN_ENTITY)
        {
            storedPacket.HiddenByDefault = true;
        }

        return storedPacket;
    }

    public static StoredPacket UpdatePacketPartsForContent (this StoredPacket storedPacket)
    {
        if (storedPacket.Source == PacketSource.CLIENT)
        {
            // TODO
            return storedPacket;
        }

        storedPacket.AnalyzeState = PacketAnalyzeState.NONE;

        var packetsInContent = SplitContentIntoPackets(storedPacket, true);
        var allParts = new List<PacketPart>();
        var undefTypes = false;
        var typesInside = new List<ObjectType>();
        var hpByLevel = new List<KeyValuePair<int, int>>();
        var shouldHidePacket = true;
        var subPacketIndex = -1;
        var fullStream = new BitStream(storedPacket.ContentBytes);
        foreach (var packetInContent in packetsInContent)
        {
            var bitOffsetFromPreviousSubpackets = 0;
            if (packetInContent.Header is not null)
            {
                fullStream.SeekBitOffset((packetInContent.ByteOffsetFromFullContentStart - 7) * 8);
                var header = FindPartsByNameSkipLastUndefSetCommentUpdateBitOffset(
                    fullStream, "server_packet_header", (int) fullStream.BitOffsetFromStart, subPacketIndex + 1,
                    "NEXT PACKET");
                allParts.AddRange(header);
            }

            while (fullStream.ValidPosition)
            {
                subPacketIndex++;
                var initialBitOffset = (int) fullStream.BitOffsetFromStart;
                var test1 = fullStream.ReadBytes(4, true);
                var breakAfterCurrentTry = false;
                if (test1.HasEqualElementsAs(packet_04_00_4F_01))
                {
                    var parts = FindPartsByNameSkipLastUndefSetCommentUpdateBitOffset(fullStream, "0x0400F401",
                        initialBitOffset, subPacketIndex);
                    allParts.AddRange(parts);
                    if (!fullStream.ValidPosition)
                    {
                        break;
                    }

                    continue;
                }

                fullStream.SeekBitOffset(initialBitOffset);

                // try to find entity id and object type
                var entId = fullStream.ReadUInt16();
                if (!fullStream.ValidPosition)
                {
                    break;
                }

                fullStream.ReadBits(2);
                if (!fullStream.ValidPosition)
                {
                    break;
                }

                var objectTypeVal = fullStream.ReadUInt16(10);
                if (!fullStream.ValidPosition)
                {
                    break;
                }

                var objectType = Enum.IsDefined(typeof (ObjectType), objectTypeVal)
                    ? (ObjectType) objectTypeVal
                    : ObjectType.Unknown;
                if (objectType != ObjectType.Unknown)
                {
                    typesInside.Add(objectType);
                }

                var currentParts = new List<PacketPart>();
                var typeWithDelimiter = false;
                switch (objectType)
                {
                    case ObjectType.Despawn:
                        var despawn = FindPartsByNameSkipLastUndefSetCommentUpdateBitOffset(fullStream, "despawn",
                            initialBitOffset, subPacketIndex, $"DESPAWN: {entId:X4}");
                        currentParts.AddRange(despawn);
                        typeWithDelimiter = true;
                        break;
                    case ObjectType.MobSpawner:
                    case ObjectType.SackMobLoot:
                    case ObjectType.Monster:
                    case ObjectType.MonsterFlyer:
                    case ObjectType.NpcTrade:
                    case ObjectType.ChestInDungeon:
                    case ObjectType.DoorEntrance:
                    case ObjectType.DoorExit:
                        fullStream.ReadBit();
                        var actionTypeVal = fullStream.ReadByte();
                        var actionType = Enum.IsDefined(typeof (EntityActionType), (int) actionTypeVal)
                            ? (EntityActionType) actionTypeVal
                            : EntityActionType.UNDEF;
                        fullStream.SeekBack(9);
                        var (success, parts) = GetNewEntityPacketParts(fullStream, objectType,
                            initialBitOffset, entId, actionType, subPacketIndex);
                        currentParts.AddRange(parts);
                        if (success)
                        {
                            if (objectType is ObjectType.Monster or ObjectType.MonsterFlyer or ObjectType.NpcTrade
                                or ObjectType.NpcQuestDegree or ObjectType.NpcQuestTitle)
                            {
                                shouldHidePacket = false;
                            }

                            typeWithDelimiter = true;
                        }
                        else
                        {
                            undefTypes = true;
                            breakAfterCurrentTry = false;
                        }

                        break;
                    default:
                        undefTypes = true;
                        typeWithDelimiter = false;
                        break;
                }

                allParts.AddRange(currentParts);
                if (breakAfterCurrentTry)
                {
                    break;
                }

                if (typeWithDelimiter)
                {
                    var currentFullStreamPosition = (int) fullStream.BitOffsetFromStart;
                    var delimTest = fullStream.ReadByte();
                    if (!fullStream.ValidPosition)
                    {
                        break;
                    }

                    if (delimTest != 0x7E && delimTest != 0x7F)
                    {
                        fullStream.SeekBack(8);
                    }
                    else
                    {
                        // fullStream.SeekBitOffset(currentFullStreamPosition);
                        subPacketIndex++;
                        var delimiter = FindPartsByNameSkipLastUndefSetCommentUpdateBitOffset(fullStream, "delimiter",
                            currentFullStreamPosition, subPacketIndex, PacketPart.UndefinedFieldValue);
                        allParts.AddRange(delimiter);
                        continue;
                    }
                }

                var header = FindPartsByNameSkipLastUndefSetCommentUpdateBitOffset(fullStream, "entity_header",
                    initialBitOffset, subPacketIndex,
                    $"UNKNOWN TYPE: {objectType} ({objectTypeVal})");
                allParts.AddRange(header);
                break;
            }
        }

        storedPacket.PacketParts = allParts;
        if (allParts.Any())
        {
            storedPacket.AnalyzeState = undefTypes ? PacketAnalyzeState.UNDEF_TYPE : PacketAnalyzeState.PARTIAL;
        }

        if (shouldHidePacket)
        {
            storedPacket.HiddenByDefault = true;
        }

        AddPacketPartAnalyzeData(storedPacket);

        return storedPacket;
    }

    private static Tuple<bool, List<PacketPart>> GetNewEntityPacketParts (BitStream stream, ObjectType objectType,
        int initialBitOffset, ushort entId, EntityActionType actionType, int subpacketIndex)
    {
        var packetName = string.Empty;
        var entityNameForComment = CamelCaseToUpperWithSpaces(objectType.ToString());
        var success = true;
        var comment = (string?) null;

        if (actionType == EntityActionType.SET_POSITION)
        {
            packetName = "mob_0x06";
            comment = $"ENTITY MOVES [{entId:X4}])";
        }
        else if (actionType == EntityActionType.ATTACK)
        {
            packetName = "change_target_health";
            comment = $"ENTITY DEALS DAMAGE [{entId:X4}]";
        }
        else if (actionType == EntityActionType.DEATH)
        {
            packetName = "entity_killed";
            comment = $"ENTITY KILLED [{entId:X4}]";
        }
        else if (actionType == EntityActionType.UNKNOWN)
        {
            packetName = "action_0x14";
            comment = $"ENTITY DOING 0x14 [{entId:X4}]";
        }
        // assuming full
        else
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
                default:
                    success = false;
                    break;
            }
        }

        comment ??= $"NEW ENTITY -- {entityNameForComment} [{entId:X4}]";

        return packetName == string.Empty
            ? new Tuple<bool, List<PacketPart>>(success, new List<PacketPart>())
            : new Tuple<bool, List<PacketPart>>(success,
                FindPartsByNameSkipLastUndefSetCommentUpdateBitOffset(stream, packetName, initialBitOffset,
                    subpacketIndex, comment));
    }

    private static StoredPacket AddPacketPartAnalyzeData (this StoredPacket storedPacket)
    {
        storedPacket.AnalyzeResult.Clear();
        var partsBySubpacket = new Dictionary<int, List<PacketPart>>();
        storedPacket.PacketParts.ForEach(part =>
        {
            if (!partsBySubpacket.ContainsKey(part.SubpacketIndex))
            {
                partsBySubpacket.Add(part.SubpacketIndex, new List<PacketPart>());
            }

            partsBySubpacket[part.SubpacketIndex].Add(part);
        });

        foreach (var key in partsBySubpacket.Keys)
        {
            if (partsBySubpacket[key].Count == 1 && partsBySubpacket[key].First().Name == PacketPartNames.Delimiter)
            {
                continue;
            }

            storedPacket.AnalyzeResult.Add(GetAnalyzeDataForSubpacket(partsBySubpacket[key]));
        }

        return storedPacket;
    }

    private static PacketAnalyzeData GetAnalyzeDataForSubpacket (List<PacketPart> subpacket)
    {
        var result = new PacketAnalyzeData(subpacket);
        if (result.ObjectType is ObjectType.Monster or ObjectType.MonsterFlyer or ObjectType.MobSpawner)
        {
            result = new MobPacket(subpacket);
        }

        if (result.ObjectType is ObjectType.Despawn)
        {
            result = new DespawnPacket(subpacket);
        }

        if (result.ObjectType is ObjectType.NpcTrade)
        {
            result = new NpcTradePacket(subpacket);
        }

        return result;
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

    private static List<PacketPart> FindPartsByName (BitStream stream, string name, bool isSubpacket, int bitOffset)
    {
        if (isSubpacket)
        {
            var subpacket = PacketLogViewerMainWindow.Subpackets.FirstOrDefault(x => x.Name == name);
            if (subpacket is null)
            {
                return new List<PacketPart>();
            }

            return subpacket.LoadFromFile(stream, bitOffset);
        }

        var definition = PacketLogViewerMainWindow.PacketDefinitions.FirstOrDefault(x => x.Name == name);
        if (definition is null)
        {
            return new List<PacketPart>();
        }

        return definition.LoadFromFile(stream, bitOffset);
    }

    private static List<PacketPart> FindPartsByNameSkipLastUndefSetCommentUpdateBitOffset (BitStream stream,
        string name, int bitOffsetFromStart, int subpacketIndex, string? comment = null, bool isSubpacket = true)
    {
        var parts = FindPartsByName(stream, name, isSubpacket, bitOffsetFromStart);
        if (!parts.Any())
        {
            return parts;
        }

        comment ??= name;
        parts[0].Comment = comment;
        foreach (var t in parts)
        {
            t.SubpacketIndex = subpacketIndex;
        }

        return parts;
    }
}
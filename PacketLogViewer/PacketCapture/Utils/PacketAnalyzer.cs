using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitStreams;
using PacketLogViewer.Models;

namespace PacketLogViewer;

internal enum PacketTypes
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
    SERVER_TELEPORT_PLAYER
}

internal static class PacketAnalyzer
{
    private static readonly byte[] packet_04_00_4F_01 = { 0x04, 0x00, 0xF4, 0x01 };
    private static readonly byte[] ok_mark = { 0x2c, 0x01, 0x00 };

    public static readonly List<Func<byte[], bool>> ServerPacketHideRules = new ()
    {
        c => ObjectPacketTools.ByteArrayCompare(c, packet_04_00_4F_01),
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

    public static List<byte[]> SplitPacketIntoParts (StoredPacket storedPacket)
    {
        var bytesWithoutHeaders = new List<byte[]>();
        var offset = 0;
        while (offset < storedPacket.ContentBytes.Length)
        {
            var subspanTotalLength = BitConverter.ToInt16(storedPacket.ContentBytes, offset);
            var start = offset + 7;
            var end = offset + subspanTotalLength;
            if (end < start)
            {
                break;
            }

            bytesWithoutHeaders.Add(storedPacket.ContentBytes[start..end]);
            offset = end;
        }

        var subspans = new List<byte[]>();
        foreach (var subPacket in bytesWithoutHeaders)
        {
            var stream = new BitStream(subPacket);
            var previousOffset = (long) 0;
            var previousBit = 0;
            while (stream.ValidPosition)
            {
                var splitTest1 = stream.ReadByte();
                if (!stream.ValidPosition)
                {
                    break;
                }

                if (splitTest1 == 0x00)
                {
                    var splitTest2 = stream.ReadByte();
                    if (!stream.ValidPosition)
                    {
                        break;
                    }

                    if (splitTest2 != 0x3F && splitTest2 != 0x7E)
                    {
                        stream.SeekBack(15);
                        continue;
                    }

                    var subspan =
                        stream.GetStreamDataBetween(previousOffset, previousBit, stream.Offset - 2, stream.Bit);
                    subspans.Add(subspan);
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

    public static List<CapturedPacketRawData> TryAnalyzePacketContents (CapturedPacketRawData capturedPacketRawData)
    {
        var decoded = capturedPacketRawData.DecodedBuffer;
        var stream = new BitStream(capturedPacketRawData.DecodedBuffer);

        var results = new List<CapturedPacketRawData> { capturedPacketRawData };
        // stream.ReadBytes(6, true);
        // while (stream.ValidPosition)
        // {
        //     var packetTarget = stream.ReadUInt16();
        //     var entityRemoved = SkipZeroesTryFindPacketSplitter(stream);
        //     if (entityRemoved)
        //     {
        //         continue;
        //     }
        //
        //     stream.ReadByte(2);
        //     var objectType = stream.ReadUInt16(10);
        //     Console.WriteLine($"{packetTarget:X4} {objectType}");
        // }

        return results;
    }

    private static bool SkipZeroesTryFindPacketSplitter (BitStream stream)
    {
        var count = 0;
        var test = stream.ReadByte(1);
        while (test == 0x00 && stream.ValidPosition)
        {
            count++;
            test = stream.ReadByte(1);
        }

        if (!stream.ValidPosition)
        {
            stream.SeekBack(count);
            return false;
        }

        var test2 = stream.ReadByte(7);
        if (test2 == 0x3F || test2 == 0x7E)
        {
            return true;
        }

        stream.SeekBack(count + 7);
        return false;
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

        if (ObjectPacketTools.ByteArrayCompare(contents, new byte[] { 0x2c, 0x01, 0x00 }, 2))
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
        stream.ReadByte(2);
        var action1 = stream.ReadByte();
        var action2 = stream.ReadByte();
        var output = new StringBuilder("\n");
        if (action1 == 0x85 && action2 == 0x81)
        {
            // container item list
            stream.ReadUInt16(15);
            var itemSeparator = stream.ReadUInt16(15);
            var inTrade = itemSeparator == 0x600A;
            if (inTrade)
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
            else
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
        }

        return $"ID: {entityId:X4} ({entityType}, {entityTypeName})\n{output}";
    }
}
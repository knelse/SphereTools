using System;
using System.Collections.Generic;
using System.Linq;
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

    public static readonly List<Func<byte[], bool>> ServerPacketHideRules = new ()
    {
        c => ObjectPacketTools.ByteArrayCompare(c, packet_04_00_4F_01),
        c => c[0] == 0x08 && c[6] == 0xF4 && c[7] == 0x01,
        c => c[0] == 0x0C && c[10] == 0x0D && c[11] == 0xE2,
        c => c[0] == 0x12 && c[14] == 0x1B && c[15] == 0x01 && c[16] == 0x60,
        c => c[0] == 0x10 && c[14] == 0x52 && c[15] == 0x09,
        c => c[0] == 0x17 || c[0] == 0x1D || c[0] == 0x2D || c[0] == 0x22 || c[0] == 0x12 || c[0] == 0x0D,
        c => c[0] == 0x11 && c[9] == 0x08 && c[10] == 0x40 && c[11] == 0x63,
        c => c[0] == 0x0F && c[12] == 0x84 && c[13] == 0x20,
        c => c[0] == 0x10 && c[7] == 0x00 && c[8] == 0x00,
        c => c[0] == 0x76 && c[9] == 0x08 && c[10] == 0x40 && c[11] == 0x63 // file check
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
}
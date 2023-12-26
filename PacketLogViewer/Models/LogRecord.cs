using System;
using LiteDB;

namespace PacketLogViewer.Models;

public class LogRecord
{
    public LogRecord (StoredPacket storedPacket)
    {
        Id = storedPacket.Id;
        Source = storedPacket.Source;
        Timestamp = storedPacket.Timestamp;
        ContentBytes = storedPacket.ContentBytes;
        ContentString = Convert.ToHexString(ContentBytes);
        ContentJson = storedPacket.ContentJson;
        Favorite = storedPacket.Favorite;
        HiddenByDefault = storedPacket.HiddenByDefault || PacketAnalyzer.ShouldBeHiddenByDefault(storedPacket);
        PacketType = storedPacket.PacketType;
        TargetId = storedPacket.TargetId;
        ObjectType = storedPacket.ObjectType;
    }

    public int Id { get; set; }
    public PacketSource Source { get; set; }
    public DateTime Timestamp { get; set; }
    public byte[] ContentBytes { get; set; }
    public string ContentJson { get; set; }

    [BsonIgnore] public string ContentString { get; set; }

    public bool Favorite { get; set; }
    public bool HiddenByDefault { get; set; }
    public PacketTypes? PacketType { get; set; }
    public ushort TargetId { get; set; }
    public ObjectType? ObjectType { get; set; }
}
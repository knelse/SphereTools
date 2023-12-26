using System;
using LiteDB;

namespace PacketLogViewer.Models;

public class StoredPacket
{
    [BsonId] public int Id { get; set; }
    public byte[] ContentBytes { get; set; }
    public string ContentJson { get; set; }
    public PacketSource Source { get; set; }
    public DateTime Timestamp { get; set; }
    public bool HiddenByDefault { get; set; }
    public bool Favorite { get; set; }
    public PacketTypes? PacketType { get; set; }
    public ushort TargetId { get; set; }
    public ObjectType? ObjectType { get; set; }
}
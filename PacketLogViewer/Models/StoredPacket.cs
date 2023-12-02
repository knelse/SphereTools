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
}
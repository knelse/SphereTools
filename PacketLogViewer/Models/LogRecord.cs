using System;
using LiteDB;

namespace PacketLogViewer.Models;

public class LogRecord
{
    public LogRecord (StoredPacket storedPacket)
    {
        Source = storedPacket.Source;
        Timestamp = storedPacket.Timestamp;
        ContentBytes = storedPacket.ContentBytes;
        ContentString = Convert.ToHexString(ContentBytes);
        ContentJson = storedPacket.ContentJson;
        Favorite = false;
        HiddenByDefault = PacketAnalyzer.ShouldBeHiddenByDefault(storedPacket);
    }

    public PacketSource Source { get; set; }
    public DateTime Timestamp { get; set; }
    public byte[] ContentBytes { get; set; }
    public string ContentJson { get; set; }

    [BsonIgnore] public string ContentString { get; set; }

    public bool Favorite { get; set; }
    public bool HiddenByDefault { get; set; }
}
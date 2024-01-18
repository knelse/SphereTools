using System;
using System.Collections.Generic;
using SpherePacketVisualEditor;

namespace PacketLogViewer.Models.PacketAnalyzeData;

public class DespawnPacket : PacketAnalyzeData
{
    public string DisplayValue => $"{Id:X4} ({Enum.GetName(ObjectType) ?? string.Empty})";

    public DespawnPacket (List<PacketPart> parts) : base(parts)
    {
    }
}
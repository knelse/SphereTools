using System.Collections.Generic;
using BitStreams;

namespace SpherePacketVisualEditor;

public class PacketDefinition
{
    public string Name { get; set; }
    public string FilePath { get; set; }

    public List<PacketPart> LoadFromFile (BitStream stream, int bitOffset)
    {
        return PacketPart.LoadFromFile(FilePath, Name, stream, bitOffset);
    }
}
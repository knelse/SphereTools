using System.Collections.Generic;

namespace SpherePacketVisualEditor;

public class Subpacket
{
    public List<PacketPart> PacketParts = new ();
    public string Name { get; set; }
    public string FilePath { get; set; }

    public void LoadFromFile ()
    {
        PacketParts = PacketPart.LoadFromFile(FilePath, Name);
    }
}
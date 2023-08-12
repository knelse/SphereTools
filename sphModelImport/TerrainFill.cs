using System.IO;
using System.Text;
using Godot;

public partial class TerrainFill : GridMap
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready ()
    {
        Clear();
        using var mapFile = File.OpenRead(@"c:\Games\Sfera_std\landscape\map.bin");
        using var reader = new BinaryReader(mapFile);
        var terrainIndex = -1;
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            var geometryNameBytes = reader.ReadBytes(20);
            var variantBytes = reader.ReadBytes(2);
            terrainIndex++;

            var variant = $"{variantBytes[0]:0}{variantBytes[1]:0}";
            var sb = new StringBuilder();
            foreach (var nameByte in geometryNameBytes)
            {
                if (nameByte == 0)
                {
                    break;
                }

                sb.Append((char) nameByte);
            }

            var geometryName = sb.ToString().ToLower() + "_" + variant;
            var meshIndex = MeshLibrary.FindItemByName(geometryName);

            SetCellItem(new Vector3I(terrainIndex % 80, 0, terrainIndex / 80), meshIndex, 22);
        }

        var scene = new PackedScene();
        scene.Pack(this);
        ResourceSaver.Save(scene, "TerrainGrid.tscn");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process (double delta)
    {
    }
}
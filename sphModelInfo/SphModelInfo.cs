using System.Text;

// ReSharper disable CollectionNeverQueried.Global

namespace sphModelInfo;

public class SphModelInfo
{
    public const int FirstTextureNameIndex = 0x102;
    public static readonly byte[] SphModelMagicBytes = { 0x4d, 0x44, 0x4c, 0x21 }; // MDL!
    public readonly List<byte> ObjectIndices = new ();
    public readonly List<Object3D> Objects = new ();
    public readonly List<Surface> Surfaces = new ();
    public readonly List<string> TextureNames = new ();
    public readonly List<Triangle> Triangles = new ();
    public readonly List<Vertex> Vertices = new ();
    public ModelHeader Header;

    public string Name;

    public static SphModelInfo? GetModelInfo (string filePath)
    {
        using var file = File.OpenRead(filePath);
        using var reader = new BinaryReader(file, Encoding.ASCII);
        var magicBytes = reader.ReadBytes(4);

        if (magicBytes[0] != SphModelMagicBytes[0]
            || magicBytes[1] != SphModelMagicBytes[1]
            || magicBytes[2] != SphModelMagicBytes[2]
            || magicBytes[3] != SphModelMagicBytes[3])
        {
            Console.WriteLine($"Skipping: {filePath}");

            return null;
        }

        var modelInfo = new SphModelInfo
        {
            Name = Path.GetFileNameWithoutExtension(filePath),
            Header = reader.ReadStruct<ModelHeader>()
        };

        file.Seek(FirstTextureNameIndex, SeekOrigin.Begin);

        for (var i = 0; i < modelInfo.Header.TextureCount; i++)
        {
            var textureNameLength = reader.ReadByte();
            var textureNameBytes = reader.ReadBytes(textureNameLength);
            var textureName = Encoding.ASCII.GetString(textureNameBytes);
            modelInfo.TextureNames.Add(textureName);
        }

        for (var i = 0; i < modelInfo.Header.VertexCount; i++)
        {
            modelInfo.Vertices.Add(reader.ReadStruct<Vertex>());
        }

        for (var i = 0; i < modelInfo.Header.TriangleCount; i++)
        {
            modelInfo.Triangles.Add(reader.ReadStruct<Triangle>());
        }

        for (var i = 0; i < modelInfo.Header.SurfaceCount; i++)
        {
            modelInfo.Surfaces.Add(reader.ReadStruct<Surface>(15));
        }

        for (var i = 0; i < modelInfo.Header.ObjectCount; i++)
        {
            modelInfo.Objects.Add(reader.ReadStruct<Object3D>(39));
        }

        for (var i = 0; i < modelInfo.Header.ObjectIndexCount; i++)
        {
            modelInfo.ObjectIndices.Add(reader.ReadByte());
        }

        return modelInfo;
    }
}
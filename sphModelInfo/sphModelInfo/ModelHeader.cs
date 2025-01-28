// ReSharper disable UnassignedField.Global

namespace sphModelInfo;

public struct ModelHeader
{
    public ushort VertexCount;
    public ushort TriangleCount;
    public ushort SurfaceCount;
    public byte TextureCount;
    public ushort TextureNamesLength;
    public byte ObjectCount;
    public ushort ObjectIndexCount;
    public ushort TransformKeyCount;
    public ushort TransformKeyIndexCount;
    public byte ActionCount;
}
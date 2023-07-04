using System.Runtime.InteropServices;

namespace sphModelInfo;

public struct Surface
{
    public byte ObjectIndex;
    public byte TextureIndex;
    public short FirstTriangleIndex;
    public short TriangleCount;
    public short FirstVertexIndex;
    public short VertexCount;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public byte[] Reserved;
}
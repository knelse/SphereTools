// ReSharper disable UnassignedField.Global
using System.Runtime.InteropServices;

namespace sphModelInfo;

public struct Object3D
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public char[] Name;
    public byte BoneType;
    public byte ConnectedBoneCount;
    public byte ObjectIndexOffset;
    public byte IsAnimated;
    public short KeyIndex;
    public byte ParentIndex;
}
using System.Runtime.InteropServices;

namespace sphModelInfo;

public static class BinaryReaderExtensions
{
    public static T ReadStruct<T>(this BinaryReader reader, int size = int.MinValue) where T : struct
    {
        var buffer = reader.ReadBytes(size == int.MinValue ? Marshal.SizeOf<T>() : size);
        var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        var obj = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
        handle.Free();
        return obj;
    }
}
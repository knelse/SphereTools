using System;

namespace PacketLogViewer;

internal class CapturedPacketRawData
{
    internal DateTime ArrivalTime;
    internal byte[] Buffer;
    internal byte[] DecodedBuffer;
    internal PacketSource Source;
    internal bool WasProcessed = false;

    internal static int Compare (CapturedPacketRawData self, CapturedPacketRawData other)
    {
        if (self.DecodedBuffer.Length < 7 || other.DecodedBuffer.Length < 7)
        {
            return -1;
        }

        return GetPacketNumberInSequence(self.DecodedBuffer).CompareTo(GetPacketNumberInSequence(other.DecodedBuffer));
    }

    internal static int GetPacketNumberInSequence (byte[] buffer)
    {
        return (buffer[7] << 8) + buffer[6];
    }
}
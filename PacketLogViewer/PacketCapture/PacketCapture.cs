using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PacketDotNet;
using PacketLogViewer.Extensions;
using SharpPcap;

namespace PacketLogViewer;

internal enum PacketSource
{
    CLIENT,
    SERVER
}

public class PacketCapture
{
    private const int sphereLiveServerPort = 25860;

    private readonly List<byte> packetDataQueue = new ();

    private readonly List<CapturedPacketRawData> rawCapturedPackets = new ();

    private readonly HashSet<IPAddress> sphereLiveServers = new ()
    {
        IPAddress.Parse("77.223.107.68"),
        IPAddress.Parse("77.223.107.69")
    };

    public PacketCapture (string macAddress = "C87F54061FF1")
    {
        var captureDevice = CaptureDeviceList.Instance.FirstOrDefault(x => x.MacAddress?.ToString() == macAddress);
        if (captureDevice is null)
        {
            ConsoleExtensions.WriteLineColored($"ERROR: Capture device with mac address {macAddress} not found",
                ConsoleColor.Red);
            return;
        }

        captureDevice.OnPacketArrival += CaptureDeviceOnPacketArrival;
        var time = DateTime.Now;
        // prewarm
        _ = SphObjectDb.GameObjectDataDb;
        ObjectPacketTools.RegisterBsonMapperForBit();
        var timeAfterLoad = DateTime.Now;
        ConsoleExtensions.WriteLineColored(
            $"Ready for packets. Load time: {(timeAfterLoad - time).TotalMilliseconds} msec", ConsoleColor.Yellow);
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        try
        {
            captureDevice.Open();
            captureDevice.StartCapture();
            Task.Run(PacketQueueProcessingLoop);
        }
        catch (Exception ex)
        {
            ConsoleExtensions.WriteException(ex);
        }
        finally
        {
            captureDevice.Close();
        }
    }

    private void CaptureDeviceOnPacketArrival (object _, SharpPcap.PacketCapture capture)
    {
        try
        {
            var rawCapture = capture.GetPacket();
            var packet = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);
            var ipPacket = packet.Extract<IPPacket>();
            if (ipPacket is null)
            {
                return;
            }

            if (!sphereLiveServers.Contains(ipPacket.DestinationAddress) &&
                !sphereLiveServers.Contains(ipPacket.SourceAddress))
            {
                return;
            }

            var tcpPacket = packet.Extract<TcpPacket>();

            if (tcpPacket?.PayloadData is null
                || (tcpPacket.DestinationPort != sphereLiveServerPort && tcpPacket.SourcePort != sphereLiveServerPort))
            {
                return;
            }

            var source = tcpPacket.DestinationPort == sphereLiveServerPort ? PacketSource.CLIENT : PacketSource.SERVER;

            if (!tcpPacket.Push)
            {
                // ack
                packetDataQueue.AddRange(tcpPacket.PayloadData);
            }
            else
            {
                // psh + ack
                packetDataQueue.AddRange(tcpPacket.PayloadData);
                var combinedPacket = packetDataQueue.ToArray();
                packetDataQueue.Clear();
                SchedulePacketProcessing(combinedPacket, source, rawCapture.Timeval.Date);
            }
        }
        catch (Exception ex)
        {
            ConsoleExtensions.WriteException(ex);
        }
    }

    private void SchedulePacketProcessing (byte[] data, PacketSource source, DateTime arrivalTime,
        bool shouldDecode = true)
    {
        byte[] decodedData;
        if (shouldDecode && source == PacketSource.CLIENT)
        {
            decodedData = PacketDecoder.DecodeClientPacket(data);
        }
        else
        {
            decodedData = data;
        }

        rawCapturedPackets.Add(new CapturedPacketRawData
        {
            ArrivalTime = arrivalTime,
            Buffer = data,
            DecodedBuffer = decodedData,
            Source = source
        });
    }

    private void PacketQueueProcessingLoop ()
    {
        while (true)
        {
            try
            {
                ProcessPacketQueue();
            }
            catch (Exception ex)
            {
                ConsoleExtensions.WriteException(ex);
            }

            Thread.Sleep(100);
        }
    }

    private void ProcessPacketQueue ()
    {
        var packetsToProcess = new Dictionary<PacketSource, List<CapturedPacketRawData>>
        {
            [PacketSource.CLIENT] = new (),
            [PacketSource.SERVER] = new ()
        };
        var timeLimit = DateTime.UtcNow.AddSeconds(-1);

        foreach (var rawCapturedPacket in rawCapturedPackets)
        {
            if (rawCapturedPacket.WasProcessed || rawCapturedPacket.ArrivalTime > timeLimit)
            {
                continue;
            }

            rawCapturedPacket.WasProcessed = true;
            packetsToProcess[rawCapturedPacket.Source].Add(rawCapturedPacket);
        }

        packetsToProcess[PacketSource.CLIENT].Sort(CapturedPacketRawData.Compare);
        packetsToProcess[PacketSource.SERVER].Sort(CapturedPacketRawData.Compare);
    }
}
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using SharpPcap;

const string pingCaptureFilePath = "C:\\_sphereDumps\\ping";
const string clientCaptureFilePath = "C:\\_sphereDumps\\client";
const string serverCaptureFilePath = "C:\\_sphereDumps\\server";
const string mixedCaptureFilePath = "C:\\_sphereDumps\\mixed";
const string currentWorldCoordsFilePath = "C:\\_sphereDumps\\currentWorldCoords";
WorldCoords oldCoords = new WorldCoords(9999, 9999, 9999, 9999);

const string pingCaptureFilePathLocal = "C:\\_sphereDumps\\local_ping";
const string clientCaptureFilePathLocal = "C:\\_sphereDumps\\local_client";
const string serverCaptureFilePathLocal = "C:\\_sphereDumps\\local_server";
const string mixedCaptureFilePathLocal = "C:\\_sphereDumps\\local_mixed";
WorldCoords oldCoordsLocal = new WorldCoords(9999, 9999, 9999, 9999);

void OnPacketArrival(object sender, PacketCapture c)
{
    try
    {
        var rawCapture = c.GetPacket();
        var packet = PacketDotNet.Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);

        var ipPacket = packet.Extract<PacketDotNet.IPPacket>();

        if (ipPacket == null)
        {
            return;
        }

        var tcpPacket = packet.Extract<PacketDotNet.TcpPacket>();

        var isRemote = ipPacket.DestinationAddress.Equals(IPAddress.Parse("77.223.107.68"))
                      || ipPacket.DestinationAddress.Equals(IPAddress.Parse("77.223.107.69"))
                      || ipPacket.SourceAddress.Equals(IPAddress.Parse("77.223.107.68"))
                      || ipPacket.SourceAddress.Equals(IPAddress.Parse("77.223.107.69"));

        if (!isRemote && !ipPacket.DestinationAddress.Equals(IPAddress.Parse("192.168.0.12")) &&
            !ipPacket.SourceAddress.Equals(IPAddress.Parse("192.168.0.12")))
        {
            return;
        }

        if (tcpPacket?.PayloadData == null || (tcpPacket.DestinationPort != 25860 && tcpPacket.SourcePort != 25860))
        {
            return;
        }

        var data = tcpPacket.PayloadData;

        var len = data.Length;

        if (len is 0 or 4 or 8 or 11 or 12 or 13 or 16 or 17 or 18)
        {
            return;
        }

        var isClient = tcpPacket.DestinationPort == 25860;

        data = isClient && isRemote ? DecodeClientPacket(data) : data;
        var pingPath = !isRemote ? pingCaptureFilePathLocal : pingCaptureFilePath;
        var clientPath = !isRemote ? clientCaptureFilePathLocal : clientCaptureFilePath;
        var serverPath = !isRemote ? serverCaptureFilePathLocal : serverCaptureFilePath;
        var mixedPath = !isRemote ? mixedCaptureFilePathLocal : mixedCaptureFilePath;
        
        // ping
        if (len == 38 && isClient && data[17] != 0)
        {
            var coords = CoordsHelper.GetCoordsFromPingBytes(data);

            if (!coords.Equals(oldCoords))
            {
                File.AppendAllText(pingPath, coords.ToFileDumpString());
                File.WriteAllText(currentWorldCoordsFilePath, coords.x + "\n" + coords.y + "\n" + coords.z + "\n" + coords.turn);
                oldCoords = coords;
            }

            return;
        }

        var dataHex = Convert.ToHexString(data);
        var packetIndices = Regex.Matches(dataHex, @"..0.2C0100", RegexOptions.Compiled);
        var splitPacket = new StringBuilder();
        var splitPacketForMixed = new StringBuilder();
        var previousMatchIndex = 0;

        foreach (Match match in packetIndices)
        {
            if (match.Success)
            {
                var substr = dataHex.Substring(previousMatchIndex, match.Index - previousMatchIndex);

                if (!string.IsNullOrWhiteSpace(substr))
                {
                    var prefix = previousMatchIndex == 0 ? "" : "-------------------\t\t\t";
                    splitPacket.Append($"{prefix}{substr}\n");
                    var prefixForMixed = previousMatchIndex == 0 ? "" : "-------------------------------\t\t\t";
                    splitPacketForMixed.Append($"{prefixForMixed}{substr}\n");
                }

                previousMatchIndex = match.Index;
            }
        }

        var lastSubstr = dataHex.Substring(previousMatchIndex, dataHex.Length - previousMatchIndex);

        if (!string.IsNullOrWhiteSpace(lastSubstr))
        {
            var prefix = previousMatchIndex == 0 ? "" : "-------------------\t\t\t";
            splitPacket.Append($"{prefix}{lastSubstr}\n");
            var prefixForMixed = previousMatchIndex == 0 ? "" : "-------------------------------\t\t\t";
            splitPacketForMixed.Append($"{prefixForMixed}{lastSubstr}\n");
        }

        var splitPacketResult = splitPacket.ToString();
        var splitPacketForMixedResult = splitPacketForMixed.ToString();
        
        if (isClient)
        {
            var binary = ByteArrayToBinaryString(data);
            File.AppendAllText(clientPath, $"{DateTime.Now}\t\t\t{splitPacketResult}");
            File.AppendAllText(mixedPath, $"CLI\t\t\t{DateTime.Now}\t\t\t{splitPacketForMixedResult}");

            if (data[0] == 0x1A)
            {
                // item move
                File.AppendAllText("C:\\_sphereDumps\\itemMove", $"CLI\t\t\t{DateTime.Now}\t\t\t{splitPacketResult}");
            }
        }

        // 0x17 is mob positions?
        else if (data[0] != 0x17)
        {
            var binary = ByteArrayToBinaryString(data);
            File.AppendAllText(serverPath, $"{DateTime.Now}\t\t\t{splitPacketResult}");
            File.AppendAllText(mixedPath, $"SRV\t\t\t{DateTime.Now}\t\t\t{splitPacketForMixedResult}");

            if (data[0] == 0x2E)
            {
                // item move
                File.AppendAllText("C:\\_sphereDumps\\itemMove", $"SRV\t\t\t{DateTime.Now}\t\t\t{splitPacketResult}\n");
            }
        }
    }
    catch (Exception ex) {
        Console.WriteLine(ex);
    }
}

static byte[] DecodeClientPacket(byte [] input)
{
    var encoded = input[9..];
    var result = new byte[encoded.Length + 9];
    byte mask3 = 0x0;
    var encodingMask = new byte[] { 0x4B, 0x0D, 0xEF, 0x60, 0xC9, 0x9A, 0x70, 0x0E, 0x03 };

    for (var i = 0; i < encoded.Length; i++)
    {
        var curr = (byte)(encoded[i] ^ encodingMask[i % 9] ^ mask3);
        result[i + 9] = curr;
        mask3 = (byte)(curr * i + 2 * mask3);
    }
    
    Array.Copy(input, result, 9);

    return result;
}

static string ByteArrayToBinaryString(byte[] ba, bool noPadding = false, bool addSpaces = false)
{
    var hex = new StringBuilder(ba.Length * 2);
    
    foreach (var val in ba)
    {
        var str = Convert.ToString(val, 2);
        if (!noPadding) str = str.PadLeft(8, '0');
        hex.Append(str);

        if (addSpaces)
        {
            hex.Append(' ');
        }
    }
    
    return hex.ToString();
}

var devices = CaptureDeviceList.Instance;
var ethernet = devices.FirstOrDefault(x => x.MacAddress?.ToString() == "00D861BEC926");
ethernet.OnPacketArrival += OnPacketArrival;

ethernet.Open();
ethernet.Capture();
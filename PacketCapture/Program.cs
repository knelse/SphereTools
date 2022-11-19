using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using SharpPcap;
using static BitStreamTools;

const string pingCaptureFilePath = "C:\\_sphereDumps\\ping";
const string clientCaptureFilePath = "C:\\_sphereDumps\\client";
const string serverCaptureFilePath = "C:\\_sphereDumps\\server";
const string serverCaptureFilePathUnfiltered = "C:\\_sphereDumps\\server_unfiltered";
const string mixedCaptureFilePath = "C:\\_sphereDumps\\mixed";
const string currentWorldCoordsFilePath = "C:\\_sphereDumps\\currentWorldCoords";
const string itemPacketDecodeFilePath = "C:\\_sphereDumps\\itemPackets";
WorldCoords oldCoords = new WorldCoords(9999, 9999, 9999, 9999);

const string pingCaptureFilePathLocal = "C:\\_sphereDumps\\local_ping";
const string clientCaptureFilePathLocal = "C:\\_sphereDumps\\local_client";
const string serverCaptureFilePathLocal = "C:\\_sphereDumps\\local_server";
const string mixedCaptureFilePathLocal = "C:\\_sphereDumps\\local_mixed";
WorldCoords oldCoordsLocal = new WorldCoords(9999, 9999, 9999, 9999);

byte[] packetRemainder = {};

// 1200 - ping
// 1300 - 6s ping
// 1000 - 15s ping
// 1400 - echo
// 0800 - client smth
// 0C00 - client smth 2
// 1700 - object position
// 2200 - object position
// 2D00 - object position
// 0F00 - idk
// 1F00 - damage to client
var serverPacketsToHide = new HashSet<byte>
    { 0x12, 0x13, 0x10, 0x14, 0x17, 0x22, 0x0F, 0x2D, 0x11, 0x1D, 0x19, 0x0B, 0x43, 0x38, 0x1F };//, "08", "0C"};// {"2D", "0E", "1D", "12"};
var sessionDivider =
    "================================================================================================================================================================================================================\n" +
    "================================================================================================================================================================================================================\n";

var endPacket = new byte[] { 0x04, 0x00, 0xF4, 0x01 };
var packetOkMarker = new byte[] { 0x2C, 0x01 };
var newSessionFirstPacket = new byte[] { 0x0A, 0x00, 0xC8, 0x00 };

bool ShouldHidePacket(byte[] packet)
{
    if (ByteArrayCompare(packet, endPacket))
    {
        return true;
    }

    return packet.Length is 8 or 12 || 
           serverPacketsToHide.Contains(packet[0]) && ByteArrayCompare(packet, packetOkMarker, 2);
}

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

        if (len is 0 or 4)// or 8 or 11 or 12 or 13 or 16 or 17 or 18)
        {
            return;
        }

        var isClient = tcpPacket.DestinationPort == 25860;

        data = isClient && isRemote ? DecodeClientPacket(data) : data;
        var pingPath = !isRemote ? pingCaptureFilePathLocal : pingCaptureFilePath;
        var clientPath = !isRemote ? clientCaptureFilePathLocal : clientCaptureFilePath;
        var serverPath = !isRemote ? serverCaptureFilePathLocal : serverCaptureFilePath;
        var mixedPath = !isRemote ? mixedCaptureFilePathLocal : mixedCaptureFilePath;

        if (packetRemainder.Length > 0 && !isClient)
        {
            var dataConcatList = new List<byte>(packetRemainder);
            dataConcatList.AddRange(data);
            data = dataConcatList.ToArray();
            packetRemainder = Array.Empty<byte>();
        }
        
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

        var bytePacketSplit = new List<byte[]>();

        try
        {
            if (!isClient)
            {
                for (var i = 0; i < data.Length - 4; i++)
                {
                    if (data[i + 2] == 0x2C && data[i + 3] == 0x01 && data[i + 4] == 0x00)
                    {
                        var length = data[i + 1] * 16 + data[i];

                        if (i + length - 1 >= data.Length)
                        {
                            // packet got split in the middle
                            packetRemainder = data[i..];
                        }
                        else
                        {
                            var split = data[i..(i + length - 1)];
                            bytePacketSplit.Add(split);
                            i += length - 1;
                        }
                    }
                }
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            Console.WriteLine("Broken: " + Convert.ToHexString(data));
        }

        for (int i = 0; i < bytePacketSplit.Count; i++)
        {
            var prefix = i == 0 ? "" : "-------------------\t\t\t";
            var prefixForMixed = i == 0 ? "" : "-------------------------------\t\t\t";
            var currentPacket = bytePacketSplit[i];
            var newSessionDivider = ByteArrayCompare(currentPacket, newSessionFirstPacket) ? sessionDivider : "";
            var currentPacketPaddedHex = prefix + Convert.ToHexString(currentPacket) + "\n";
            var currentPacketPaddedHexForMixed = prefixForMixed + Convert.ToHexString(currentPacket) + "\n";

            if (isClient)
            {
                var actionSource = "";
                var actionDestination = "";

                if (data[0] == 0x20)
                {
                    //damage
                    actionDestination = (Convert.ToString(GetDestinationIdFromDamagePacket(data), 16)).PadLeft(4, '0') +
                                        "\t\t\t";
                }

                File.AppendAllText(clientPath,
                    $"{newSessionDivider}{DateTime.Now}\t\t\t{actionSource}{actionDestination}{currentPacketPaddedHex}");
                File.AppendAllText(mixedPath,
                    $"{newSessionDivider}CLI\t\t\t{DateTime.Now}\t\t\t{actionSource}{actionDestination}{currentPacketPaddedHexForMixed}");

                if (data[0] == 0x1A)
                {
                    // item move
                    File.AppendAllText("C:\\_sphereDumps\\itemMove",
                        $"CLI\t\t\t{DateTime.Now}\t\t\t{currentPacketPaddedHex}");
                }
            }

            else
            {
                if (!ShouldHidePacket(currentPacket))
                {
                    if (data[0] != 0x17)
                    {
                        File.AppendAllText(serverPath,
                            $"{newSessionDivider}{DateTime.Now}\t\t\t{currentPacketPaddedHex}");
                        File.AppendAllText(mixedPath,
                            $"{newSessionDivider}SRV\t\t\t{DateTime.Now}\t\t\t{currentPacketPaddedHexForMixed}");

                        if (data[0] == 0x2E)
                        {
                            // item move
                            File.AppendAllText("C:\\_sphereDumps\\itemMove",
                                $"SRV\t\t\t{DateTime.Now}\t\t\t{currentPacketPaddedHex}\n");
                        }

                        if (data.Length > 25 && data[25] == 0x91 && data[26] == 0x45)
                        {
                            // item packet?
                            File.AppendAllText(itemPacketDecodeFilePath,
                                $"\nSRV\t\t\t{DateTime.Now}\t\t\t{currentPacketPaddedHex}\n");

                            foreach (var splitPacketBytes in bytePacketSplit)
                            {
                                var itemList = GetItemsFromPacket(splitPacketBytes);
                                File.AppendAllText(itemPacketDecodeFilePath,
                                    $"{Convert.ToHexString(splitPacketBytes)}\n");

                                foreach (var item in itemList)
                                {
                                    File.AppendAllText(itemPacketDecodeFilePath, $"{item.ToDebugString()}\n");
                                }
                            }
                        }
                    }
                    else
                    {
                        // mob move
                        var x = (data[5] & 0b1111111) + (data[6] << 7) + ((data[7] % 2) << 15) - 32768;
                        var y = ((data[7] & 0b11111110) >> 1) + ((data[8] & 0b111111) << 7) - 1200;
                        var z = ((data[8] & 0b11000000) >> 6) + (data[9] << 2) + ((data[10] & 0b111111) << 10) - 32768;
                        var id = ((data[12] & 0b11100000) >> 5) + (data[13] << 3) + ((data[14] & 0b11111) << 11);
                        var xDec = 4096 - (((data[17] & 0b11111100) >> 2) + ((data[18] & 0b11111) << 6));
                        var yDec = 2048 - (((data[18] & 0b11000000) >> 6) + (data[19] << 2) + ((data[20] & 0b1) << 10));
                        var zDec = 4096 - (((data[20] & 0b11111100) >> 2) + ((data[21] & 0b11111) << 6));
                        var t = ((data[21] & 0b11000000) >> 6) + ((data[22] & 0b11111) << 2);

                        xDec *= (data[18] & 0b100000) > 0 ? -1 : 1;
                        yDec *= (data[20] & 0b10) > 0 ? -1 : 1;
                        zDec *= (data[21] & 0b100000) > 0 ? -1 : 1;
                        t *= (data[22] & 0b100000) > 0 ? -1 : 1;

                        var xDecFloat = (float) xDec / 2048;
                        var yDecFloat = (float) yDec / 2048;
                        var zDecFloat = (float) zDec / 2048;

                        var idStr = Convert.ToString(id, 16).PadLeft(4, '0');

                        File.AppendAllText(serverPath,
                            $"{newSessionDivider}{DateTime.Now}\t\t\t{idStr}\t{x + xDecFloat:F6}\t{y + yDecFloat:F6}\t{z + zDecFloat:F6}\t{t}\t\t\t{currentPacketPaddedHex}");
                        File.AppendAllText(mixedPath,
                            $"{newSessionDivider}SRV\t\t\t{DateTime.Now}\t\t\t{idStr}\t{x + xDecFloat:F6}\t{y + yDecFloat:F6}\t{z + zDecFloat:F6}\t{t}\t\t\t{currentPacketPaddedHex}");
                    }
                }
                else
                {
                    File.AppendAllText(serverCaptureFilePathUnfiltered,
                        $"{newSessionDivider}{DateTime.Now}\t\t\t{currentPacketPaddedHex}");
                }
            }
        }
    }
    catch (Exception ex) {
        Console.WriteLine(ex);
    }
}

static byte[] DecodeClientPacket(byte [] input)
{
    if (input.Length <= 9)
    {
        return input;
    }
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

static int GetDestinationIdFromDamagePacket(byte[] rcvBuffer)
{
    var destBytes = rcvBuffer[28..];

    return ((destBytes[2] & 0b11111) << 11) + ((destBytes[1]) << 3) + ((destBytes[0] & 0b11100000) >> 5);
}

var devices = CaptureDeviceList.Instance;
var ethernet = devices.FirstOrDefault(x => x.MacAddress?.ToString() == "00D861BEC926");
ethernet.OnPacketArrival += OnPacketArrival;

ethernet.Open();
ethernet.Capture();
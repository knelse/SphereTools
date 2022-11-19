using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using SharpPcap;

const string pingCaptureFilePath = "C:\\_sphereDumps\\ping";
const string clientCaptureFilePath = "C:\\_sphereDumps\\client";
const string serverCaptureFilePath = "C:\\_sphereDumps\\server";
const string mixedCaptureFilePath = "C:\\_sphereDumps\\mixed";
const string currentWorldCoordsFilePath = "C:\\_sphereDumps\\currentWorldCoords";
const string itemPacketDecodeFilePath = "C:\\_sphereDumps\\itemPackets";
WorldCoords oldCoords = new WorldCoords(9999, 9999, 9999, 9999);

const string pingCaptureFilePathLocal = "C:\\_sphereDumps\\local_ping";
const string clientCaptureFilePathLocal = "C:\\_sphereDumps\\local_client";
const string serverCaptureFilePathLocal = "C:\\_sphereDumps\\local_server";
const string mixedCaptureFilePathLocal = "C:\\_sphereDumps\\local_mixed";
WorldCoords oldCoordsLocal = new WorldCoords(9999, 9999, 9999, 9999);

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
var serverPacketsToHide = new string[] { "12", "13", "10", "14", "17", "22", "0F", "2D", "11", "1D", "19", "0B", "43", "38", "1F"};//, "08", "0C"};// {"2D", "0E", "1D", "12"};
// 4601 earth end
// 4701 water air
// 4801 shiny arbalet
// 0C80 formula?
// 1660 medit ring
// 1460 dur ring
// 1460 adventure belt 1
var sessionDivider =
    "================================================================================================================================================================================================================\n" +
    "================================================================================================================================================================================================================\n";

bool ShouldHidePacket(string str)
{
    if (str.Equals("0400F401", StringComparison.InvariantCultureIgnoreCase)) return true;
    foreach (var hidden in serverPacketsToHide!)
    {
        if (str.StartsWith(hidden + "002C01") || str.Length is 16 or 24)
        {
            return true;
        }
    }

    return false;
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

        var pattern = isClient ? "..00........2C0100" : @"..0.2C0100";
        var dataHex = Convert.ToHexString(data);
        var packetIndices = Regex.Matches(dataHex, pattern, RegexOptions.Compiled);
        var splitPacket = new StringBuilder();
        var splitPacketForMixed = new StringBuilder();
        var previousMatchIndex = 0;

        var bytePacketSplit = new List<byte[]>();

        for (var i = 0; i < data.Length - 4; i++)
        {
            if (data[i + 2] == 0x2C && data[i + 3] == 0x01 && data[i + 4] == 0x00)
            {
                var length = data[i+1] * 16 + data[i];
                var split = data[i..(i + length - 1)];
                bytePacketSplit.Add(split);
                i += length - 1;
            }
        }

        foreach (Match match in packetIndices)
        {
            if (match.Success)
            {
                var substr = dataHex.Substring(previousMatchIndex, match.Index - previousMatchIndex);

                if (!string.IsNullOrWhiteSpace(substr) && !ShouldHidePacket(substr))
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

        if (!string.IsNullOrWhiteSpace(lastSubstr) && !ShouldHidePacket(lastSubstr))
        {
            var prefix = previousMatchIndex == 0 ? "" : "-------------------\t\t\t";
            splitPacket.Append($"{prefix}{lastSubstr}\n");
            var prefixForMixed = previousMatchIndex == 0 ? "" : "-------------------------------\t\t\t";
            splitPacketForMixed.Append($"{prefixForMixed}{lastSubstr}\n");
        }

        if (splitPacket.Length == 0)
        {
            return;
        }

        var splitPacketResult = splitPacket.ToString();
        var splitPacketForMixedResult = splitPacketForMixed.ToString();
        var newSessionDivider = splitPacketResult.StartsWith("0A00C800") ? sessionDivider : "";
        
        if (isClient)
        {
            var binary = ByteArrayToBinaryString(data);

            var actionSource = "";
            var actionDestination = "";

            if (data[0] == 0x20)
            {
                //damage
                actionDestination = (Convert.ToString(GetDestinationIdFromDamagePacket(data), 16)).PadLeft(4, '0') + "\t\t\t";
            }
            
            File.AppendAllText(clientPath, $"{newSessionDivider}{DateTime.Now}\t\t\t{actionSource}{actionDestination}{splitPacketResult}");
            File.AppendAllText(mixedPath, $"{newSessionDivider}CLI\t\t\t{DateTime.Now}\t\t\t{actionSource}{actionDestination}{splitPacketForMixedResult}");

            if (data[0] == 0x1A)
            {
                // item move
                File.AppendAllText("C:\\_sphereDumps\\itemMove", $"CLI\t\t\t{DateTime.Now}\t\t\t{splitPacketResult}");
            }
        }

        else
        {
            if (data[0] != 0x17)
            {
                var binary = ByteArrayToBinaryString(data);
                File.AppendAllText(serverPath, $"{newSessionDivider}{DateTime.Now}\t\t\t{splitPacketResult}");
                File.AppendAllText(mixedPath,
                    $"{newSessionDivider}SRV\t\t\t{DateTime.Now}\t\t\t{splitPacketForMixedResult}");

                if (data[0] == 0x2E)
                {
                    // item move
                    File.AppendAllText("C:\\_sphereDumps\\itemMove",
                        $"SRV\t\t\t{DateTime.Now}\t\t\t{splitPacketResult}\n");
                }

                if (data.Length > 25 && data[25] == 0x91 && data[26] == 0x45)
                {
                    // item packet?
                    File.AppendAllText(itemPacketDecodeFilePath, $"\nSRV\t\t\t{DateTime.Now}\t\t\t{splitPacketResult}\n");

                    foreach (var splitPacketBytes in bytePacketSplit)
                    {
                        var itemList = BitStreamTools.GetItemsFromPacket(splitPacketBytes);
                        File.AppendAllText(itemPacketDecodeFilePath, $"{Convert.ToHexString(splitPacketBytes)}\n");

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

                var xDecFloat = (float)xDec / 2048;
                var yDecFloat = (float)yDec / 2048;
                var zDecFloat = (float)zDec / 2048;

                var idStr = Convert.ToString(id, 16).PadLeft(4, '0');
                
                File.AppendAllText(serverPath, $"{newSessionDivider}{DateTime.Now}\t\t\t{idStr}\t{x + xDecFloat:F6}\t{y + yDecFloat:F6}\t{z + zDecFloat:F6}\t{t}\t\t\t{splitPacketResult}");
                File.AppendAllText(mixedPath,
                    $"{newSessionDivider}SRV\t\t\t{DateTime.Now}\t\t\t{idStr}\t{x + xDecFloat:F6}\t{y + yDecFloat:F6}\t{z + zDecFloat:F6}\t{t}\t\t\t{splitPacketResult}");
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
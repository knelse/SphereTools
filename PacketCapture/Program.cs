using System.Net;
using System.Text;
using LiteDB;
using SharpPcap;
using static ObjectPacketTools;


var pingCaptureFile = "C:\\_sphereDumps\\ping";
var clientCaptureFile = "C:\\_sphereDumps\\client";
var serverCaptureFile = "C:\\_sphereDumps\\server";
var serverCaptureFileUnfiltered = "C:\\_sphereDumps\\server_unfiltered";
var mixedCaptureFile = "C:\\_sphereDumps\\mixed";
var currentWorldCoordsFile = "C:\\_sphereDumps\\currentWorldCoords";
var objectPacketDecodeFile = "C:\\_sphereDumps\\objectPackets";
var itemMove = "C:\\_sphereDumps\\itemMove";
var oldCoords = new WorldCoords(9999, 9999, 9999, 9999);

var pingCaptureFileLocal = "C:\\_sphereDumps\\local_ping";
var clientCaptureFileLocal = "C:\\_sphereDumps\\local_client";
var serverCaptureFileLocal = "C:\\_sphereDumps\\local_server";
var mixedCaptureFileLocal = "C:\\_sphereDumps\\local_mixed";
var oldCoordsLocal = new WorldCoords(9999, 9999, 9999, 9999);
using var itemDb = new LiteDatabase(@"Filename=C:\_sphereStuff\sph_items.db;Connection=shared;");
var itemCollection = itemDb.GetCollection<ObjectPacket>("ObjectPackets");

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
var clientPacketsToHide = new HashSet<byte>
{0x08, 0x0C, };
const string sessionDivider = "================================================================================================================================================================================================================\n" +
                              "================================================================================================================================================================================================================\n";

var endPacket = new byte[] { 0x04, 0x00, 0xF4, 0x01 };
var packetOkMarker = new byte[] { 0x2C, 0x01 };
var newSessionFirstPacket = new byte[] { 0x0A, 0x00, 0xC8, 0x00 };

var lastSplitPacket = new List<byte>();

void WriteWithRetry(string filepath, string content)
{
    Task.Run(() =>
    {
        var retryCount = 0;
        while (retryCount < 10)
        {
            try
            {
                File.AppendAllText(filepath, content);
                break;
            }
            catch (IOException ex)
            {
                retryCount++;
                if (retryCount >= 10)
                {
                    Console.WriteLine(ex.ToString());
                }
                Thread.Sleep(50);
            }
        }
    });
}

bool ShouldHidePacket(byte[] packet, bool isClient = false)
{
    if (ByteArrayCompare(packet, endPacket))
    {
        return true;
    }

    return isClient 
        ? clientPacketsToHide.Contains(packet[0])
        : packet.Length is 8 or 12 || 
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
        
        ProcessPacket(tcpPacket.PayloadData, tcpPacket.DestinationPort == 25860, isRemote);
    }
    catch (Exception ex) {
        Console.WriteLine(ex);
    }
}

void ProcessPacket(byte[] data, bool isClient, bool isRemote)
{
        var len = data.Length;

        if (len is 0 or 4)// or 8 or 11 or 12 or 13 or 16 or 17 or 18)
        {
            return;
        }

        data = isClient && isRemote ? DecodeClientPacket(data) : data;
        var ping = !isRemote ? pingCaptureFileLocal : pingCaptureFile;
        var client = !isRemote ? clientCaptureFileLocal : clientCaptureFile;
        var server = !isRemote ? serverCaptureFileLocal : serverCaptureFile;
        var mixed = !isRemote ? mixedCaptureFileLocal : mixedCaptureFile;

        if (lastSplitPacket.Count > 0 && len >=4 && !isClient)
        {
            //continuation of a split packet
            lastSplitPacket.AddRange(data);
            data = lastSplitPacket.ToArray();
            // Console.WriteLine($"Concat data: {Convert.ToHexString(data)}");
            lastSplitPacket.Clear();
        }
        
        // ping
        if (len == 38 && isClient && data[17] != 0)
        {
            var coords = CoordsHelper.GetCoordsFromPingBytes(data);

            if (!coords.Equals(oldCoords))
            {
                WriteWithRetry(ping, coords.ToFileDumpString());
                WriteWithRetry(currentWorldCoordsFile, coords.x + "\n" + coords.y + "\n" + coords.z + "\n" + coords.turn);
                oldCoords = coords;
            }

            return;
        }

        var bytePacketSplit = new List<byte[]>();
        var bytePacketForAnalysis = new List<byte>();

        try
        {
            if (!isClient)
            {
                for (var i = 0; i < data.Length - 4; i++)
                {
                    if (data[i + 2] == 0x2C && data[i + 3] == 0x01 && data[i + 4] == 0x00)
                    {
                        var length = data[i + 1] * 16 + data[i];

                        if (i + length - 1 > data.Length)
                        {
                            // packet got split in the middle
                            lastSplitPacket.AddRange(data);
                            bytePacketSplit.Clear();
                            bytePacketForAnalysis.Clear();
                        }
                        else
                        {
                            var end = i + length > data.Length ? data.Length : i + length;
                            var split =  data[i..end];

                            bytePacketSplit.Add(split);
                            // size_1 size_2 00 2C 01 sync_1 sync_2 id...
                            var forAnalysis = data[(i + 7)..end];
                            bytePacketForAnalysis.AddRange(forAnalysis);
                            i += length - 4;
                        }
                    }
                }
            }
            else
            {
                bytePacketSplit.Add(data);
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            Console.WriteLine("Broken: " + Convert.ToHexString(data));
        }

        for (var i = 0; i < bytePacketSplit.Count; i++)
        {
            var prefix = i == 0 ? "" : "-------------------\t\t\t";
            var prefixForMixed = i == 0 ? "" : "-------------------------------\t\t\t";
            var currentPacket = bytePacketSplit[i];
            var newSessionDivider = ByteArrayCompare(currentPacket, newSessionFirstPacket) ? sessionDivider : "";
            var currentPacketPaddedHex = prefix + Convert.ToHexString(currentPacket) + "\n";
            var currentPacketPaddedHexForMixed = prefixForMixed + Convert.ToHexString(currentPacket) + "\n";

            if (isClient && !ShouldHidePacket(currentPacket))
            {
                var actionSource = "";
                var actionDestination = "";

                if (data[0] == 0x20)
                {
                    //damage
                    actionDestination = (Convert.ToString(GetDestinationIdFromDamagePacket(data), 16)).PadLeft(4, '0') +
                                        "\t\t\t";
                }

                WriteWithRetry(client,
                    $"{newSessionDivider}{DateTime.Now}\t\t\t{actionSource}{actionDestination}{currentPacketPaddedHex}");
                WriteWithRetry(mixed,
                    $"{newSessionDivider}CLI\t\t\t{DateTime.Now}\t\t\t{actionSource}{actionDestination}{currentPacketPaddedHexForMixed}");

                if (data[0] == 0x1A)
                {
                    // item move
                    WriteWithRetry(itemMove, $"CLI\t\t\t{DateTime.Now}\t\t\t{currentPacketPaddedHex}");
                }
            }

            else
            {
                if (!ShouldHidePacket(currentPacket))
                {
                    if (data[0] != 0x17)
                    {
                        WriteWithRetry(server,
                            $"{newSessionDivider}{DateTime.Now}\t\t\t{currentPacketPaddedHex}");
                        WriteWithRetry(mixed, $"{newSessionDivider}SRV\t\t\t{DateTime.Now}\t\t\t{currentPacketPaddedHexForMixed}");

                        if (data[0] == 0x2E)
                        {
                            // item move
                            WriteWithRetry(itemMove,
                                $"SRV\t\t\t{DateTime.Now}\t\t\t{currentPacketPaddedHex}");
                        }

                        if (data.Length > 25 && data[25] == 0x91 && data[26] == 0x45)
                        {
                            // object packet?
                            WriteWithRetry(objectPacketDecodeFile,
                                $"SRV\t\t\t{DateTime.Now}\t\t\t{currentPacketPaddedHex}");
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

                        WriteWithRetry(server,
                            $"{newSessionDivider}{DateTime.Now}\t\t\t{idStr}\t{x + xDecFloat:F6}\t{y + yDecFloat:F6}\t{z + zDecFloat:F6}\t{t}\t\t\t{currentPacketPaddedHex}");
                        WriteWithRetry(mixed,
                            $"{newSessionDivider}SRV\t\t\t{DateTime.Now}\t\t\t{idStr}\t{x + xDecFloat:F6}\t{y + yDecFloat:F6}\t{z + zDecFloat:F6}\t{t}\t\t\t{currentPacketPaddedHex}");
                    }
                }
                else
                {
                    WriteWithRetry(serverCaptureFileUnfiltered,
                        $"{newSessionDivider}{DateTime.Now}\t\t\t{currentPacketPaddedHex}");
                }
            }
        }

        var bytesForAnalysis = bytePacketForAnalysis.ToArray();
        // analysis
        if (!isClient && bytesForAnalysis.Length > 25)
        {
            var objectList = GetObjectsFromPacket(bytesForAnalysis);

            if (objectList.Count > 0)
            {
                objectList.ForEach(x => itemCollection.Insert(x));
                WriteWithRetry(objectPacketDecodeFile, $"{Convert.ToHexString(bytesForAnalysis)}\n" +
                                                       $"{GetTextOutput(objectList, true)}\n" +
                                                       $"({objectList.Count} total)\n" +
                                                       "----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n");
            }
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
var ethernet = devices.FirstOrDefault(x => x.MacAddress?.ToString() == "C87F54061FF1");
RegisterBsonMapperForBit();
ethernet.OnPacketArrival += OnPacketArrival;
// to let it load before first object packet
var time = DateTime.Now;
var _ = SphObjectDb.GameObjectDataDb;
var timeAfterLoad = DateTime.Now;
Console.WriteLine($"Ready for packets. Load time: {(timeAfterLoad - time).TotalMilliseconds} msec");

// var test = Convert.FromHexString(
//     "C8002C0100BC6A9A7EE48B0FF8B52F09402DFF18009AC018409145A69CA44B0106CAFA000A5900F0FFFFFF5FF08080DFA61FB8E303142E11DAE01E3AE69060339E576491F9B040051828EB03286401C0FFFFFFFF8FD38FCAF001A1C9250132A322038096160328B2C800508081B23E80C20404145E618CEE6B2E6CEC6B8E06E0D7E947BEF8800CF4924058AC9301C85E8E811553642A48BA1460A0AC0FA0900500FFFFFFFF050F08F879FA51193E60E3BA244078516400A0B36200451619000A3050D607509880C6002C0100BC6A9E7E5446E115C6E8BEE6C2C6BEE660007E9F7ED88B0FB8192F09E008071900B09D18409145A610031760A0AC0FA0900500FFFFFFFF3F503F2AC3071C48970478538C0C00484D04A0C82203400106CAFA000A131050788531BAAFB9B0B1AFB91A805FA81F9CE1039EBE4B0258614106000E2AC651649101A00003657D00850908F889FAE12E3E9456FBE391CAE864C0BEF26300851599DE8B30051828EB03286401C0FFFFFFFF6FD40F7AF101E0CB250108C3220300BC160128A8C8AC8B84C6002C0100BC6AA37ED04B0106CAFA000A5900F0FFFFFFFF23F583337C40837A49000F07C800E0F6C5008A2C32001460A0AC0FA0300101BF523F78C7074C409704C047740C003B630CA0C822F35D010A3050D60750C80280FFFFFFFF9FA91FF3E2036EC64B0238C24106006C2706506491F97B45051828EB03286401C0FFFFFFFFEFD40FDDF1017FF0250104812003003F140128B2C8F457B0020C94F50114B200E0FFFFFFFF87EA4765F800F3F1920008E29001801F8B011459640028C040591F40610202CC002C0100BC6AA87E5446E115C6E8BEE6C2C6BEE666007EA97EDC8B0F98802E09808FE8180076C6184091456674151560A0AC0FA0900500FFFFFFFFBF553FDEC507BC3E97042CAC910C80226D0CA0C822B3F591A20003657D00852C00F8FFFFFFFFB1FA512C3EE0F6BD2400A3FF6300180763004516999640548081B23E80421600FCFFFFFF1758C000E0D7EA87BBF88071DF9280C5869201109E8D01145664962FC21460A0AC0FA0900500FFFFFFFF050F08F8B9FA912F3E8025BD240020276400A0F362004516993292BD002C0100BC6AAE7EE44B0106CAFA000A5900F0FFFFFFFF7BF5434B7CC0B3744900E6AAC700702DC6008A2C32A187A80003657D00852C00F8FFFFFF2FB00002C00FD68FEBF00152D22501701E230300BC120128B2C81C15A2020C94F50114B200E0FFFFFFBFC0020D6EBF583F8AC30700A09704D003890C00D85E3CA0C8227344880A3050D60750C80280FFFFFFFF020BDC07FC64FD280E1F20385E1280C320320000000080228B4C08212AC040591F40210B00FEFFFFFF0B2C403BAF002C0100BC6AB37E14876FCB58AB58832AE4986A4BBD98409145A68B101560A0AC0FA0900500FFFFFFFF05162800F8D1FA511C3EA0A4B92400AA90630030F562704516997C58548081B23E80421600FCFFFFFF1758405EE057EB4771F8B0565D8BB9C8128F35E50A8D09145964D208510106CAFA000A5900F0FFFFFF5F600105809FAD1FC5E103DEC44B023C42440600212F06506491492344051828EB03286401C0FFFFFF7F8105F4010A1E10");
//
// ProcessPacket(test, false, true);

ethernet.Open();
ethernet.Capture();
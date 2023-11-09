using System.Net;
using System.Text;
using LiteDB;
using PacketDotNet;
using SharpPcap;
using static ObjectPacketTools;

var pingCaptureFile = "C:\\_sphereDumps\\ping";
var clientCaptureFile = "C:\\_sphereDumps\\client";
var serverCaptureFile = "C:\\_sphereDumps\\server";
var serverCaptureFileUnfiltered = "C:\\_sphereDumps\\server_unfiltered";
var mixedCaptureFile = "C:\\_sphereDumps\\mixed";
var currentWorldCoordsFile = "C:\\_sphereDumps\\currentWorldCoords";
var objectPacketDecodeFile = "C:\\_sphereDumps\\objectPackets";
var chatFile = "C:\\_sphereDumps\\chat";
var itemMove = "C:\\_sphereDumps\\itemMove";
var oldCoords = new WorldCoords(9999, 9999, 9999, 9999);
var pingCaptureFileLocal = "C:\\_sphereDumps\\local_ping";
var clientCaptureFileLocal = "C:\\_sphereDumps\\local_client";
var serverCaptureFileLocal = "C:\\_sphereDumps\\local_server";
var mixedCaptureFileLocal = "C:\\_sphereDumps\\local_mixed";
var oldCoordsLocal = new WorldCoords(9999, 9999, 9999, 9999);
using var itemDb = new LiteDatabase(@"Filename=C:\_sphereStuff\sph_items.db;Connection=shared;");
var itemCollection = itemDb.GetCollection<ObjectPacket>("ObjectPackets");

var serverPacketsToProcess = new List<PacketBufferEntry>();
var clientPacketsToProcess = new List<PacketBufferEntry>();

var packetQueue = new List<byte>();

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
{
    0x12,
    0x13,
    0x10,
    0x14,
    0x17,
    0x22,
    0x0F,
    0x2D,
    0x11,
    0x1D,
    0x19,
    0x0B,
    0x43,
    0x38,
    0x1F
}; //, "08", "0C"};// {"2D", "0E", "1D", "12"};
var clientPacketsToHide = new HashSet<byte> { 0x08, 0x0C };
const string sessionDivider =
    "================================================================================================================================================================================================================\n" +
    "================================================================================================================================================================================================================\n";
var endPacket = new byte[] { 0x04, 0x00, 0xF4, 0x01 };
var packetOkMarker = new byte[] { 0x2C, 0x01 };
var newSessionFirstPacket = new byte[] { 0x0A, 0x00, 0xC8, 0x00 };

void WriteWithRetry (string filepath, string content)
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
                if (retryCount >= 30)
                {
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("---");
                    Console.WriteLine(content);
                }

                Thread.Sleep(50);
            }
        }
    });
}

bool ShouldHidePacket (byte[] packet, bool isClient = false)
{
    if (ByteArrayCompare(packet, endPacket))
    {
        return true;
    }

    if (packet[0] == 0x08 && packet[6] == 0xF4 && packet[7] == 0x01)
    {
        return true;
    }

    if (packet[0] == 0x0C && packet[10] == 0x0D && packet[11] == 0xE2)
    {
        return true;
    }

    if (packet[0] == 0x12 && packet[14] == 0x1B && packet[15] == 0x01 && packet[16] == 0x60)
    {
        return true;
    }

    if (packet[0] == 0x10 && packet[14] == 0x52 && packet[15] == 0x09)
    {
        return true;
    }

    if (packet[0] == 0x17)
    {
        return true;
    }

    if (packet[0] == 0x1D)
    {
        return true;
    }

    if (packet[0] == 0x11 && packet[9] == 0x08 && packet[10] == 0x40 && packet[11] == 0x63)
    {
        return true;
    }

    if (packet[0] == 0x0F && packet[12] == 0x84 && packet[13] == 0x20)
    {
        return true;
    }

    if (packet[0] == 0x2D || packet[0] == 0x22 || packet[0] == 0x12)
    {
        return true;
    }

    return false; // packet.Length is 8 or 12 or 15 or 16 or 18;
}

void OnPacketArrival (object sender, PacketCapture c)
{
    try
    {
        var rawCapture = c.GetPacket();
        var packet = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);
        var ipPacket = packet.Extract<IPPacket>();
        if (ipPacket == null)
        {
            return;
        }

        var tcpPacket = packet.Extract<TcpPacket>();
        var isRemote = ipPacket.DestinationAddress.Equals(IPAddress.Parse("77.223.107.68")) ||
                       ipPacket.DestinationAddress.Equals(IPAddress.Parse("77.223.107.69")) ||
                       ipPacket.SourceAddress.Equals(IPAddress.Parse("77.223.107.68")) ||
                       ipPacket.SourceAddress.Equals(IPAddress.Parse("77.223.107.69"));
        if (!isRemote && !ipPacket.DestinationAddress.Equals(IPAddress.Parse("192.168.1.65")) &&
            !ipPacket.SourceAddress.Equals(IPAddress.Parse("192.168.1.65")))
        {
            return;
        }

        if (tcpPacket?.PayloadData == null
            || (tcpPacket.DestinationPort != 25860 && tcpPacket.SourcePort != 25860))
        {
            return;
        }

        if (!tcpPacket.Push)
        {
            // ack
            packetQueue.AddRange(tcpPacket.PayloadData);
        }
        else
        {
            // psh + ack
            packetQueue.AddRange(tcpPacket.PayloadData);
            var combinedPacket = packetQueue.ToArray();
            packetQueue.Clear();
            SendPacketToProcessing(combinedPacket, tcpPacket.DestinationPort == 25860, isRemote,
                rawCapture.Timeval.Date);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
}

int GetSequenceNumber (byte[] buffer)
{
    return (buffer[7] << 8) + buffer[6];
}

void ProcessPacketBufferWithRetry (bool isClient)
{
    while (true)
    {
        try
        {
            ProcessPacketBuffer(isClient);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        Thread.Sleep(1000);
    }
}

void ProcessPacketBuffer (bool isClient)
{
    var packetBuffer = isClient ? clientPacketsToProcess : serverPacketsToProcess;
    var packetsToProcess = new List<byte[]>();
    var now = DateTime.UtcNow;
    foreach (var entry in packetBuffer)
    {
        // skip processed or new
        if (entry.WasProcessed || entry.ArrivalTime > now.AddSeconds(-1))
        {
            continue;
        }

        entry.WasProcessed = true;
        packetsToProcess.Add(entry.Buffer);
    }

    packetsToProcess.Sort((a, b) =>
    {
        if (a.Length < 7 || b.Length < 7)
        {
            return -1;
        }

        return GetSequenceNumber(a).CompareTo(GetSequenceNumber(b));
    });

    var combinedPackets = new List<byte[]>();

    for (var i = 0; i < packetsToProcess.Count; i++)
    {
        var temp = new List<byte>(packetsToProcess[i]);
        if (i == packetsToProcess.Count - 1 ||
            GetSequenceNumber(packetsToProcess[i + 1]) != GetSequenceNumber(packetsToProcess[i]))
        {
            combinedPackets.Add(temp.ToArray());
            continue;
        }

        var j = 1;
        while (i + j < packetsToProcess.Count &&
               GetSequenceNumber(packetsToProcess[i + j]) == GetSequenceNumber(packetsToProcess[i]))
        {
            temp.AddRange(packetsToProcess[i + j]);
            j++;
        }

        combinedPackets.Add(temp.ToArray());

        i += j - 1;
    }

    foreach (var data in combinedPackets)
    {
        var len = data.Length;
        var ping = pingCaptureFile;
        var client = clientCaptureFile;
        var server = serverCaptureFile;
        var mixed = mixedCaptureFile;
        // ping
        if (len == 38 && isClient && data[17] != 0)
        {
            var coords = CoordsHelper.GetCoordsFromPingBytes(data);
            if (!coords.Equals(oldCoords))
            {
                WriteWithRetry(ping, coords.ToFileDumpString());
                WriteWithRetry(currentWorldCoordsFile,
                    coords.x + "\n" + coords.y + "\n" + coords.z + "\n" + coords.turn + "\n");
                WriteWithRetry(currentWorldCoordsFile, "---------------\n");
                oldCoords = coords;
            }

            return;
        }

        var prefix = "";
        var prefixForMixed = "";
        var newSessionDivider = ByteArrayCompare(data, newSessionFirstPacket) ? sessionDivider : "";
        var currentPacketPaddedHex = prefix + Convert.ToHexString(data) + "\n";
        var currentPacketPaddedHexForMixed = prefixForMixed + Convert.ToHexString(data) + "\n";
        if (isClient && !ShouldHidePacket(data))
        {
            var actionSource = "";
            var actionDestination = "";
            if (data[0] == 0x20)
            {
                //damage
                actionDestination = Convert.ToString(GetDestinationIdFromDamagePacket(data), 16).PadLeft(4, '0') +
                                    "\t\t\t";
            }

            // chat message
            // if (data[0] == 0x1A && data.Length > 50 && data[13] == 0x08 && data[14] == 0x40 && data[15] == 0x43)
            // {
            //     var win1251 = Encoding.GetEncoding(1251);
            //     var firstPacket = originalData[..26];
            //     var firstPacketDecoded = DecodeClientPacket(firstPacket);
            //     var packetCount = (firstPacketDecoded[23] >> 5) + ((firstPacketDecoded[24] & 0b11111) << 3);
            //     var packetStart = 26;
            //     var decodeList = new List<byte>();
            //     for (var i = 0; i < packetCount; i++)
            //     {
            //         var packetLength = originalData[packetStart + 1] * 256 + originalData[packetStart];
            //         var packetEnd = packetStart + packetLength;
            //         var packetDecode = DecodeClientPacket(originalData[packetStart..packetEnd]);
            //         packetStart = packetEnd;
            //         decodeList.AddRange(packetDecode);
            //     }
            //
            //     var secondPacketLength = originalData[27] * 256 + originalData[26];
            //     var secondPacketStart = 26;
            //     var secondPacketEnd = secondPacketStart + secondPacketLength;
            //     var secondPacket = originalData[secondPacketStart..secondPacketEnd];
            //     var secondPacketDecoded = DecodeClientPacket(secondPacket);
            //     var thirdPacketDecoded = Array.Empty<byte>();
            //     var fourthPacketDecoded = Array.Empty<byte>();
            //     if (originalData.Length - secondPacketEnd > 0)
            //     {
            //         var thirdPacketLength = originalData[secondPacketEnd + 1] * 256 + originalData[secondPacketEnd];
            //         var thirdPacketEnd = secondPacketEnd + thirdPacketLength;
            //         var thirdPacket = originalData[secondPacketEnd..thirdPacketEnd];
            //         thirdPacketDecoded = DecodeClientPacket(thirdPacket);
            //         if (originalData.Length - thirdPacketEnd > 0)
            //         {
            //             var fourthPacketLength = originalData[thirdPacketEnd + 1] * 256 + originalData[thirdPacketEnd];
            //             var fourthPacketEnd = thirdPacketEnd + fourthPacketLength;
            //             var fourthPacket = originalData[thirdPacketEnd..fourthPacketEnd];
            //             fourthPacketDecoded = DecodeClientPacket(fourthPacket);
            //         }
            //     }
            //
            //     var chatMessageData = new List<byte>();
            //     // var fullPacketData = new List<byte>();
            //     var decodedPart = DecodeClientPacket(originalData[..298], 35);
            //     for (var i = 47; i < 297; i++)
            //     {
            //         chatMessageData.Add((byte) ((decodedPart[i] >> 5) + ((decodedPart[i + 1] & 0b11111) << 3)));
            //     }
            //
            //     if (originalData.Length > 298)
            //     {
            //         var end = Math.Min(571, originalData.Length);
            //         var decodedPart2 = DecodeClientPacket(originalData[299..end]);
            //         for (var i = 21; i < decodedPart2.Length - 1; i++)
            //         {
            //             chatMessageData.Add((byte) ((decodedPart2[i] >> 5) + ((decodedPart2[i + 1] & 0b11111) << 3)));
            //         }
            //     }
            //
            //     if (originalData.Length > 571)
            //     {
            //         var end = Math.Min(844, originalData.Length); // should be 802 max
            //         var decodedPart3 = DecodeClientPacket(originalData[572..end]);
            //         for (var i = 21; i < end - 573; i++)
            //         {
            //             if (decodedPart3[i + 1] == 0)
            //             {
            //                 break;
            //             }
            //
            //             chatMessageData.Add((byte) ((decodedPart3[i] >> 5) + ((decodedPart3[i + 1] & 0b11111) << 3)));
            //         }
            //     }
            //
            //     var chatData = chatMessageData.ToArray();
            //     var chatString = win1251.GetString(chatData);
            //     var nameClosingTagIndex = chatString.IndexOf("</l>: ", StringComparison.OrdinalIgnoreCase);
            //     var name = "?";
            //     var message = "?";
            //     if (nameClosingTagIndex > 0)
            //     {
            //         var nameStart = chatString.IndexOf("\\]\"", nameClosingTagIndex - 30,
            //             StringComparison.OrdinalIgnoreCase);
            //         name = chatString[(nameStart + 4)..nameClosingTagIndex];
            //         message = chatString[(nameClosingTagIndex + 6)..].TrimEnd((char) 0); // weird but necessary
            //     }
            //
            //     var chatTypeVal = ((firstPacketDecoded[18] & 0b11111) << 3) + (firstPacketDecoded[17] >> 5);
            //     string chatTypeStr;
            //     if (Enum.IsDefined(typeof (PrivateChatType), chatTypeVal))
            //     {
            //         chatTypeStr = $"[{Enum.GetName(typeof (PrivateChatType), chatTypeVal)!}]";
            //     }
            //     else if (Enum.IsDefined(typeof (PublicChatType), chatTypeVal))
            //     {
            //         chatTypeStr = $"[{Enum.GetName(typeof (PublicChatType), chatTypeVal)!}]";
            //     }
            //     else
            //     {
            //         chatTypeStr = "[Unknown]";
            //     }
            //
            //     WriteWithRetry(mixed,
            //         $"{newSessionDivider}CLI\t\t\t{DateTime.Now}\t\t\t{actionSource}{actionDestination}" +
            //         $"{Convert.ToHexString(firstPacketDecoded)}{Convert.ToHexString(decodeList.ToArray())}\n");
            //     WriteWithRetry(chatFile, $"CLI\t\t{DateTime.Now}\t\t{chatTypeStr,-9} {name}: {message} \n");
            // }
            // else
            // {
            //     WriteWithRetry(client,
            //         $"{newSessionDivider}{DateTime.Now}\t\t\t{actionSource}{actionDestination}{currentPacketPaddedHex}");
            //     WriteWithRetry(mixed,
            //         $"{newSessionDivider}CLI\t\t\t{DateTime.Now}\t\t\t{actionSource}{actionDestination}{currentPacketPaddedHexForMixed}");
            // }

            // if (data[0] == 0x1A)
            // {
            //     // item move
            //     WriteWithRetry(itemMove, $"CLI\t\t\t{DateTime.Now}\t\t\t{currentPacketPaddedHex}");
            // }
        }
        else
        {
            if (!ShouldHidePacket(data))
            {
                if (data[0] != 0x17)
                {
                    WriteWithRetry(server, $"{newSessionDivider}{DateTime.Now}\t\t\t{currentPacketPaddedHex}");
                    WriteWithRetry(mixed,
                        $"{newSessionDivider}SRV\t\t\t{DateTime.Now}\t\t\t{currentPacketPaddedHexForMixed}");
                    if (data[0] == 0x2E)
                    {
                        // item move
                        WriteWithRetry(itemMove, $"SRV\t\t\t{DateTime.Now}\t\t\t{currentPacketPaddedHex}");
                    }

                    var objectList = GetObjectsFromPacket(data);
                    if (objectList.Count > 0)
                    {
                        // object packet?
                        WriteWithRetry(objectPacketDecodeFile,
                            $"SRV\t\t\t{DateTime.Now}\t\t\t{currentPacketPaddedHex}");
                        objectList.ForEach(x => itemCollection.Insert(x));
                        WriteWithRetry(objectPacketDecodeFile,
                            $"{GetTextOutput(objectList, true)}\n" +
                            $"({objectList.Count} total)\n" +
                            "----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n");
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
            // }
        }
    }
}

void SendPacketToProcessing (byte[] data, bool isClient, bool isRemote, DateTime packetTime)
{
    var len = data.Length;
    if (len is 0 or 4) // or 8 or 11 or 12 or 13 or 16 or 17 or 18)
    {
        return;
    }

    var originalData = new byte[data.Length];
    for (var index = 0; index < data.Length; index++)
    {
        originalData[index] = data[index];
    }

    data = isClient && isRemote ? DecodeClientPacket(data) : data;

    if (!isClient)
    {
        serverPacketsToProcess.Add(new PacketBufferEntry
        {
            Buffer = data,
            ArrivalTime = packetTime,
            WasProcessed = false,
            OriginalBuffer = null
        });
    }
    else
    {
        clientPacketsToProcess.Add(new PacketBufferEntry
        {
            Buffer = data,
            ArrivalTime = packetTime,
            WasProcessed = false,
            OriginalBuffer = originalData
        });
    }
}

void CleanupOldPackets ()
{
    while (true)
    {
        clientPacketsToProcess.RemoveAll(x => x.WasProcessed && x.ArrivalTime < DateTime.Now.AddSeconds(-10));
        serverPacketsToProcess.RemoveAll(x => x.WasProcessed && x.ArrivalTime < DateTime.Now.AddSeconds(-10));
        Thread.Sleep(10000);
    }
}

static byte[] DecodeClientPacket (byte[] input, int start = 9)
{
    if (input.Length <= 9)
    {
        return input;
    }

    var encoded = input[start..];
    var result = new byte[encoded.Length + start];
    byte mask3 = 0x0;
    var encodingMask = new byte[] { 0x4B, 0x0D, 0xEF, 0x60, 0xC9, 0x9A, 0x70, 0x0E, 0x03 };
    for (var i = 0; i < encoded.Length; i++)
    {
        var curr = (byte) (encoded[i] ^ encodingMask[i % 9] ^ mask3);
        result[i + start] = curr;
        mask3 = (byte) (curr * i + 2 * mask3);
    }

    Array.Copy(input, result, start);
    return result;
}

static string ByteArrayToBinaryString (byte[] ba, bool noPadding = false, bool addSpaces = false)
{
    var hex = new StringBuilder(ba.Length * 2);
    foreach (var val in ba)
    {
        var str = Convert.ToString(val, 2);
        if (!noPadding)
        {
            str = str.PadLeft(8, '0');
        }

        hex.Append(str);
        if (addSpaces)
        {
            hex.Append(' ');
        }
    }

    return hex.ToString();
}

static int GetDestinationIdFromDamagePacket (byte[] rcvBuffer)
{
    var destBytes = rcvBuffer[28..];
    return ((destBytes[2] & 0b11111) << 11) + (destBytes[1] << 3) + ((destBytes[0] & 0b11100000) >> 5);
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
//     "1a00125d151d2c01007f155f1e3bce5b6c46fb4d2db665a280ee1101db5f191d2c01007f155f1e3bce5b8c78cbcd6fe04eec1b8bcf942f204093a1ec41fa750983925a7849cdbb363e03bb0995652349c9c3e7d27472a93de82886c6cb3691752b3c8e772644b1588b32184d916a21a0f4b2fb165fc1247b4937e74f838fa1a0188eb3c2f741be76b6d40d6b7d778a70095847d3c8fdc2801d16e37b5baf4e459c4860a52e74c7b1d487ab8df7235c53173d20382f4a27af249d16c23a03acf5e62094564b8b855f5b1521f6997cce130a405580134e5489edca15b42cf2b3d20f594cefa9cf0bfb03277960c486a7beab25ea9a5e882113f1cbf23cb9b03b2f14959ea0e1d64f3bffef2a7da09c2851a25c7badcc572bad5743c065dc4062714ab6f750c2d72bb5a188b1110134591c1d2c01007f155f1e3bce5b8c78cbed2f40ee49fe82635bc5dc67dd4188f26a220b2ab7595fc706741ce0f3d4b1e8738a03ed3c0bb8a6b072e74d4c444ae915dd5ab58bd73e8c1ffad4214bc731ad4de2262754cac2d5de2f5bef615c9b030cf0be11b1660921fc1d51cf90e1c953843f4275c6a275844a6f0248b378596ee490ee5b0b253f7bc95d82d5581eafaebc19687b2a02d1efcafe60c365591289c1328d735f39be473c6899aa4f152e874fbaa78937e0f70cf0b12ca883ecd8dca3c6786576db75b14db5f7aed4df53ac4bb13ae593bcc329448f4e90763759091fbc418e3002f3c55dd91cc3cb01bf963d389e2bf23a125f5ae76f4e64173649e5d3e8a84246eda5f21b18bb61f7370088a0201d2c01007f155f1e3bce5bcca3635d4fe0841f46c8bedbc7db710ed18bec2ca0289d56b00c8abbae6f06cca01b06487ee38b");
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

Task.Run(() => { ProcessPacketBufferWithRetry(false); });
Task.Run(() => { ProcessPacketBufferWithRetry(true); });
Task.Run(CleanupOldPackets);

// ProcessPacket(test, true, true);
ethernet.Open();
ethernet.Capture();
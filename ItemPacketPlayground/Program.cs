
Console.WindowWidth = 256;

var fromCode = true;

if (fromCode)
{
    var container = Convert.FromHexString(
        "C6002C0100D44C775F5446E115C6E8BEE6C2C6BEE660007E785FD88B0F30512F09403D041900008908409145A610031760609C0FA0900500FFFFFFFFBFBC2F2AC307D863970490A08B0C00985804A0C82203400106C6F9000A131050788531BAAFB9B0B1AFB91A809FDE179CE1037CCC4B0270AE400600401EC651649101A00003E37C00850908F8ED7DE12E3E9467F4E3C164E764C01BF06300851599DE8B30051818E703286401C0FFFFFFFF8FEF0B7AF101CFD225014E962203C006180128A8C8AC8B84");
        //"F8C1EE53193E00EDBB24C0C8E7640089096400451619000A3070510F50988080C22B8CD17DCD858D7DCDC100FCBCE8A90C1FB08A5D128881683280D1E63180228B0C000518B8A807284C4040E115C6E8BEE6C2C6BEE660007ED3E3D0870F984E2E09A0F00919006FD358519145A63BA34801062EEA010A5900F0FFFFFF0F");

    var bytePacketSplit = new List<byte[]>();
    
    for (var i = 0; i < container.Length - 4; i++)
    {
        if (container[i + 2] == 0x2C && container[i + 3] == 0x01 && container[i + 4] == 0x00)
        {
            var length = container[i+1] * 16 + container[i];

            if (i + length - 1 <= container.Length)
            {
                var split = container[i..(i + length - 1)];
                bytePacketSplit.Add(split);
                i += length - 1;
            }
        }
    }

    foreach (var split in bytePacketSplit)
    {
        var items = BitStreamTools.GetItemsFromPacket(split);

        foreach (var item in items)
        {
            Console.WriteLine(item.ToDebugString());
        }
        
    }
}

else
{
    while (true)
    {
        var container = Convert.FromHexString(Console.ReadLine());
        var items = BitStreamTools.GetItemsFromPacket(container);

        foreach (var item in items)
        {
            Console.WriteLine(item.ToDebugString());
        }
    }
}


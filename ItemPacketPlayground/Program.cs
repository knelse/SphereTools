
Console.WindowWidth = 256;

var fromCode = true;

if (fromCode)
{
    var container = Convert.FromHexString(
        "A8002C0100269213DCA48F0F80842E090000000000000000409145665812156040821BA0C00201003F0A6EEEC507404297040000000000000000A0C82213F7890A3020C10D50C80280FFFFFFFF5F0537F8E20320A14B02000000000000000050649109FD88518001096E80421600FCFFFFFF17DE41EDEE8B2C2DAC4D2ECDED2C4606E067C1CDBBF80048E8920000000000000000001459648A3E51010624B8010A5900F0FFFFFF0F");
        //"F8C1EE53193E00EDBB24C0C8E7640089096400451619000A3070510F50988080C22B8CD17DCD858D7DCDC100FCBCE8A90C1FB08A5D128881683280D1E63180228B0C000518B8A807284C4040E115C6E8BEE6C2C6BEE660007ED3E3D0870F984E2E09A0F00919006FD358519145A63BA34801062EEA010A5900F0FFFFFF0F");

    var bytePacketSplit = new List<byte[]>();
    
    for (var i = 0; i < container.Length - 4; i++)
    {
        if (container[i + 2] == 0x2C && container[i + 3] == 0x01 && container[i + 4] == 0x00)
        {
            var length = container[i+1] * 16 + container[i];
            var split = container[i..(i + length - 1)];
            bytePacketSplit.Add(split);
            i += length - 1;
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


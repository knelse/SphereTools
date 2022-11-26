
using static ObjectPacketTools;

Console.WindowWidth = 300;

var fromCode = true;

if (fromCode)
{
    // ma missing
    var container = Convert.FromHexString(
        "4BF214870F80842E090000000000000000409145669F101560C0521DA0900500FFFFFFFF05166800F831C9B3053E0012BA24000000000000000000451619000A3060A90E50C80280FFFFFFFFC20BEC01009F8A7DE017DB049DCB105C5DF4D93E00");
           // "23D8DC8B0FF0BA2D09406B1D1980A7F418407145A61C168D60732F0518600107286401C0FFFFFF7FC103027E51D8DC8B0F88142D09D0471F1980D4E918589145261C16156080051C0051D8DC4B210B00FEFFFFFF7F");

    var objects = GetObjectsFromPacket(container, true);

    foreach (var obj in objects)
    {
        Console.WriteLine(obj.ToDebugString());
    }
}

else
{
    while (true)
    {
        var container = Convert.FromHexString(Console.ReadLine());
        var objects = GetObjectsFromPacket(container, true);

        foreach (var obj in objects)
        {
            Console.WriteLine(obj.ToDebugString());
        }
    }
}


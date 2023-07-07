using System.Text;
using BitStreams;

string hexString;

Console.WriteLine("Input hexstring:");
while (!string.IsNullOrEmpty(hexString = Console.ReadLine()))
{
    Console.WriteLine("Input nums to find:");
    var bytes = Convert.FromHexString(hexString);
    var numsToFindStr = Console.ReadLine();
    var numsToFind = numsToFindStr.Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
    if (string.IsNullOrEmpty(numsToFindStr))
    {
        Console.WriteLine("No nums, start over");
        continue;
    }
    var stream = new BitStream(bytes);

    foreach (var num in numsToFind)
    {
        var positions = new List<int>();
        var readLength = IntToBits(num).Length;
        while(stream.ValidPosition)
        {
            var bits = stream.ReadBits(readLength);
            if (BitsToInt(bits) == num)
            {
                positions.Add((int) stream.Offset * 8 + stream.Bit);
            }

            if (!stream.ValidPosition)
            {
                break;
            }

            var returnLength = readLength - 1;

            stream.Offset -= returnLength / 8;
            stream.Bit -= returnLength % 8;
        }
        stream.Seek(0, 0);
        if (positions.Any())
        {
            var sb = new StringBuilder();
            positions.ForEach(x => sb.Append($" {x}"));
            Console.WriteLine($"{num}:{sb.ToString()}");
        }
    }

    Console.WriteLine("Input hexstring:");
}

Bit[] IntToBits(int val)
{
    if (val < 0)
    {
        val = int.MaxValue + val + 1;
    }
    var result = new List<Bit>();

    while (val > 0)
    {
        result.Add(val & 0b1);
        val >>= 1;
    }

    return result.ToArray();
}

int BitsToInt(Bit[] bits)
{
    var result = 0;

    for (var i = bits.Length - 1; i >= 0; i--)
    {
        result <<= 1;
        result += bits[i];
    }

    return result;
}
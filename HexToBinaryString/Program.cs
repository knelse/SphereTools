
using System.Text;

var shift = int.Parse(Console.ReadLine());

while (true)
{
    try
    {
        var str = Console.ReadLine()!.Replace(" ", "").Replace("\t","");
        var ba = Convert.FromHexString(str);
        Console.Write(ByteArrayToBinaryString(ref ba, shift));
        Console.WriteLine();

        var sb = new StringBuilder();

        for (var i = 0; i < ba.Length; i++)
        {
            sb.Append(Convert.ToString(ba[i], 16).PadLeft(2, '0').ToUpper());
        }

        Console.WriteLine(sb.ToString());
    }
    catch
    {
        Console.WriteLine("Bad input, try again");
    }
}

static string ByteArrayToBinaryString(ref byte[] ba, int shift, bool noPadding = false) 
{
    if (shift != 0)
    {
        if (shift > 0)
        {
            var mask = (int) (Math.Pow(2, shift)) - 1;
            for (var i = 0; i < ba.Length - 2; i++)
            {
                ba[i] = (byte) (((ba[i + 1] & mask) << (8 - shift)) + (ba[i] >> shift));
            }

            ba[^1] >>= shift;
        }
        else
        {
            shift = -shift;
            var mask = 0xFF - ((int) (Math.Pow(2, 8 - shift)) - 1);
            for (var i = ba.Length - 1; i > 0; i--)
            {
                ba[i] = (byte) ((ba[i] << shift) + ((ba[i - 1] & mask) >> (8 - shift)));
            }

            ba[0] <<= shift;
        }
    }

    var hex = new StringBuilder(ba.Length * 2);
    
    // foreach (var val in ba)
    // {
    //     var str = Convert.ToString(val, 2);
    //     if (!noPadding) str = str.PadLeft(8, '0');
    //     hex.AppendLine(str); // + "\t" + $"{val:X2}");
    //     //hex.Append(' ');
    // }
    
    return hex.ToString();
}//989271689 987193993 974625417 975668361 977782152
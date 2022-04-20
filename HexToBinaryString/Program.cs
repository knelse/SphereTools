
using System.Text;

while (true)
{
    try
    {
        var str = Console.ReadLine().Replace(" ", "");
        Console.WriteLine(ByteArrayToBinaryString(Convert.FromHexString(str)));
        Console.WriteLine();
    }
    catch
    {
        Console.WriteLine("Bad input, try again");
    }
}

static string ByteArrayToBinaryString(byte[] ba, bool noPadding = false)
{
    var hex = new StringBuilder(ba.Length * 2);
    
    foreach (var val in ba)
    {
        var str = Convert.ToString(val, 2);
        if (!noPadding) str = str.PadLeft(8, '0');
        hex.Append(str);
        //hex.Append(' ');
    }
    
    return hex.ToString();
}
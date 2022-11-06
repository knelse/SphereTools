
using System.Text;

while (true)
{
    try
    {
        var str = Console.ReadLine()!.Replace(" ", "");
        Console.Write(ByteArrayToBinaryString(Convert.FromHexString(str)));
        Console.WriteLine();

        for (var i = 0; i < str.Length; i += 2)
        {
            Console.WriteLine(str[i].ToString() + str[i + 1].ToString());
        }
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
        hex.AppendLine(str); // + "\t" + $"{val:X2}");
        //hex.Append(' ');
    }
    
    return hex.ToString();
}//989271689 987193993 974625417 975668361 977782152
// See https://aka.ms/new-console-template for more information

using System.Text;

while (true)
{
    var str = Console.ReadLine()?.Trim();

    if (str == null)
    {
        continue;
    }
    
    try
    {
        var offset = 18;
        var encoded = Convert.FromHexString(str[offset..]);

        var result = new byte[encoded.Length];
        byte mask3 = 0x0;
        var encodingMask = new byte[] { 0x4B, 0x0D, 0xEF, 0x60, 0xC9, 0x9A, 0x70, 0x0E, 0x03 };

        for (var i = 0; i < encoded.Length; i++)
        {
            var curr = (byte)(encoded[i] ^ encodingMask[i % 9] ^ mask3);
            result[i] = curr;
            mask3 = (byte)(curr * i + 2 * mask3);
        }

        var x = DecodeClientCoordinate(result[12..17]);
        var y = DecodeClientCoordinate(result[16..21]);
        var z = DecodeClientCoordinate(result[20..25]);
        var turn = DecodeClientCoordinate(result[24..29]);
        Console.WriteLine();
        // Console.WriteLine(ByteArrayToBinaryString(result[16..21]));
        Console.WriteLine(x + "\n" + y +  "\n" + z + "\n" + turn);
        Console.WriteLine();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}

byte[] EncodeServerCoordinate(double a)
{
    var scale = 69;
        
    var a_abs = Math.Abs(a);
    var a_temp = a_abs;
        
    var steps = 0;

    if (((int)a_abs) == 0)
    {
        scale = 58;
    }

    else
    {
        while (a_temp < 2048)
        {
            a_temp *= 2;
            steps += 1;
        }

        scale -= (steps + 1) / 2;

        if (scale < 0)
        {
            scale = 58;
        }
    }

    var a_3 = (byte) (((a < 0 ? 1 : 0) << 7) + scale);
    var mul = Math.Pow(2, ((int)Math.Log2(a_abs)));
    var numToEncode = (int) (0b100000000000000000000000 * (a_abs / mul - 1));
    var a_2 = (byte) (((numToEncode & 0b111111111111111100000000) >> 16) + (steps % 2 == 1 ? 0b10000000 : 0));
    var a_1 = (byte)((numToEncode & 0b1111111100000000) >> 8);
    var a_0 = (byte) (numToEncode & 0b11111111);

    return new [] { a_0, a_1, a_2, a_3};
}
int DecodeServerCoordinate(byte[] a)
{
    var steps = (5 - a[2] & 0b111) * 2;

    if ((a[1] & 0b10000000) > 0)
    {
        steps -= 1;
    }

    var a_last4 = (a[0] & 0b11110000) >> 4;
    var a_next7 = (a[1] & 0b01111111) << 4;
    var a_first1 = (a[2] & 0b01000000) << 5;

    var mul = (int) (Math.Pow(2, steps));

    return (a_first1 + a_next7 + a_last4) / mul * ((a[2] & 0b10000000) > 0 ? -1 : 1);
}

double DecodeClientCoordinate(byte[] a)
{
    var x_scale = ((a[4] & 0b11111) << 3) + ((a[3] & 0b11100000) >> 5);

    if (x_scale == 126)
    {
        return 0.0;
    }
        
    var baseCoord = Math.Pow(2, x_scale - 127);
    var sign = (a[4] & 0b100000) > 0 ? -1 : 1;
    return ((1 + ((float)(((a[3] & 0b11111) << 18) + (a[2] << 10) + (a[1] << 2) +
                          ((a[0] & 0b11000000) >> 6))) / 0b100000000000000000000000) * baseCoord) * sign;
}

string ByteArrayToBinaryString(byte[] ba, bool noPadding = false, bool addSpaces = false)
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
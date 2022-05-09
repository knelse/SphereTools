// See https://aka.ms/new-console-template for more information
while (Console.ReadLine() is { } str)
{
    if (str.Length < 10)
    {
        continue;
    }

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

    Console.WriteLine((str[..offset] + Convert.ToHexString(result)).ToLower());
}
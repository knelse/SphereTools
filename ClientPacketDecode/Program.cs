// See https://aka.ms/new-console-template for more information

using System.Text;

// var str =
var
    _str = //"1a00a4b981022c01000d0894d88776abcc067b2d84397bbe389e1101ca4685022c01000d0894d88776ab2c384bcd6fe04eec1b8bcf942f204093a1ec0198006a0428b2c4b6603459d83785cfe8bff362df0b5d4fe0b2288799387154727282255abc523fd6c3312763c21848c0b7d09928cb269375348f180e6c9e460949123ad901618e69cf1bf610bb52700a9d7b73ac8b05b6b977e5dee06e24c9acd15d3039d0ec4a9fbf4200f643239d34e893b4a9f3456db10db018f0433fb9eda05a8983fe904e5bc800092d8fd0f109b3c4e033512c10a91ffe5907dbf2d674b82cf6d1e574fc09c29e007cd0f0fdc1752bfa0894c200b02c092d420eb6d5de22dff4413b05b2a08aeaf9a81a0c6978445e8342fd5a707643c0b71ca10638286802127f4a700a7d985ef2801e897c004fb888022c01000d0894d88776ab8ccafb0d6fc0e1ba98febb885079ddbf40b15923749093315ce73f533414a11c26cc8f31b991648c01743d53affbe1d2eedb0d7a22fde5b7c845088d94d8ea507bf9c01f5182814bc522607a641d6cdd059cbd3441b8ec782958b52f30e73dcccf260ae6f772a7d11c316a90";
        //"1a00cf005e022c0100d709f511e6b42fc4165b2d820f176688fe1101610162022c0100d709f511e6b42f24286b0defe04eec1b8bcf942f204093a1ec0198006a0428b2c4b6603459d83785cfe8bff362df0b5d4fe0b2288799387154727282255abc523fd6c3312763c21848c0b7d09928cb269375348f180e6c9e460949123ad901618e69cf1bf610bb52700a9d7b73ac8b05b6b977e5dee06e24c9acd15d3039d0ec4a9fbf4200f643239d34e893b4a9f3456db10db018f0433fb9eda05a8983fe904e5bc800092d8fd0f109b3c4e033512c10a91ffe5907dbf2d674b82cf6d1e574fc09c29e007cd0f0fdc1752bfa0894c200b02c092d420eb6d5de22dff4413b05b2a08aeaf9a81a0c6978445e8342fd5a707643c0b71ca10638286802127f4a700a7d985ef2801e898e00f5ff64022c0100d709f511e6b42f4498abadaf40e1ba98febb885079ddbf40b15923749093315ce73f533414a11c26cc8f31b991648c01743d53affbe1d2eedb0d7a22fde5b7c845088d94d8ea507bf9c01f5182814bc522607a641d6cdd059cbd3441b8ec782958b52f30e75d4cad92349edf4338fbc59761b4bf2127c989dc9372e64a70af3a0ea24d9260";
        "2600060d8c022c01001ee4f2cb5fd4f1c8079c3bf720987d5b302468d94c9b546cd96d2a1fa3";
// "300031bfd7012c0100ff0da09148e894bf20527f325e76e6c52cb0c38441c78dc70a45939a0bb08ccc08937616c42ba3";
//        "4500fab8d7012c01009b0ca1af4ce0842b947aafd29ef6e6c52cb0c38441c78dc70a45939a0bb08ccc08937616c4ef8bfc81e6601a4b47cd6bd8b5aa84a66f6be9773c1d32"
//  "19004abdd8012c01008e004f6f0840401dfc87898d01c40000";
//    "19008c42d7012c01004309a4a95888d45ba45a6f520ee752e0";
//  "190061bdd8012c0100b50da09148e894dba45a6f520ee752e0"
//  "190089bdd6012c0100cf0da09148e894dba45a6f520ee752e0"
//  "190089bdd6012c0100cf0da09148e894dba45a6f520ee752e0"
//  "190093bdd6012c0100c50da09148e894dba45a6f520ee752e0"
// try
// {

var strs = new List<string>
{
    "2600160e5c022c01001ee4f2cb5f545188879c3bf720987d5b302468d94c9b546cd96d2a1fa3",
    "260053086b022c01001ee4f2cb5f545d4017bc7b7720987d5b302468d94c9b546cd96d2a1fa3",
    "2600dd0e7a022c01001ee4f2cb5f54660a83942bd7e0187d5b302468d94c9b546cd96d2a1fa3",
    "2600060d8c022c01001ee4f2cb5fd4f1c8079c3bf720987d5b302468d94c9b546cd96d2a1fa3",
    "26004a0d94022c01001ee4f2cb5fd4fd8097bc7b7720987d5b302468d94c9b546cd96d2a1fa3",
    "2600000f9b022c01001ee4f2cb5f548a523374eb57e0187d5b302468d94c9b546cd96d2a1fa3"
};

var decodeResults = new List<string>();

foreach (var str in strs)
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

    decodeResults.Add(str[..offset].ToUpper() + Convert.ToHexString(result));
}

var comparisonOffset = 18;

Console.ForegroundColor = ConsoleColor.White;
Console.WindowWidth = 200;
Console.WriteLine(decodeResults[0][comparisonOffset..].ToUpper());
Console.WriteLine("----");

var strAbytes = Convert.FromHexString(decodeResults[0].ToUpper()[comparisonOffset..]); 

foreach (var stringToCompare in decodeResults.Skip(1))
{
    var strBbytes = Convert.FromHexString(stringToCompare[comparisonOffset..]);

    for (var i = 0; i < strAbytes.Length; i++)
    {
        if (strAbytes[i] != strBbytes[i])
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
        }

        Console.Write(Convert.ToHexString(strBbytes[i..(i + 1)]));

        Console.ForegroundColor = ConsoleColor.White;
    }
    Console.WriteLine();
}
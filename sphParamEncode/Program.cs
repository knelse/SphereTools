using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

if (args.Length < 2)
{
    Console.WriteLine("Usage: sphParamEncode.exe <input_file_path> <output_file_path>");
    Environment.Exit(1);
}

var inputPath = args[0];

if (!File.Exists(inputPath))
{
    Console.WriteLine($"File not found: {inputPath}");
    Environment.Exit(1);
}

var outputPath = args[1];

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
var win1251 = Encoding.GetEncoding(1251);

var inputFileBytes = File.ReadAllBytes(inputPath);
var outputFile = File.Open(outputPath, FileMode.Create);
var inputMemoryStream = new MemoryStream();

var deflaterStream = new DeflaterOutputStream(inputMemoryStream, new Deflater(1));
deflaterStream.Write(inputFileBytes);
deflaterStream.Close();

var inputBuffer = inputMemoryStream.ToArray();
inputBuffer[1] ^= 0x78;
inputBuffer[9] ^= 0x78;
inputBuffer[12] ^= 0x78;

var outputFileWriter = new StreamWriter(outputFile, win1251);
var crcOrSmth = new byte [4];
crcOrSmth[0] = 0x00;
crcOrSmth[1] = 0x00;
crcOrSmth[2] = inputBuffer[6];
crcOrSmth[3] = inputBuffer[6];

outputFileWriter.Write("SPHR");
outputFileWriter.Write(win1251.GetString(crcOrSmth));
outputFileWriter.Write(win1251.GetString(inputBuffer));
outputFileWriter.Close();
outputFile.Close();
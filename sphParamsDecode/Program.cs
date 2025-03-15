using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

if (args.Length < 2)
{
    Console.WriteLine("Usage: sphParamDecode.exe <input_path> <output_path>");
    Environment.Exit(1);
}

var inputPath = args[0];

if (!Directory.Exists(inputPath))
{
    Console.WriteLine($"Directory not found for input_path: {inputPath}");
    Environment.Exit(1);
}

var outputPath = args[1];

Directory.CreateDirectory(outputPath);

var fileList = Directory.EnumerateFiles(inputPath, "*.*", SearchOption.AllDirectories);

var buffer = new byte[1024];

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
var win1251 = Encoding.GetEncoding(1251);

foreach (var filePath in fileList)
{
    try
    {
        var fileContents = File.ReadAllBytes(filePath);

        if (fileContents.Length < 4)
        {
            continue;
        }

        var sphrMarker = win1251.GetString(fileContents[..4]);

        if (!sphrMarker.Equals("SPHR"))
        {
            continue;
        }

        var xor_8 = fileContents[8];
        var xor_14 = fileContents[14];
        fileContents[9] ^= xor_8;
        fileContents[17] ^= xor_8;
        fileContents[20] ^= xor_8;
        fileContents[4] ^= xor_14;
        fileContents[5] ^= xor_14;
        fileContents[6] ^= xor_14;
        fileContents[7] ^= xor_14;

        var ms = new MemoryStream(fileContents[8..]);
        var inflaterStream = new InflaterInputStream(ms);
        
        var fileName = Path.GetFileName(filePath);
        var relativePath = Path.GetRelativePath(inputPath, filePath);
        var currentDirectory = Path.GetDirectoryName(relativePath);
        var outputDirectoryPath = Path.Combine(outputPath, currentDirectory);
        Directory.CreateDirectory(outputDirectoryPath);

        var outputFilePath = Path.Combine(outputDirectoryPath, fileName);
        var outputFile = File.Open(outputFilePath, FileMode.Create);
        StreamUtils.Copy(inflaterStream, outputFile, buffer);
        outputFile.Close();
        
        Console.WriteLine("Processed: " + relativePath);
    }
    catch (IOException e)
    {
    }
} 

// offzip

/*
    var tempDecryptedPath = Path.Combine(outputPath, "temp_decrypted");
    Directory.CreateDirectory(tempDecryptedPath);
    var tempUnzippedPath = Path.Combine(outputPath, "temp_unzipped");
    Directory.CreateDirectory(tempUnzippedPath);
 
    var fileName = Path.GetFileName(filePath);
    var outputDecryptedFilePath = Path.Combine(tempDecryptedPath, fileName);

    File.WriteAllBytes(outputDecryptedFilePath, fileContents);
    Console.WriteLine($"Prepared: {fileName}");

    var offzipAgrs = $"-a -1 -o {outputDecryptedFilePath} {tempUnzippedPath}";
    Console.WriteLine(offzipAgrs);
    using var offzipProcess = Process.Start("D:\\SphereDev\\SphereSource\\source\\offzip\\offzip.exe", offzipAgrs);
    offzipProcess.WaitForExit();

    var offzippedFile = Directory.EnumerateFiles(tempUnzippedPath).FirstOrDefault();

    if (offzippedFile is null)
    {
        continue;
    }

    var currentDirectory = Path.GetDirectoryName(Path.GetRelativePath(inputPath, filePath));
    var outputDirectoryPath = Path.Combine(outputPath, currentDirectory);
    Directory.CreateDirectory(outputDirectoryPath);
    File.Move(offzippedFile, Path.Combine(outputDirectoryPath, fileName), true);
    File.Delete(outputDecryptedFilePath);
 */
using System.Text;
using sphModelInfo;

// var sphModelsFolder = @"c:\Games\Sfera\models\";
// var skippedNames = new List<string>();
//
// foreach (var filePath in Directory.EnumerateFiles(sphModelsFolder))
// {
//     var modelInfo = SphModelInfo.GetModelInfo(filePath);
//
//     if (modelInfo is null)
//     {
//         skippedNames.Add(Path.GetFileName(filePath));
//         continue;
//     }
//     Console.WriteLine($"{modelInfo.Name} vC: {modelInfo.Header.VertexCount} tC: {modelInfo.Header.TriangleCount} sC: {modelInfo.Header.SurfaceCount} " +
//                       $"txC: {modelInfo.Header.TextureCount} tnL: {modelInfo.Header.TextureNamesLength} oC: {modelInfo.Header.ObjectCount} oiC: {modelInfo.Header.ObjectIndexCount} tkC: {modelInfo.Header.TransformKeyCount} " +
//                       $"tkiC: {modelInfo.Header.TransformKeyIndexCount} ac: {modelInfo.Header.ActionCount} {string.Join(", ", modelInfo.TextureNames)}");
// }

// Console.WriteLine($"Skipped: {string.Join('\n', skippedNames)}");

var sphLandscapeFolder = @"c:\Games\Sfera\landscape\";

foreach (var path in Directory.EnumerateFiles(sphLandscapeFolder))
{
    using var file = File.OpenRead(path);
    using var reader = new BinaryReader(file);
    // if (Path.GetExtension(path) == ".siz")
    // {
    //     var t = File.ReadAllBytes(path);
    //     var x = reader.ReadInt32();
    //     var y = reader.ReadInt32();
    //     Console.WriteLine($"{Path.GetFileNameWithoutExtension(path)} X: {x} Y: {y}");
    // }
    // if (Path.GetExtension(path) == ".wtr")
    // {
    //     var curr = 0;
    //     while (file.Position < file.Length)
    //     {
    //         var start = reader.ReadSingle();
    //         var end = reader.ReadSingle();
    //         // Console.WriteLine($"S: {start} E: {end}");
    //         Console.Write(Math.Abs(start - 1000) < float.Epsilon ? "-" : "+");
    //         curr++;
    //         if (curr >= 12)
    //         {
    //             curr = 0;
    //             Console.WriteLine();
    //         }
    //     }
    //
    //     // Console.WriteLine($"Total: {c}");
    //
    //     break;
    // }

    if (Path.GetExtension(path) == ".lnd")
    {
        var magicBytes = reader.ReadBytes(4);
        // var vertexCount = reader.ReadInt16();
        file.Seek(0x1b24, SeekOrigin.Begin);
        var vertexCount = 0;

        while (file.Position < file.Length)
        {
            // var vertex = reader.ReadStruct<Vertex>();
            // Console.WriteLine($"{vertex.X} {vertex.Y} {vertex.Z} {vertex.NormalX} {vertex.NormalY} {vertex.NormalZ} {vertex.TextureU} {vertex.TextureV}");
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();

            Console.WriteLine($"{x} {y} {z}");
            vertexCount++;
        }

        Console.WriteLine(vertexCount);
        break;
    }
}
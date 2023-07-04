using System.Text;
using sphModelInfo;

var sphModelsFolder = @"c:\Games\Sfera\models\";
var skippedNames = new List<string>();

foreach (var filePath in Directory.EnumerateFiles(sphModelsFolder))
{
    var modelInfo = SphModelInfo.GetModelInfo(filePath);

    if (modelInfo is null)
    {
        skippedNames.Add(Path.GetFileName(filePath));
        continue;
    }
    Console.WriteLine($"{modelInfo.Name} vC: {modelInfo.Header.VertexCount} tC: {modelInfo.Header.TriangleCount} sC: {modelInfo.Header.SurfaceCount} " +
                      $"txC: {modelInfo.Header.TextureCount} tnL: {modelInfo.Header.TextureNamesLength} oC: {modelInfo.Header.ObjectCount} oiC: {modelInfo.Header.ObjectIndexCount} tkC: {modelInfo.Header.TransformKeyCount} " +
                      $"tkiC: {modelInfo.Header.TransformKeyIndexCount} ac: {modelInfo.Header.ActionCount} {string.Join(", ", modelInfo.TextureNames)}");
}

Console.WriteLine($"Skipped: {string.Join('\n', skippedNames)}");
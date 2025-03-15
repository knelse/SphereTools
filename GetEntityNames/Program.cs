// See https://aka.ms/new-console-template for more information

using System.Text;

// Use with decoded files (via sphFileDecode in the same repo and skip files for language you don't need (_e is English, _i is Italian, _p is Portuguese and no suffix is Russian))
var entPaths = Directory.EnumerateFiles("D:\\SphereDev\\SphereSource\\source\\_sphFilesDecode\\language\\");
var entDict = new SortedDictionary<int, string>();
using var entOutput = File.Create("D:\\SphereDev\\SphereSource\\source\\entityNamesCollected");
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

foreach (var path in entPaths)
{
    var name = Path.GetFileNameWithoutExtension(path);

    if (name.EndsWith("_e") || name.EndsWith("_i") || name.EndsWith("_p"))
    {
        continue;
    }
    try
    {
        var file = File.ReadAllLines(path, Encoding.GetEncoding(1251));

        for (var i = 0; i < file.Length; i++)
        {
            if (file[i].StartsWith('#'))
            {
                var id = Convert.ToInt32(file[i][1..].Split('-')[0]);
                Console.WriteLine(id + "   " + file[i+1]);
                entDict.Add(id, file[i+1]);
                i++;
            }
        }

    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}

foreach ((var key, var value) in entDict)
{
    entOutput.Write(Encoding.GetEncoding(1251).GetBytes(key + "   " + value + "\n"));
}

var itemPaths = Directory.EnumerateFiles("D:\\SphereDev\\SphereSource\\source\\_sphFilesDecode\\params\\");
var itemsDict = new SortedDictionary<int, string>();
using var itemsOutput = File.Create("D:\\SphereDev\\SphereSource\\source\\itemNamesCollected");

foreach (var path in itemPaths)
{
    try
    {
        var file = File.ReadAllLines(path, Encoding.GetEncoding(1251));

        foreach (var t in file)
        {
            var firstNumEnd = t.IndexOf("\t", StringComparison.InvariantCulture);

            try
            {
                var firstNum = Convert.ToInt32(t[..firstNumEnd]);
                itemsDict.Add(firstNum, t);
                Console.WriteLine(t);
            }
            catch
            {
                
            }
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}

foreach ((var key, var value) in itemsDict)
{
    itemsOutput.Write(Encoding.GetEncoding(1251).GetBytes(key + "   " + value + "\n"));
}
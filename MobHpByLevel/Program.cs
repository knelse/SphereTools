var mobsFile = File.ReadAllLines(@"C:\_sphereDumps\mobs.txt");
var idLevelHp = new SortedDictionary<int, SortedDictionary<int, int>>();

foreach (var line in mobsFile)
{
    var split = line.Split('\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    if (split.Length < 3)
    {
        continue;
    }
    var id = int.Parse(split[0]);
    var level = int.Parse(split[1]);
    var hp = int.Parse(split[2]);
    if (!idLevelHp.ContainsKey(id))
    {
        idLevelHp.Add(id, []);
    }

    idLevelHp[id][level] = hp;
}

foreach (var id in idLevelHp)
{
    var previousHp = -1;
    var previousLevel = -1;
    foreach (var levelHp in id.Value)
    {
        if (previousHp == -1)
        {
            previousLevel = levelHp.Key;
            previousHp = levelHp.Value;
            continue;
        }

        var levelDiff = levelHp.Key - previousLevel;
        var hpDiffAbs = levelHp.Value - previousHp;
        var hpDiffAvg = hpDiffAbs / levelDiff;
    }
}
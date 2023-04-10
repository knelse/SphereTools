using LiteDB;

using var db = new LiteDatabase(@"C:\_sphereStuff\sph.db");

var charCollection = db.GetCollection<CharacterData>("Characters");
var clanCollection = db.GetCollection<Clan>("Clans");
var playerCollection = db.GetCollection<Player>("Players");

charCollection.DeleteAll();
clanCollection.DeleteAll();
playerCollection.DeleteAll();
var clan = new Clan
{
    Name = "TestClan"
};

var character = new CharacterData
{
    Name = "Test",
    GlovesModelId = 1,
    Clan = clan
};

var character2 = new CharacterData
{
    Name = "Test2",
};

var character3 = new CharacterData
{
    Name = "Test3",
    Clan = clan
};

var player = new Player
{
    Login = "abc",
    PasswordHash = "def",
    Characters = new List<CharacterData>
    {
        character, character2, character3
    }
};

if (!clanCollection.Exists(x => x.Id == Clan.DefaultClan.Id))
{
    clanCollection.Insert(Clan.DefaultClan.Id, Clan.DefaultClan);
}

clanCollection.Insert(clan);
charCollection.Insert(character);
charCollection.Insert(character2);
charCollection.Insert(character3);
playerCollection.Insert(player);

var collClan = clanCollection.Find(x => x.Id != Clan.DefaultClan.Id).First();
collClan.Name = "SomeOtherName";
clanCollection.Update(collClan);

var c = charCollection.Include(x => x.Clan).Find(_ => true).First();
var p = playerCollection.Query().Include(new List<BsonExpression> { "$.Characters[*]", "$.Characters[*].Clan" }).First();

Console.WriteLine(c.Name);
Console.WriteLine(c.Clan.Name);
Console.WriteLine(p.Characters.First().Name);
Console.WriteLine(p.Characters.First().Clan.Name);

public enum KarmaTypes
{
    VeryBad = 0x1,
    Bad = 0x2,
    Neutral = 0x3,
    Good = 0x4,
    Benign = 0x5
}

public enum SpecTypes
{
    None = 0x0,
    Assasin = 0x1,
    Crusader = 0x2,
    Inquisitor = 0x3,
    Hunter = 0x4,
    Archmage = 0x5,
    Barbarian = 0x6,
    Druid = 0x7,
    Thief = 0x8,
    MasterOfSteel = 0x9,
    Armorer = 0x10,
    Blacksmith = 0x11,
    Warlock = 0x12,
    Necromancer = 0x13,
    Bandier = 0x14,
}

public enum ClanRank
{
    Senior = 0x0,
    Seneschal = 0x1,
    Vassal = 0x2,
    Neophyte = 0x3
}

public class Clan
{
    [BsonId] public int Id { get; set; }
    public string Name { get; set; }
    
    public static readonly Clan DefaultClan = new ()
    {
        Id = 0,
        Name = "___"
    };
}

public class Player
{
    [BsonId] public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    [BsonRef("Characters")] public List<CharacterData> Characters { get; set; }
}

public class CharacterData
{
    [BsonId] public int ID { get; set; }
    public int Unknown { get; set; }
    public double X { get; set; }
    public double Y { get; set; } = 150;
    public double Z { get; set; }
    public double Turn { get; set; }
    public int CurrentHP { get; set; }
    public int MaxHP { get; set; }
    public int TypeID { get; set; }
    public int TitleLevelMinusOne { get; set; }
    public int DegreeLevelMinusOne { get; set; }
    public int LookType { get; set; } = 0x7;
    public int IsTurnedOff { get; set; } = 0x9;
    public int MaxMP { get; set; } = 100;
    public int Strength { get; set; } = 0;
    public int Agility { get; set; } = 0;
    public int Accuracy { get; set; } = 0;
    public int Endurance { get; set; } = 0;
    public int Earth { get; set; } = 0;
    public int Air { get; set; } = 0;
    public int Water { get; set; } = 0;
    public int Fire { get; set; } = 0;
    public int PDef { get; set; } = 0;
    public int MDef { get; set; } = 0;
    public KarmaTypes Karma { get; set; } = KarmaTypes.Neutral;
    public int MaxSatiety { get; set; } = 100;
    public uint TitleXP { get; set; } = 0;
    public uint DegreeXP { get; set; } = 0;
    public int CurrentSatiety { get; set; } = 50;
    public int CurrentMP { get; set; } = 100;
    public int AvailableTitleStats { get; set; } = 4;
    public int AvailableDegreeStats { get; set; } = 4;
    public bool IsGenderFemale { get; set; }
    public string Name { get; set; } = "Test";
    [BsonRef("Clans")] public Clan Clan { get; set; }
    public int FaceType { get; set; }
    public int HairStyle { get; set; }
    public int HairColor { get; set; }
    public int Tattoo { get; set; }
    public int BootModelId { get; set; }
    public int PantsModelId { get; set; }
    public int ArmorModelId { get; set; }
    public int HelmetModelId { get; set; }
    public int GlovesModelId { get; set; }
    public bool IsNotQueuedForDeletion { get; set; } = true;
    public int? HelmetSlot { get; set; } = null;
    public int? AmuletSlot { get; set; } = null;
    public int? SpecSlot { get; set; } = null;
    public int? ArmorSlot { get; set; } = null;
    public int? ShieldSlot { get; set; } = null;
    public int? BeltSlot { get; set; } = null;
    public int? GlovesSlot { get; set; } = null;
    public int? LeftBraceletSlot { get; set; } = null;
    public int? PantsSlot { get; set; } = null;
    public int? RightBraceletSlot { get; set; } = null;
    public int? TopLeftRingSlot { get; set; } = null;
    public int? TopRightRingSlot { get; set; } = null;
    public int? BottomLeftRingSlot { get; set; } = null;
    public int? BottomRightRingSlot { get; set; } = null;
    public int? BootsSlot { get; set; } = null;
    public int? LeftSpecialSlot1 { get; set; } = null;
    public int? LeftSpecialSlot2 { get; set; } = null;
    public int? LeftSpecialSlot3 { get; set; } = null;
    public int? LeftSpecialSlot4 { get; set; } = null;
    public int? LeftSpecialSlot5 { get; set; } = null; // spec ability 1
    public int? LeftSpecialSlot6 { get; set; } = null; // spec ability 2
    public int? LeftSpecialSlot7 { get; set; } = null; // spec ability 3
    public int? LeftSpecialSlot8 { get; set; } = null;
    public int? LeftSpecialSlot9 { get; set; } = null;
    public int? WeaponSlot { get; set; } = null;
    public int? AmmoSlot { get; set; } = null;
    public int? MapBookSlot { get; set; } = null;
    public int? RecipeBookSlot { get; set; } = null;
    public int? MantraBookSlot { get; set; } = null;
    public int? InkpotSlot { get; set; } = null;
    public int? IslandTokenSlot { get; set; } = null;
    public int? SpeedhackMantraSlot { get; set; } = null;
    public int? MoneySlot { get; set; } = null;
    public int? TravelbagSlot { get; set; } = null;
    public int? KeySlot1 { get; set; } = null;
    public int? KeySlot2 { get; set; } = null;
    public int? MissionSlot { get; set; } = null;
    public int? InventorySlot1 { get; set; } = null;
    public int? InventorySlot2 { get; set; } = null;
    public int? InventorySlot3 { get; set; } = null;
    public int? InventorySlot4 { get; set; } = null;
    public int? InventorySlot5 { get; set; } = null;
    public int? InventorySlot6 { get; set; } = null;
    public int? InventorySlot7 { get; set; } = null;
    public int? InventorySlot8 { get; set; } = null;
    public int? InventorySlot9 { get; set; } = null;
    public int? InventorySlot10 { get; set; } = null;
    public int Money { get; set; } = 0;
    public int SpecLevelMinusOne { get; set; } = 0;
    public SpecTypes SpecType { get; set; } = SpecTypes.None;
    public ClanRank ClanRank { get; set; } = 0;
}
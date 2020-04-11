namespace WCell.RealmServer.NPCs
{
    public enum TrainerSubType : byte
    {
        Weapons = 0,
        Fishing = 2,
        Herbalism = 4,
        Shaman = 4,
        Skinning = 4,
        Druid = 5,
        Priest = 5,
        Rogue = 5,
        Warlock = 5,
        Warrior = 5,
        Mage = 6,
        Paladin = 6,
        FirstAid = 7,
        Cooking = 10, // 0x0A
        Mining = 13, // 0x0D
        Alchemy = 44, // 0x2C
        Pet = 56, // 0x38
        Engineering = 68, // 0x44
        Enchanting = 72, // 0x48
        Leatherworking = 76, // 0x4C
        Blacksmithing = 78, // 0x4E
        Tailoring = 101, // 0x65
        Hunter = 144, // 0x90
        NotATrainer = 255, // 0xFF
    }
}
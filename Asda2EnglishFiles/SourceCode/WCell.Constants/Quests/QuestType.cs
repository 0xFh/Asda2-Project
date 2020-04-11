namespace WCell.Constants.Quests
{
    public enum QuestType : uint
    {
        Normal = 0,
        Elite = 1,
        Life = 21, // 0x00000015
        PvP = 41, // 0x00000029
        Raid = 62, // 0x0000003E
        Dungeon = 81, // 0x00000051
        WorldEvent = 82, // 0x00000052
        Legendary = 83, // 0x00000053
        Escort = 84, // 0x00000054
        Heroic = 85, // 0x00000055
    }
}
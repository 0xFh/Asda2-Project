namespace WCell.Constants.Quests
{
    public enum QuestFailedReason : byte
    {
        NoDetails = 0,
        InventoryFull = 4,
        DupeItemFound = 17, // 0x11
    }
}
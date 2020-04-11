namespace WCell.Constants.Quests
{
    public enum QuestStatus : byte
    {
        NotAvailable = 0,
        TooHighLevel = 1,
        Obsolete = 2,
        NotCompleted = 5,
        RepeateableCompletable = 6,
        Repeatable = 7,
        Available = 8,
        CompletableNoMinimap = 9,
        Completable = 10, // 0x0A
        Count = 11, // 0x0B
    }
}
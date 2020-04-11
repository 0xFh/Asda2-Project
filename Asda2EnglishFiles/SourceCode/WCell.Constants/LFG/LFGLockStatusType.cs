namespace WCell.Constants.LFG
{
    /// <summary>Reason players cant join</summary>
    public enum LFGLockStatusType
    {
        Ok = 0,
        InsufficientExpansion = 1,
        TooLowLevel = 2,
        TooHighLevel = 3,
        GearScoreTooLow = 4,
        GearScoreTooHigh = 5,
        RaidLocked = 6,
        AttonementTooLowLevel = 1001, // 0x000003E9
        AttonementTooHighLevel = 1002, // 0x000003EA
        QuestNotCompleted = 1022, // 0x000003FE
        MissingItem = 1025, // 0x00000401
        NotInSeason = 1031, // 0x00000407
    }
}
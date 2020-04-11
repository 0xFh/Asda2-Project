namespace WCell.Constants
{
    public enum BattlegroundJoinError
    {
        InNonRandomBg = -15, // -0x0000000F
        InRandomBg = -14, // -0x0000000E
        LfgCantUseBg = -13, // -0x0000000D
        JoinFailed = -12, // -0x0000000C
        JoinTimedOut = -11, // -0x0000000B
        JoinRangeIndex = -10, // -0x0000000A
        JoinXpGain = -9,
        GroupJoinedNotEligible = -8,
        TeamLeftQueue = -7,
        InRatedMatch = -6,
        StillEnqueued = -5,
        Max3Battles = -4,
        NotSameTeam = -3,
        Deserter = -2,
        Nothing = -1,
        None = 0,
    }
}
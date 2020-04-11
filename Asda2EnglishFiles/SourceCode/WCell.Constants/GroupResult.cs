namespace WCell.Constants
{
    /// <summary>Values sent in the SMSG_PARTY_COMMAND_RESULT packet</summary>
    public enum GroupResult
    {
        NoError = 0,
        OfflineOrDoesntExist = 1,
        NotInYourParty = 2,
        NotInYourInstance = 3,
        GroupIsFull = 4,
        AlreadyInGroup = 5,
        PlayerNotInParty = 6,
        DontHavePermission = 7,
        TargetIsUnfriendly = 8,
        TargetIsIgnoringYou = 9,
        LfgPending = 10, // 0x0000000A
        InviteRestricted = 11, // 0x0000000B
        GroupSwapFailed = 12, // 0x0000000C
        UnknownRealm = 13, // 0x0000000D
        RaidDisallowedByLevel = 25, // 0x00000019
    }
}
namespace WCell.Constants.LFG
{
    public enum LFGUpdateType
    {
        Default = 0,
        Leader = 1,
        RoleCheckAborted = 4,
        JoinProposal = 5,
        RoleCheckFailed = 6,
        RemovedFromQueue = 7,
        ProposalFailed = 8,
        ProposalDeclined = 9,
        GroupFound = 10, // 0x0000000A
        AddedToQueue = 12, // 0x0000000C
        ProposalBegin = 13, // 0x0000000D
        ClearLockList = 14, // 0x0000000E
        GroupMemberOffline = 15, // 0x0000000F
        GroupDisband = 16, // 0x00000010
    }
}
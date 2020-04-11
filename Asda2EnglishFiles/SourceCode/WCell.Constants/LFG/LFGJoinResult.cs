namespace WCell.Constants.LFG
{
    /// <summary>Queue join results</summary>
    public enum LFGJoinResult
    {
        Ok,
        Failed,
        GroupFull,
        Unknown,
        InternalError,
        DontMeetRequierments,
        PartyMemeberDoesntMeetRequirements,
        CannotMixRaidsAndDungeons,
        MultiRealmUnsupported,
        Disconnected,
        PartyInfoFailed,
        InvalidDungeon,
        Deserter,
        PartyMemberIsDeserter,
        RandomCooldown,
        PartyMemberIsRandomCooldown,
        TooManyMembers,
        CannotJoinWhileInBgOrArena,
        Failed2,
    }
}
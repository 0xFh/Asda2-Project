namespace WCell.RealmServer.Interaction
{
    /// <summary>
    /// TODO: Clean up
    /// TODO: Get rid of events and call methods directly to ensure this to be deterministic
    /// 
    /// Defines all kinds of Relations between Characters, mainly Friends, Ignores, Mutings, Invitations to Groups and Guilds etc.
    /// Characters can have active Relations (which were triggered by the Character himself) and
    /// passive Relations (which are Relations that others established with the corresponding Character).
    /// </summary>
    public enum CharacterRelationType
    {
        Invalid,
        Friend,
        Ignored,
        Muted,
        GroupInvite,
        GuildInvite,
        Count,
    }
}
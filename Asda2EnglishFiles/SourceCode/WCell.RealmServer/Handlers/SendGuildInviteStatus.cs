namespace WCell.RealmServer.Handlers
{
    public enum SendGuildInviteStatus
    {
        CannotJoinTheGuild = 0,
        Ok = 1,
        AProblemWasFoundedInUserProfile = 2,
        AProblemWithGuildInformation = 4,
        YouCantInviteMembersOfAnotherGuild = 6,
        GuildInvitionHasBeenRefused = 7,
        YouMustInviteCharacterFromSameFactionAsYours = 8,
        YourGuildRosterIsFullYouCannotAddMoreMembers = 10, // 0x0000000A
        YouDontHavePermitionsToUseThis = 11, // 0x0000000B
    }
}
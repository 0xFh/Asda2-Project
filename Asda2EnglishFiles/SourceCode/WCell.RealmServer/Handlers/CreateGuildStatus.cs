namespace WCell.RealmServer.Handlers
{
    public enum CreateGuildStatus
    {
        YouMustBeLevel10And1StJobToCreateClan = 0,
        Ok = 1,
        YouMustCompleteGuildRightsQuestToCreateGuild = 2,
        NotEnoghtMoney = 4,
        YouAreInAnotherGuild = 5,
        YouCantCreateGuildWith = 7,
        YouCantCreateAnotherGuild = 8,
        GuildNameAlreadyExist = 9,
        YouHaveChoosedTheInvalidGuildName = 10, // 0x0000000A
    }
}
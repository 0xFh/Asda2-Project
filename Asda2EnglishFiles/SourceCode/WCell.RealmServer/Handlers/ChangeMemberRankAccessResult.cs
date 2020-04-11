namespace WCell.RealmServer.Handlers
{
    public enum ChangeMemberRankAccessResult
    {
        FailedToChangeGuildRank = 0,
        Ok = 1,
        ThereIsAnErrorInUserProfile = 1,
        YouAreNotInGuild = 3,
        ThereIsAProblemWithGuildInformation = 4,
        CantChangeTheGuildLeaderPrivilegies = 5,
        YouDontHavePermitionToUseThis = 6,
        YouCantChangeThisRank = 7,
        YouCantAddMoreViceGuildLeaders = 8,
        YouCantMakeChangesToAUserOfHigherRankThanYou = 9,
    }
}
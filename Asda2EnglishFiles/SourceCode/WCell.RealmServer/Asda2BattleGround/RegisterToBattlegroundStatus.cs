namespace WCell.RealmServer.Asda2BattleGround
{
    public enum RegisterToBattlegroundStatus
    {
        Fail = 0,
        Ok = 1,
        YouRegisterAsFactionWarCandidat = 2,
        YouMustCHangeYourJobTwiceToEnterWar = 3,
        BattleGroupInfoIsInvalid = 4,
        YouHaveAlreadyRegistered = 5,
        YouCanJoinTheFActionWarOnlyOncePerDay = 6,
        GamesInfoStrange = 8,
        YouCantEnterCauseYouHaveBeenDissmised = 9,
        WrongLevel = 10, // 0x0000000A
        WarHasBeenCanceledCauseLowPlayers = 11, // 0x0000000B
    }
}
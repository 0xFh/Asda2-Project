namespace WCell.RealmServer.Asda2BattleGround
{
    public enum BattleGroundInfoMessageType
    {
        FailedToTemporarilyOccuptyTheNumOccupationPoints,
        SuccessToTemporarilyOccuptyTheNumOccupationPoints,
        CanceledToTemporarilyOccuptyTheNumOccupationPoints,
        FailedToCompletelyOccuptyTheNumOccupationPoints,
        SuccessToCompletelyOccuptyTheNumOccupationPoints,
        CanceledToCompletelyOccuptyTheNumOccupationPoints,
        TheOtherSideHasTemporarilyOccupiedTheNumOccupationPoint,
        WarStartsInNumMins,
        WarStarted,
        WarEndsInNumMins,
        PreWarCircle,
        DarkWillReciveBuffs,
        DarkBuffsHasBeedRemoved,
    }
}
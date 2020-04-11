namespace WCell.RealmServer.Handlers
{
    internal enum Asda2StartMovementStatus
    {
        Ok = 1,
        UnavalibleArea = 2,
        CantMoveSoFar = 3,
        InstantTeleport = 5,
        CantMoveInThisCondition = 6,
        WeightLimitHiger90YouCantFigth = 7,
        CantMoveBeforeWarStarted = 8,
        YouCantMoveToOtherSideOfRevivalArea = 9,
    }
}
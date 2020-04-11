namespace WCell.RealmServer.Handlers
{
    public enum Asda2CharacterAtackStatus
    {
        Fail = 0,
        Inv90Proc = 2,
        BallistaCharged = 3,
        Fail2 = 4,
        YouCannotTargetThisObject = 5,
        YouCannotAtackThisMosterAtThisTime = 6,
        DontHaveEnoughArrows = 7,
        YouOnVehicle = 8,
        YouCannotAtackFromHere = 9,
        YouCannotAtackWithFishingRod = 11, // 0x0000000B
        YouCannotAtackWithDrill = 12, // 0x0000000C
        CharOnRevialZone = 13, // 0x0000000D
    }
}
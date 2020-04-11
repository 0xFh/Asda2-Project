namespace WCell.RealmServer.Asda2_Items
{
    public enum UseItemResult
    {
        Fail = 0,
        Ok = 1,
        ItemOnCooldown = 3,
        YouCantUseThePotion = 4,
        YouCantUseThisItemInTown = 5,
        YouMustUseSoulStoneWithASoulMate = 6,
        CantUseSoulStoneSinceYouAreNotInteEntityPosition = 7,
        ThereIsAlreadyAUsedSoulStone = 8,
        CantUseBacauseOfItemLevel = 9,
        CantUseDigingItemRightNow = 10, // 0x0000000A
        ItemsCantBeUsedUnderAbnormalStatus = 12, // 0x0000000C
        ThereIsNoActivePet = 13, // 0x0000000D
        PetIsMature = 14, // 0x0000000E
        YourLevelIsToLowAndYouCantDig = 15, // 0x0000000F
    }
}
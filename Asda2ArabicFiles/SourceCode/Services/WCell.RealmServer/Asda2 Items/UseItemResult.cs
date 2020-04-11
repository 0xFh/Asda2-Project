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
        CantUseDigingItemRightNow = 10,
        ItemsCantBeUsedUnderAbnormalStatus = 12,
        ThereIsNoActivePet = 13,
        PetIsMature = 14,
        YourLevelIsToLowAndYouCantDig = 15,

    }
}
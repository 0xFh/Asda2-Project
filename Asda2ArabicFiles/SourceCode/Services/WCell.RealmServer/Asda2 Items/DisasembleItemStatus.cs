namespace WCell.RealmServer.Asda2_Items
{
    public enum DisasembleItemStatus
    {
        Ok = 1,
        NoMoney = 2,
        NoEmptySlotInThePlate = 3,
        NoWeightValue = 4,
        LackOfMaterialForCraft = 5,
        NoMaterialForCraft = 6,
        CraftingLevelIsNotMatch = 7,
        FailedByCraftigSuccessProbability = 8,
        CraftingInfoIsInaccurate = 9
    }
}
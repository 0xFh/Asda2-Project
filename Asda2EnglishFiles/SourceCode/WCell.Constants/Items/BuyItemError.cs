namespace WCell.Constants.Items
{
    public enum BuyItemError : byte
    {
        CantFindItem = 0,
        ItemAlreadySold = 1,
        NotEnoughMoney = 2,
        Unknown1 = 3,
        SellerDoesntLikeYou = 4,
        DistanceTooFar = 5,
        Unknown2 = 6,
        ItemSoldOut = 7,
        CantCarryAnymore = 8,
        Unknown3 = 9,
        Unknown4 = 16, // 0x10
        RankRequirementNotMet = 17, // 0x11
        ReputationRequirementNotMet = 18, // 0x12
        Ok = 255, // 0xFF
    }
}
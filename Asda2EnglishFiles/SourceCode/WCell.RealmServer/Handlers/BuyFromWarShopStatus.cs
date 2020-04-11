namespace WCell.RealmServer.Handlers
{
    public enum BuyFromWarShopStatus
    {
        Fail,
        Ok,
        InventoryIsFull,
        InvalidWeight,
        NonEnoghtGold,
        NotEnoghtExchangeItems,
        NonEnoghtHonorRanks,
        CantFoundItem,
        AvalibleOnlyToWiningFaction,
        UnableToPurshace,
    }
}
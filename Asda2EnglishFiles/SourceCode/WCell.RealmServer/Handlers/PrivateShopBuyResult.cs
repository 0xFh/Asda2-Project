namespace WCell.RealmServer.Handlers
{
    public enum PrivateShopBuyResult
    {
        Error,
        Ok,
        SelectedItemsIsNotAvailable,
        UserClosedTheWindow,
        WeightValueExceedsTheLimit,
        NoSlotAreAvailable,
        NotEnoghtGold,
        RequestedNumberOfItemsIsNoLongerAvaliable,
        AnotherPlayerHasAlreadyPurchasedTheItemPleasyTryAgain,
        OnlyThoseOver24LevelAreAlowedToExchangePurchasedItems,
    }
}
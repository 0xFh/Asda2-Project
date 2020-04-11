namespace WCell.RealmServer.Handlers
{
    public enum PrivateShopOpenedResult
    {
        Error,
        Ok,
        ThereIsNoItemInfo,
        ItemIsAlreadyInPlace,
        ThisItemIsUnexchangeable,
        ThisItemCantBeTradedDueToTheequippedSowel,
        YouOwnMoreItemsThanAllowed,
        YouCantTradeTheGold,
        YouMastBeMoreThan24Level,
    }
}
namespace WCell.RealmServer.Handlers
{
    public enum Asda2PushItemToTradeStatus
    {
        AnErrorWasFoundWithTransferedItem,
        Ok,
        ThereIsNoInformationOfTheTransferedItem,
        TheItemRemainsInThePlaceWhereIMovedTo,
        ItemCantBeTraded,
        CantTradeInTheCurrentTransactionWindow,
        ItemsFromShowelCantBeTrated,
        YouMustHaveAtLeastOneGold,
        YouExchangeApurchasedItem,
        UnexchanableItem,
        YouMustHave24Lvl,
    }
}
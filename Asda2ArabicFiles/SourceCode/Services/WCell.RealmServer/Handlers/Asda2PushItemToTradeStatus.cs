namespace WCell.RealmServer.Handlers
{
    public enum Asda2PushItemToTradeStatus
    {
        AnErrorWasFoundWithTransferedItem = 0,
        Ok = 1,
        ThereIsNoInformationOfTheTransferedItem = 2,
        TheItemRemainsInThePlaceWhereIMovedTo = 3,
        ItemCantBeTraded = 4,
        CantTradeInTheCurrentTransactionWindow = 5,
        ItemsFromShowelCantBeTrated = 6,
        YouMustHaveAtLeastOneGold = 7,
        YouExchangeApurchasedItem = 8,
        UnexchanableItem = 9,
        YouMustHave24Lvl = 10,
    }
}
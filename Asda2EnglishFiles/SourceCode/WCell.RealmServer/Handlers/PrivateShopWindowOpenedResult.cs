namespace WCell.RealmServer.Handlers
{
    public enum PrivateShopWindowOpenedResult
    {
        Fail,
        Ok,
        YouAreInYourShop,
        YouAreInWar,
        YouAreDead,
        YourLevelMustBeHigherThanTen,
        NoInfoAbountFunctionItem,
        CantOpenPrivateShopInsidePvpZones,
    }
}
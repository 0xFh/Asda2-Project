namespace WCell.RealmServer.Handlers
{
    internal enum PushItemToWhStatus
    {
        CantFindItem,
        Ok,
        NotEnoughtSlots,
        OnlyForSoulmate,
        ItemNotFounded,
        NotEnoughtSlotsInWh,
        NotEnoughtGold,
        NoWeightLimit,
        AlreadyUsingWh,
    }
}
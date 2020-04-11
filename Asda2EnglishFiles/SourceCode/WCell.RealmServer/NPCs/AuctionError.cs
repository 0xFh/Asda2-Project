namespace WCell.RealmServer.NPCs
{
    public enum AuctionError : uint
    {
        Ok = 0,
        InternalError = 2,
        NotEnoughMoney = 3,
        ItemNotFound = 4,
        CannotBidOnOwnAuction = 10, // 0x0000000A
    }
}
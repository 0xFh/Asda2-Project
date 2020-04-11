namespace WCell.RealmServer.NPCs
{
    public enum TaxiActivateResponse : uint
    {
        Ok,
        InvalidChoice,
        NotAvailable,
        InsufficientFunds,
        NoPathNearby,
        NoVendorNearby,
        NodeNotActive,
        PlayerBusy,
        PlayerAlreadyMounted,
        PlayerShapeShifted,
        PlayerMoving,
        SameNode,
        PlayerNotStanding,
    }
}
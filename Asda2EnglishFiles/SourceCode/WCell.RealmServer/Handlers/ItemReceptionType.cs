namespace WCell.RealmServer.Handlers
{
    /// <summary>Send in the packet that logs new items</summary>
    public enum ItemReceptionType : ulong
    {
        Loot = 0,
        Receive = 1,
        YouCreated = 4294967296, // 0x0000000100000000
    }
}
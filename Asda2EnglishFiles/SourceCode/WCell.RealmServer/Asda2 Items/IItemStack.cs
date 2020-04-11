namespace WCell.RealmServer.Items
{
    public interface IItemStack
    {
        int Amount { get; }

        ItemTemplate Template { get; }
    }
}
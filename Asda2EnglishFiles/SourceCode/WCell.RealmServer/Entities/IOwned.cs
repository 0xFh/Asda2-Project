namespace WCell.RealmServer.Entities
{
    /// <summary>Anything that is usable: Items and GameObjects so far</summary>
    public interface IOwned
    {
        Unit Owner { get; }
    }
}
using WCell.Core;

namespace WCell.RealmServer.Entities
{
    /// <summary>Defines an in-game entity.</summary>
    public interface IEntity
    {
        /// <summary>The EntityId</summary>
        EntityId EntityId { get; }
    }
}
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Entities
{
    /// <summary>Defines an entity that can recieve packets.</summary>
    public interface IPacketReceivingEntity : IEntity, IPacketReceiver
    {
    }
}
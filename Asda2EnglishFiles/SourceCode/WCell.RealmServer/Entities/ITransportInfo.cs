using WCell.Core.Paths;
using WCell.RealmServer.Factions;
using WCell.Util;
using WCell.Util.Threading;

namespace WCell.RealmServer.Entities
{
    public interface ITransportInfo : IFactionMember, IWorldLocation, IHasPosition, INamedEntity, IEntity, INamed,
        IContextHandler
    {
        float Orientation { get; }
    }
}
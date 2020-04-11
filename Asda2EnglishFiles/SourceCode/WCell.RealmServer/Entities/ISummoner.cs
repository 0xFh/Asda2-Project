using WCell.Core.Paths;
using WCell.RealmServer.Global;
using WCell.Util;

namespace WCell.RealmServer.Entities
{
    /// <summary>A Summoner has an Id, a Name and a WorldZonePosition</summary>
    public interface ISummoner : INamedEntity, IEntity, INamed, IWorldZoneLocation, IWorldLocation, IHasPosition
    {
        Zone Zone { get; }
    }
}
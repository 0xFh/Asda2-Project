using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.RealmServer.Global;
using WCell.Util.Data;

namespace WCell.RealmServer.Entities
{
    public interface IWorldZoneLocation : IWorldLocation, IHasPosition
    {
        ZoneId ZoneId { get; }

        [NotPersistent] ZoneTemplate ZoneTemplate { get; }
    }
}
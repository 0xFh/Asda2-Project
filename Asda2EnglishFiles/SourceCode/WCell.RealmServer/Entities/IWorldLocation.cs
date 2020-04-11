using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.RealmServer.Global;
using WCell.Util.Data;

namespace WCell.RealmServer.Entities
{
    public interface IWorldLocation : IHasPosition
    {
        MapId MapId { get; }

        [NotPersistent] Map Map { get; }

        [NotPersistent] uint Phase { get; }
    }
}
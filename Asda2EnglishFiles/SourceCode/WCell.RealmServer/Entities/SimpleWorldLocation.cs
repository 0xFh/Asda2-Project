using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
    public class SimpleWorldLocation : IWorldLocation, IHasPosition
    {
        public SimpleWorldLocation(MapId map, Vector3 pos, uint phase = 1)
        {
            this.Position = pos;
            this.MapId = map;
            this.Phase = phase;
        }

        public Vector3 Position { get; set; }

        public MapId MapId { get; set; }

        public Map Map
        {
            get { return WCell.RealmServer.Global.World.GetNonInstancedMap(this.MapId); }
        }

        public uint Phase { get; set; }
    }
}
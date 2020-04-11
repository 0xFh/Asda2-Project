using System;
using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
    public class WorldLocation : IWorldLocation, IHasPosition
    {
        public WorldLocation(MapId map, Vector3 pos, uint phase = 1)
        {
            this.Position = pos;
            this.Map = WCell.RealmServer.Global.World.GetNonInstancedMap(map);
            if (this.Map == null)
                throw new ArgumentException(nameof(map), "Invalid Map in WorldLocation: " + (object) map);
            this.Phase = phase;
        }

        public WorldLocation(Map map, Vector3 pos, uint phase = 1)
        {
            this.Position = pos;
            this.Map = map;
            this.Phase = phase;
        }

        public Vector3 Position { get; set; }

        public MapId MapId
        {
            get { return this.Map.Id; }
        }

        public Map Map { get; set; }

        public uint Phase { get; set; }
    }
}
using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
    public class WorldZoneLocation : WorldLocation, IWorldZoneLocation, IWorldLocation, IHasPosition
    {
        public WorldZoneLocation(MapId map, Vector3 pos, ZoneTemplate zone)
            : base(map, pos, 1U)
        {
            this.ZoneTemplate = zone;
        }

        public WorldZoneLocation(Map map, Vector3 pos, ZoneTemplate zone)
            : base(map, pos, 1U)
        {
            this.ZoneTemplate = zone;
        }

        public WorldZoneLocation(IWorldZoneLocation location)
            : base(location.Map, location.Position, 1U)
        {
            this.ZoneTemplate = location.ZoneTemplate;
        }

        public WorldZoneLocation(MapId map, Vector3 pos, ZoneId zone)
            : base(map, pos, 1U)
        {
            if (this.Map == null)
                return;
            this.ZoneTemplate = WCell.RealmServer.Global.World.GetZoneInfo(zone);
        }

        public ZoneId ZoneId
        {
            get
            {
                if (this.ZoneTemplate == null)
                    return ZoneId.None;
                return this.ZoneTemplate.Id;
            }
        }

        public ZoneTemplate ZoneTemplate { get; set; }
    }
}
using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Misc
{
    /// <summary>
    /// Holds an exact World-Location, and also the Zone of the Location
    /// </summary>
    public struct NamedWorldZoneLocation : IWorldZoneLocation, IWorldLocation, IHasPosition
    {
        public static readonly NamedWorldZoneLocation Zero = new NamedWorldZoneLocation();
        public string Name;

        public Vector3 Position { get; set; }

        public MapId MapId { get; set; }

        public Map Map
        {
            get { return WCell.RealmServer.Global.World.GetNonInstancedMap(this.MapId); }
        }

        public uint Phase
        {
            get { return 1; }
        }

        public ZoneId ZoneId { get; set; }

        public ZoneTemplate ZoneTemplate
        {
            get { return WCell.RealmServer.Global.World.GetZoneInfo(this.ZoneId); }
        }

        public static bool operator ==(NamedWorldZoneLocation left, NamedWorldZoneLocation right)
        {
            if (left.Position == right.Position)
                return left.MapId == right.MapId;
            return false;
        }

        public static bool operator !=(NamedWorldZoneLocation left, NamedWorldZoneLocation right)
        {
            return !(left == right);
        }

        public bool IsValid
        {
            get { return this.MapId != MapId.End; }
        }

        public override bool Equals(object obj)
        {
            if (obj is NamedWorldZoneLocation)
                return this == (NamedWorldZoneLocation) obj;
            return false;
        }

        public override int GetHashCode()
        {
            return (int) ((double) this.MapId *
                          ((double) this.Position.X * (double) this.Position.Y * (double) this.Position.Z));
        }
    }
}
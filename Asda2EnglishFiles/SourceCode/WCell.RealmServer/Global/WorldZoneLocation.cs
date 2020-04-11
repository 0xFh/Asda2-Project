using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Lang;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Global
{
    /// <summary>
    /// World locations are specific game locations that can be accessed with teleport command.
    /// </summary>
    public class WorldZoneLocation : IDataHolder, INamedWorldZoneLocation, IWorldZoneLocation, IWorldLocation,
        IHasPosition
    {
        private string[] m_Names = new string[8];
        public uint Id;
        private ZoneTemplate m_ZoneTemplate;
        private ZoneId m_ZoneId;

        [NotPersistent]
        public string[] Names
        {
            get { return this.m_Names; }
            set { this.m_Names = value; }
        }

        public uint Phase
        {
            get { return 1; }
        }

        public WorldZoneLocation()
        {
        }

        public WorldZoneLocation(string name, MapId mapId, Vector3 pos)
            : this(mapId, pos)
        {
            this.DefaultName = name;
        }

        public WorldZoneLocation(string[] localizedNames, MapId mapId, Vector3 pos)
            : this(mapId, pos)
        {
            this.Names = localizedNames;
        }

        private WorldZoneLocation(MapId mapId, Vector3 pos)
        {
            this.MapId = mapId;
            this.Position = pos;
        }

        public string DefaultName
        {
            get { return this.Names.LocalizeWithDefaultLocale(); }
            set { this.Names[(int) RealmServerConfiguration.DefaultLocale] = value; }
        }

        public string EnglishName
        {
            get { return this.Names.LocalizeWithDefaultLocale(); }
            set { this.Names[0] = value; }
        }

        public MapId MapId { get; set; }

        public Map Map
        {
            get { return WCell.RealmServer.Global.World.GetNonInstancedMap(this.MapId); }
        }

        public Vector3 Position { get; set; }

        public ZoneId ZoneId
        {
            get { return this.m_ZoneId; }
            set { this.m_ZoneId = value; }
        }

        /// <summary>The Zone to which this Location belongs (if any)</summary>
        [NotPersistent]
        public ZoneTemplate ZoneTemplate
        {
            get { return this.m_ZoneTemplate; }
            set
            {
                this.m_ZoneTemplate = value;
                this.ZoneId = this.m_ZoneTemplate != null ? this.m_ZoneTemplate.Id : ZoneId.None;
            }
        }

        public uint GetId()
        {
            return this.Id;
        }

        public DataHolderState DataHolderState { get; set; }

        public void FinalizeDataHolder()
        {
            WorldLocationMgr.WorldLocations[this.DefaultName] = (INamedWorldZoneLocation) this;
            ZoneTemplate zoneInfo = WCell.RealmServer.Global.World.GetZoneInfo(this.ZoneId);
            if (zoneInfo == null)
                return;
            if (zoneInfo.Site is WorldZoneLocation)
                ((WorldZoneLocation) zoneInfo.Site).ZoneTemplate = (ZoneTemplate) null;
            else if (zoneInfo.Site != null)
                return;
            zoneInfo.Site = (IWorldLocation) this;
            this.ZoneTemplate = zoneInfo;
        }

        public override bool Equals(object obj)
        {
            if (obj is WorldZoneLocation && (double) ((WorldZoneLocation) obj).Position.X == (double) this.Position.X &&
                ((double) ((WorldZoneLocation) obj).Position.Y == (double) this.Position.Y &&
                 (double) ((WorldZoneLocation) obj).Position.Z == (double) this.Position.Z))
                return ((WorldZoneLocation) obj).MapId == this.MapId;
            return false;
        }

        public override int GetHashCode()
        {
            return (int) ((double) this.MapId *
                          ((double) this.Position.X * (double) this.Position.Y * (double) this.Position.Z));
        }

        /// <summary>
        /// Overload the ToString method to return a formated text with world location name and id
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} (Id: {1})", (object) this.DefaultName, (object) this.Id);
        }
    }
}
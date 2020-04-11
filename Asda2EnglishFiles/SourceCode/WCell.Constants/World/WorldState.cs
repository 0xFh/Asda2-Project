namespace WCell.Constants.World
{
    public class WorldState
    {
        public static readonly WorldState[] EmptyArray = new WorldState[0];
        public readonly MapId MapId = MapId.End;
        public readonly ZoneId ZoneId;
        public readonly WorldStateId Key;
        public int DefaultValue;

        public uint Index { get; internal set; }

        public WorldState(WorldStateId key, int value)
            : this(MapId.End, key, value)
        {
        }

        public WorldState(MapId mapId, WorldStateId key, int value)
            : this(mapId, ZoneId.None, key, value)
        {
        }

        public WorldState(MapId mapId, ZoneId zoneId, WorldStateId key, int value)
        {
            this.MapId = mapId;
            this.ZoneId = zoneId;
            this.Key = key;
            this.DefaultValue = value;
        }

        public WorldState(uint key, int value)
            : this(MapId.End, key, value)
        {
        }

        public WorldState(MapId mapId, uint key, int value)
            : this(mapId, ZoneId.None, key, value)
        {
        }

        public WorldState(MapId mapId, ZoneId zoneId, uint key, int value)
        {
            this.MapId = mapId;
            this.ZoneId = zoneId;
            this.Key = (WorldStateId) key;
            this.DefaultValue = value;
        }

        public bool IsGlobal
        {
            get { return this.MapId == MapId.End; }
        }

        public bool IsMapal
        {
            get { return this.ZoneId == ZoneId.None; }
        }
    }
}
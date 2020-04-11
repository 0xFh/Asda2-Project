using WCell.Constants;

namespace WCell.Core.DBC
{
    /// <summary>Represents an entry in Map.dbc</summary>
    public class MapInfo
    {
        public uint Id;
        public string InternalName;
        public MapType MapType;
        public bool HasTwoSides;
        public string Name;
        public uint MinimumLevel;
        public uint MaximumLevel;
        public uint MaximumPlayers;
        public int Field_24;
        public float Field_25;
        public float Field_26;
        public uint AreaTableId;
        public string HordeText;
        public string AllianceText;
        public int LoadingScreen;
        public int BattlegroundLevelIncrement;
        public int Field_64;
        public float Field_65;
        public string HeroicDescription;
        public int ParentMap;
        public float Field_118;
        public float Field_119;

        /// <summary>In seconds</summary>
        public uint RaidResetTimer;

        /// <summary>In seconds</summary>
        public uint HeroicResetTimer;
    }
}
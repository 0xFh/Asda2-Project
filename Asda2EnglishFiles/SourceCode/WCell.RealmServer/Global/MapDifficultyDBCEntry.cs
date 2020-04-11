using WCell.Constants.World;

namespace WCell.RealmServer.Global
{
    public class MapDifficultyDBCEntry
    {
        public uint Id;
        public MapId MapId;
        public uint Index;

        /// <summary>
        /// You must have level...
        /// You must have Key of...
        /// Heroid Difficulty requires completion of...
        /// You must complete the quest...
        /// </summary>
        public string RequirementString;

        /// <summary>
        /// Automatic reset-time in seconds.
        /// 0 for non-raid Dungeons
        /// </summary>
        public int ResetTime;

        /// <summary>Might be 0 (have to use MapInfo.MaxPlayerCount)</summary>
        public int MaxPlayerCount;
    }
}
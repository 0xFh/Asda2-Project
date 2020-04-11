using System;
using WCell.RealmServer.Instances;

namespace WCell.RealmServer.Global
{
    [Serializable]
    public class MapDifficultyEntry : MapDifficultyDBCEntry
    {
        public static readonly int HeroicResetTime = 86400;
        public static readonly int MaxDungeonPlayerCount = 5;
        public MapTemplate Map;
        public bool IsHeroic;
        public bool IsRaid;

        /// <summary>
        /// Softly bound instances can always be reset but you only x times per hour.
        /// </summary>
        public BindingType BindingType;

        internal void Finalize(MapTemplate map)
        {
            this.Map = map;
            if (this.ResetTime == 0)
                this.ResetTime = map.DefaultResetTime;
            this.IsHeroic = this.ResetTime == MapDifficultyEntry.HeroicResetTime;
            this.IsRaid = this.MaxPlayerCount == MapDifficultyEntry.MaxDungeonPlayerCount;
            this.BindingType = this.IsDungeon ? BindingType.Soft : BindingType.Hard;
        }

        public bool IsDungeon
        {
            get
            {
                if (!this.IsHeroic)
                    return !this.IsRaid;
                return false;
            }
        }
    }
}
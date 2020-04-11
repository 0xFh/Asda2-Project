using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.World;
using WCell.RealmServer.Lang;

namespace WCell.RealmServer.Achievements
{
    public class AchievementEntry
    {
        /// <summary>
        /// List of criteria that needs to be satisfied to achieve this achievement
        /// </summary>
        public readonly List<AchievementCriteriaEntry> Criteria = new List<AchievementCriteriaEntry>();

        /// <summary>
        /// List of all rewards that will be given once achievement is completed.
        /// </summary>
        public readonly List<AchievementReward> Rewards = new List<AchievementReward>();

        public uint ID;
        public int FactionFlag;
        public MapId MapID;
        public string[] Names;
        public AchievementCategoryEntry Category;
        public uint Points;
        public AchievementFlags Flags;
        public uint Count;
        public uint RefAchievement;

        public bool IsRealmFirstType()
        {
            return this.Flags.HasAnyFlag(AchievementFlags.RealmFirstReach | AchievementFlags.RealmFirstKill);
        }

        public override string ToString()
        {
            return this.Names.LocalizeWithDefaultLocale();
        }
    }
}
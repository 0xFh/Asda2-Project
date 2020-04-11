using System.Runtime.InteropServices;
using WCell.Constants.Achievements;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class ReachLevelAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public uint Unused;
        public uint Level;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= this.Level;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (this.AchievementEntry.IsRealmFirstType() &&
                (int) AchievementCollection.ClassSpecificAchievementId[(int) achievements.Owner.Class] !=
                (int) this.AchievementEntryId &&
                (int) AchievementCollection.RaceSpecificAchievementId[(int) achievements.Owner.Race] !=
                (int) this.AchievementEntryId)
                return;
            achievements.SetCriteriaProgress((AchievementCriteriaEntry) this, value1, ProgressType.ProgressSet);
        }
    }
}
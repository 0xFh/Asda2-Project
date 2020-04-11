using System.Runtime.InteropServices;
using WCell.Constants.Achievements;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class GainExaltedReputationAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public uint Unused;
        public uint NumberOfExaltedReputations;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= this.NumberOfExaltedReputations;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            achievements.SetCriteriaProgress((AchievementCriteriaEntry) this,
                achievements.Owner.Reputations.GetExaltedReputations(), ProgressType.ProgressHighest);
        }
    }
}
using System.Runtime.InteropServices;
using WCell.Constants.Achievements;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class GainReveredReputationAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public uint Unused;
        public uint Unused2;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= 1U;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            achievements.SetCriteriaProgress((AchievementCriteriaEntry) this,
                achievements.Owner.Reputations.GetReveredReputations(), ProgressType.ProgressHighest);
        }
    }
}
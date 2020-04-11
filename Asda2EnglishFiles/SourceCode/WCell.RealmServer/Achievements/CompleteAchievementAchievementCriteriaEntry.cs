using System.Runtime.InteropServices;
using WCell.Constants.Achievements;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class CompleteAchievementAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public uint AchievementToCompleteId;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if ((int) this.AchievementToCompleteId != (int) value1)
                return;
            achievements.SetCriteriaProgress((AchievementCriteriaEntry) this, value2, ProgressType.ProgressSet);
        }

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= 1U;
        }
    }
}
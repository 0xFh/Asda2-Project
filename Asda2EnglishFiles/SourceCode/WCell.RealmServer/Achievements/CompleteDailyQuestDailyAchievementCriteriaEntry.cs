using System.Runtime.InteropServices;
using WCell.Constants.Achievements;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class CompleteDailyQuestDailyAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public uint Unused;
        public uint NumberOfDays;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 == 0U)
                return;
            achievements.SetCriteriaProgress((AchievementCriteriaEntry) this, value1, ProgressType.ProgressAccumulate);
        }

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= this.NumberOfDays;
        }
    }
}
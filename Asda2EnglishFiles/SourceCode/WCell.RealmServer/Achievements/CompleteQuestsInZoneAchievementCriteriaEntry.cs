using System.Runtime.InteropServices;
using WCell.Constants.Achievements;
using WCell.Constants.World;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class CompleteQuestsInZoneAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public ZoneId ZoneId;
        public uint CompletedQuestCount;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= this.CompletedQuestCount;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (this.ZoneId != (ZoneId) value1)
                return;
            achievements.SetCriteriaProgress((AchievementCriteriaEntry) this, value2, ProgressType.ProgressSet);
        }
    }
}
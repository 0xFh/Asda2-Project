using System.Runtime.InteropServices;
using WCell.Constants.Achievements;
using WCell.Constants.Skills;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class ReachSkillLevelAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public SkillId SkillId;
        public uint SkillValue;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 == 0U || (SkillId) value1 != this.SkillId)
                return;
            achievements.SetCriteriaProgress((AchievementCriteriaEntry) this, value2, ProgressType.ProgressSet);
        }

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= this.SkillValue;
        }
    }
}
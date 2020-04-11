using System.Runtime.InteropServices;
using WCell.Constants.Achievements;
using WCell.Constants.NPCs;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class KillCreatureAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public NPCId CreatureId;
        public int CreatureCount;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return (long) achievementProgressRecord.Counter >= (long) this.CreatureCount;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (this.CreatureId != (NPCId) value1)
                return;
            achievements.SetCriteriaProgress((AchievementCriteriaEntry) this, value2, ProgressType.ProgressAccumulate);
        }
    }
}
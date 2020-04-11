using System.Runtime.InteropServices;
using WCell.Constants.Achievements;
using WCell.Constants.NPCs;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class KilledByCreatureAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public NPCId CreatureId;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 == 0U || (NPCId) value1 != this.CreatureId)
                return;
            achievements.SetCriteriaProgress((AchievementCriteriaEntry) this, 1U, ProgressType.ProgressAccumulate);
        }
    }
}
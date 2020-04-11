using System.Runtime.InteropServices;
using WCell.Constants.Achievements;
using WCell.Constants.Factions;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class KilledByPlayerAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 == 0U || achievements.Owner.FactionGroup == (FactionGroup) value1)
                return;
            achievements.SetCriteriaProgress((AchievementCriteriaEntry) this, 1U, ProgressType.ProgressAccumulate);
        }
    }
}
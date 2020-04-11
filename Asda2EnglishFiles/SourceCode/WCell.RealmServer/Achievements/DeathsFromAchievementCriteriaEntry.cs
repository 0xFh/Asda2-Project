using System.Runtime.InteropServices;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class DeathsFromAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public EnviromentalDamageType EnviromentalDamageType;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 == 0U || (EnviromentalDamageType) value1 != this.EnviromentalDamageType)
                return;
            achievements.SetCriteriaProgress((AchievementCriteriaEntry) this, value2, ProgressType.ProgressAccumulate);
        }
    }
}
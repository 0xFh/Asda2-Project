using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Achievements
{
    public class AchievementCriteriaRequirementHoliday : AchievementCriteriaRequirement
    {
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            return WorldEventMgr.IsHolidayActive(this.Value1);
        }
    }
}
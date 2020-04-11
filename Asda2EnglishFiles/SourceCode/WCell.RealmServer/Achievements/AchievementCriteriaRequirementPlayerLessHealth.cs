using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    public class AchievementCriteriaRequirementPlayerLessHealth : AchievementCriteriaRequirement
    {
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            if (target == null || !(target is Character))
                return false;
            return (long) target.HealthPct == (long) this.Value1;
        }
    }
}
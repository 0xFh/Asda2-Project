using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    public class AchievementCriteriaRequirementGender : AchievementCriteriaRequirement
    {
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            if (target == null)
                return false;
            return target.Gender == (GenderType) this.Value1;
        }
    }
}
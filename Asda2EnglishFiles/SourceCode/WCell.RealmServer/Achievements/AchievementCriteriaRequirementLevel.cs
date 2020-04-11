using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    public class AchievementCriteriaRequirementLevel : AchievementCriteriaRequirement
    {
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            if (target == null)
                return false;
            Character character = target as Character;
            if (character != null && character.Class == ClassId.Balista && this.Value1 < 55U)
                return false;
            return (long) target.Level >= (long) this.Value1;
        }
    }
}
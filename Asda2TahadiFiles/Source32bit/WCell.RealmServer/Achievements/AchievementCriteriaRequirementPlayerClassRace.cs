using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    public class AchievementCriteriaRequirementPlayerClassRace : AchievementCriteriaRequirement
    {
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            return target != null && target is Character &&
                   (this.Value1 == 0U || (ClassId) this.Value1 == target.Class) &&
                   (this.Value2 == 0U || (RaceId) this.Value2 == target.Race);
        }
    }
}
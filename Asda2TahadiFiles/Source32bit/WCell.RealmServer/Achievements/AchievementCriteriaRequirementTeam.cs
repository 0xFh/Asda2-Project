using WCell.Constants.Factions;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    public class AchievementCriteriaRequirementTeam : AchievementCriteriaRequirement
    {
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            if (target == null || !(target is Character))
                return false;
            return target.FactionId == (FactionId) this.Value1;
        }
    }
}
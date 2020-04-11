using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    public class AchievementCriteriaRequirementMapPlayerCount : AchievementCriteriaRequirement
    {
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            return (long) chr.Map.PlayerCount <= (long) this.Value1;
        }
    }
}
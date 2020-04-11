using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    public class AchievementCriteriaRequirementMapDifficulty : AchievementCriteriaRequirement
    {
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            return (int) chr.Map.DifficultyIndex == (int) this.Value1;
        }
    }
}
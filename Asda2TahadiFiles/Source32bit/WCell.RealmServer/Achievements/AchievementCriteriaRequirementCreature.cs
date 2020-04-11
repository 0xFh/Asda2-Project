using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    public class AchievementCriteriaRequirementCreature : AchievementCriteriaRequirement
    {
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            if (target == null)
                return false;
            return (int) this.Value1 == (int) target.EntryId;
        }
    }
}
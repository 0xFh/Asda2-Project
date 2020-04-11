using WCell.Constants.Misc;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Titles;

namespace WCell.RealmServer.Achievements
{
    public class AchievementCriteriaRequirementKnownTitle : AchievementCriteriaRequirement
    {
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            CharacterTitleEntry titleEntry = TitleMgr.GetTitleEntry((TitleId) this.Value1);
            if (titleEntry == null || chr == null)
                return false;
            return chr.HasTitle(titleEntry.TitleId);
        }
    }
}
using System.Runtime.InteropServices;
using WCell.Constants.Achievements;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class LearnSpellAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public SpellId SpellId;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= 1U;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if ((SpellId) value1 != this.SpellId || !achievements.Owner.PlayerSpells.Contains((uint) this.SpellId))
                return;
            achievements.SetCriteriaProgress((AchievementCriteriaEntry) this, 1U, ProgressType.ProgressHighest);
        }
    }
}
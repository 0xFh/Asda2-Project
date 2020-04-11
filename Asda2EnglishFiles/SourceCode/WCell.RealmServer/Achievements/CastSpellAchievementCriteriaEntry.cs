using System.Runtime.InteropServices;
using WCell.Constants.Achievements;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class CastSpellAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public SpellId SpellId;
        public uint SpellCount;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= this.SpellCount;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if ((SpellId) value1 != this.SpellId)
                return;
            achievements.SetCriteriaProgress((AchievementCriteriaEntry) this, 1U, ProgressType.ProgressAccumulate);
        }
    }
}
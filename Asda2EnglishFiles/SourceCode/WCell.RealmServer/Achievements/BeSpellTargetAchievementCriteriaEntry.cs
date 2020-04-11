using System.Runtime.InteropServices;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class BeSpellTargetAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public SpellId SpellId;
        public uint SpellCount;
    }
}
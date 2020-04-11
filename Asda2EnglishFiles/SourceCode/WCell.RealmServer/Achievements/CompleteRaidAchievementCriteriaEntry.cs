using System.Runtime.InteropServices;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class CompleteRaidAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public uint RaidSize;
    }
}
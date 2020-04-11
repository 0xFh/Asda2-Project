using System.Runtime.InteropServices;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class DeathInDungeonAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public uint ManLimit;
    }
}
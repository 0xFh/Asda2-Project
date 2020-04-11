using System.Runtime.InteropServices;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class WinRatedArenaAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public uint Unused;
        public uint Count;
        public uint Flag;
    }
}
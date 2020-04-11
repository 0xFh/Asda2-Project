using System.Runtime.InteropServices;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class HonorableKillAtAreaAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public uint AreaId;
        public uint KillCount;
    }
}
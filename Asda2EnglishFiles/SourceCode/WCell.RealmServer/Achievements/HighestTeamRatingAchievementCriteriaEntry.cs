using System.Runtime.InteropServices;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class HighestTeamRatingAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public uint TeamSize;
    }
}
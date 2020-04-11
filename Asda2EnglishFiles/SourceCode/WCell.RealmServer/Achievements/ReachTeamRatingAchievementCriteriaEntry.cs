using System.Runtime.InteropServices;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class ReachTeamRatingAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public uint TeamSize;
        public uint TeamRating;
    }
}
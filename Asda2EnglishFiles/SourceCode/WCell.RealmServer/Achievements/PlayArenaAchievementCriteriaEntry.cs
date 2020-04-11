using System.Runtime.InteropServices;
using WCell.Constants.World;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class PlayArenaAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public MapId MapId;
    }
}
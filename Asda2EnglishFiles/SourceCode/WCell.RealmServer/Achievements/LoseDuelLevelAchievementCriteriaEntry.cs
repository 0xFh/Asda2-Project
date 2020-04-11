using System.Runtime.InteropServices;
using WCell.Constants.Achievements;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class LoseDuelLevelAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public uint Unused;
        public uint DuelCount;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            achievements.SetCriteriaProgress((AchievementCriteriaEntry) this, value1, ProgressType.ProgressAccumulate);
        }
    }
}
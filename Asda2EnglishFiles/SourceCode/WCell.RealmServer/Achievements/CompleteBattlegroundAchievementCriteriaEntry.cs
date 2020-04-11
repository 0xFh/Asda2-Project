using System.Runtime.InteropServices;
using WCell.Constants.Achievements;
using WCell.Constants.World;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class CompleteBattlegroundAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public MapId MapId;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 == 0U || this.MapId != (MapId) value1)
                return;
            achievements.SetCriteriaProgress((AchievementCriteriaEntry) this, value2, ProgressType.ProgressAccumulate);
        }
    }
}
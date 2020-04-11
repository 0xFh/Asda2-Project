using System.Runtime.InteropServices;
using WCell.Constants.Achievements;
using WCell.Constants.Items;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class LootItemAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public Asda2ItemId ItemId;
        public uint ItemCount;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (this.ItemId != (Asda2ItemId) value1)
                return;
            achievements.SetCriteriaProgress((AchievementCriteriaEntry) this, value2, ProgressType.ProgressAccumulate);
        }

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= this.ItemCount;
        }
    }
}
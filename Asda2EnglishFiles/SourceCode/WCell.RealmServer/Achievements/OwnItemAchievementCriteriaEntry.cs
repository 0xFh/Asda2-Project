using System.Runtime.InteropServices;
using WCell.Constants.Items;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class OwnItemAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public Asda2ItemId ItemId;
        public uint ItemCount;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= this.ItemCount;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
        }
    }
}
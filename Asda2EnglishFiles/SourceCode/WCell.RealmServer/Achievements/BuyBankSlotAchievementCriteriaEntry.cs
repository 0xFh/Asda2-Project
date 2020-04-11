using System.Runtime.InteropServices;
using WCell.Constants.Achievements;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class BuyBankSlotAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public uint Unused;
        public uint NumberOfBankSlots;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= this.NumberOfBankSlots;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            achievements.SetCriteriaProgress((AchievementCriteriaEntry) this, (uint) achievements.Owner.BankBagSlots,
                ProgressType.ProgressHighest);
        }
    }
}
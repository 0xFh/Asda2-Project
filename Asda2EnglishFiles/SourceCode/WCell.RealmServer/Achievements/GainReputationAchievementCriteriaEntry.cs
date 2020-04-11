using System.Runtime.InteropServices;
using WCell.Constants.Achievements;
using WCell.Constants.Factions;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class GainReputationAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public FactionId FactionId;
        public uint ReputationAmount;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= this.ReputationAmount;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if ((FactionId) value1 != this.FactionId)
                return;
            int num = achievements.Owner.Reputations.GetValue(FactionMgr.GetFactionIndex(this.FactionId));
            achievements.SetCriteriaProgress((AchievementCriteriaEntry) this, (uint) num, ProgressType.ProgressHighest);
        }
    }
}
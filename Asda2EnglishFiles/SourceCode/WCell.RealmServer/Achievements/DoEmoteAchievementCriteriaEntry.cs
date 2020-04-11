using System.Runtime.InteropServices;
using WCell.Constants.Achievements;
using WCell.Constants.Chat;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievements
{
    [StructLayout(LayoutKind.Sequential)]
    public class DoEmoteAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        public TextEmote emoteId;
        public uint countOfEmotes;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= this.countOfEmotes;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 == 0U || (TextEmote) value1 != this.emoteId)
                return;
            achievements.SetCriteriaProgress((AchievementCriteriaEntry) this, 1U, ProgressType.ProgressAccumulate);
        }
    }
}
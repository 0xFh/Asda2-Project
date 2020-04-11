using WCell.Constants.Achievements;
using WCell.Core.DBC;

namespace WCell.RealmServer.Achievements
{
    public class AchievementCategoryEntryConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            AchievementCategoryEntry achievementCategoryEntry = new AchievementCategoryEntry()
            {
                ID = (AchievementCategoryEntryId) DBCRecordConverter.GetUInt32(rawData, 0),
                ParentCategory = (AchievementCategoryEntryId) DBCRecordConverter.GetUInt32(rawData, 1)
            };
            AchievementMgr.AchievementCategoryEntries[achievementCategoryEntry.ID] = achievementCategoryEntry;
        }
    }
}
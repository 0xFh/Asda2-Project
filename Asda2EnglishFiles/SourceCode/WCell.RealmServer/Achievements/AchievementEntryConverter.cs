using WCell.Constants.Achievements;
using WCell.Constants.World;
using WCell.Core.DBC;

namespace WCell.RealmServer.Achievements
{
    public class AchievementEntryConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            AchievementEntry achievementEntry = new AchievementEntry();
            achievementEntry.ID = DBCRecordConverter.GetUInt32(rawData, 0);
            achievementEntry.FactionFlag = DBCRecordConverter.GetInt32(rawData, 1);
            achievementEntry.MapID = (MapId) DBCRecordConverter.GetUInt32(rawData, 2);
            achievementEntry.Names = this.GetStrings(rawData, 4);
            AchievementCategoryEntryId uint32 = (AchievementCategoryEntryId) DBCRecordConverter.GetUInt32(rawData, 38);
            achievementEntry.Category = AchievementMgr.GetCategoryEntry(uint32);
            achievementEntry.Points = DBCRecordConverter.GetUInt32(rawData, 39);
            achievementEntry.Flags = (AchievementFlags) DBCRecordConverter.GetUInt32(rawData, 41);
            achievementEntry.Count = DBCRecordConverter.GetUInt32(rawData, 60);
            achievementEntry.RefAchievement = DBCRecordConverter.GetUInt32(rawData, 61);
            AchievementMgr.AchievementEntries[achievementEntry.ID] = achievementEntry;
        }
    }
}
using System.Collections.Generic;
using WCell.Constants.Achievements;
using WCell.Core.DBC;

namespace WCell.RealmServer.Achievements
{
    public class AchievementCriteriaConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            AchievementCriteriaType uint32 = (AchievementCriteriaType) DBCRecordConverter.GetUInt32(rawData, 2);
            AchievementCriteriaEntryCreator criteriaEntryCreator = AchievementMgr.GetCriteriaEntryCreator(uint32);
            if (criteriaEntryCreator == null)
                return;
            AchievementCriteriaEntry achievementCriteriaEntry = criteriaEntryCreator();
            achievementCriteriaEntry.AchievementCriteriaId = DBCRecordConverter.GetUInt32(rawData, 0);
            achievementCriteriaEntry.AchievementEntryId = DBCRecordConverter.GetUInt32(rawData, 1);
            AchievementEntry achievementEntry = achievementCriteriaEntry.AchievementEntry;
            if (achievementEntry == null)
                return;
            achievementEntry.Criteria.Add(achievementCriteriaEntry);
            DBCRecordConverter.CopyTo(rawData, (object) achievementCriteriaEntry, 3);
            achievementCriteriaEntry.CompletionFlag = DBCRecordConverter.GetUInt32(rawData, 26);
            achievementCriteriaEntry.GroupFlag =
                (AchievementCriteriaGroupFlags) DBCRecordConverter.GetUInt32(rawData, 27);
            achievementCriteriaEntry.TimeLimit = DBCRecordConverter.GetUInt32(rawData, 29);
            List<AchievementCriteriaEntry> criteriaEntriesByType = AchievementMgr.GetCriteriaEntriesByType(uint32);
            if (criteriaEntriesByType != null)
                criteriaEntriesByType.Add(achievementCriteriaEntry);
            achievementCriteriaEntry.RequirementSet =
                new AchievementCriteriaRequirementSet(achievementCriteriaEntry.AchievementCriteriaId);
            AchievementMgr.CriteriaEntriesById[achievementCriteriaEntry.AchievementCriteriaId] =
                achievementCriteriaEntry;
        }
    }
}
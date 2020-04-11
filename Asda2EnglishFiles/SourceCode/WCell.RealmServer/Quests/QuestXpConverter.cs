using System;
using WCell.Core.DBC;

namespace WCell.RealmServer.Quests
{
    public class QuestXpConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            QuestXPInfo questXpInfo = new QuestXPInfo()
            {
                Level = DBCRecordConverter.GetInt32(rawData, 0),
                RewXP = new int[8]
                {
                    DBCRecordConverter.GetInt32(rawData, 2),
                    DBCRecordConverter.GetInt32(rawData, 3),
                    DBCRecordConverter.GetInt32(rawData, 4),
                    DBCRecordConverter.GetInt32(rawData, 5),
                    DBCRecordConverter.GetInt32(rawData, 6),
                    DBCRecordConverter.GetInt32(rawData, 7),
                    DBCRecordConverter.GetInt32(rawData, 8),
                    DBCRecordConverter.GetInt32(rawData, 9)
                }
            };
            QuestMgr.QuestXpInfos[(uint) questXpInfo.Level] = questXpInfo;
        }
    }
}
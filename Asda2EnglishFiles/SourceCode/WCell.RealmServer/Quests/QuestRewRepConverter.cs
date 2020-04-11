using System;
using WCell.Core.DBC;

namespace WCell.RealmServer.Quests
{
    public class QuestRewRepConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            QuestRewRepInfo questRewRepInfo = new QuestRewRepInfo()
            {
                Id = DBCRecordConverter.GetInt32(rawData, 0),
                RewRep = new int[9]
                {
                    DBCRecordConverter.GetInt32(rawData, 2),
                    DBCRecordConverter.GetInt32(rawData, 3),
                    DBCRecordConverter.GetInt32(rawData, 4),
                    DBCRecordConverter.GetInt32(rawData, 5),
                    DBCRecordConverter.GetInt32(rawData, 6),
                    DBCRecordConverter.GetInt32(rawData, 7),
                    DBCRecordConverter.GetInt32(rawData, 8),
                    DBCRecordConverter.GetInt32(rawData, 9),
                    DBCRecordConverter.GetInt32(rawData, 10)
                }
            };
            QuestMgr.QuestRewRepInfos[(uint) (questRewRepInfo.Id - 1)] = questRewRepInfo;
        }
    }
}
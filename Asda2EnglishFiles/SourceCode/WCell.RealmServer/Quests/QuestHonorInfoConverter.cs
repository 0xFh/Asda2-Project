using System;
using WCell.Core.DBC;

namespace WCell.RealmServer.Quests
{
    public class QuestHonorInfoConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            QuestHonorInfo questHonorInfo = new QuestHonorInfo()
            {
                Level = DBCRecordConverter.GetInt32(rawData, 0) - 1,
                RewHonor = DBCRecordConverter.GetInt32(rawData, 1)
            };
            QuestMgr.QuestHonorInfos[(uint) questHonorInfo.Level] = questHonorInfo;
        }
    }
}
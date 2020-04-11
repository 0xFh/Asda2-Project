using System;
using WCell.Core.DBC;

namespace WCell.RealmServer.Quests
{
  public class QuestHonorInfoConverter : DBCRecordConverter
  {
    public override void Convert(byte[] rawData)
    {
      QuestHonorInfo questHonorInfo = new QuestHonorInfo
      {
        Level = GetInt32(rawData, 0) - 1,
        RewHonor = GetInt32(rawData, 1)
      };
      QuestMgr.QuestHonorInfos[(uint) questHonorInfo.Level] = questHonorInfo;
    }
  }
}
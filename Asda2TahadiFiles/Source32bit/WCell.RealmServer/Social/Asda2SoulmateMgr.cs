using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Core.Initialization;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Social
{
  public static class Asda2SoulmateMgr
  {
    public static Dictionary<uint, List<Asda2SoulmateRelationRecord>> RecordsByAccId =
      new Dictionary<uint, List<Asda2SoulmateRelationRecord>>();

    public static Dictionary<ulong, Asda2SoulmateRelationRecord> RecordsBuFullId =
      new Dictionary<ulong, Asda2SoulmateRelationRecord>();

    public static Asda2SoulmateRelationRecord GetSoulmateRecord(uint accId, uint relatedAccId)
    {
      ulong key = ((ulong) accId << 32) + relatedAccId;
      if(RecordsBuFullId.ContainsKey(key))
        return RecordsBuFullId[key];
      return null;
    }

    public static Asda2SoulmateRelationRecord GetSoulmateRecord(uint accId)
    {
      if(!RecordsByAccId.ContainsKey(accId))
        return null;
      return RecordsByAccId[accId]
        .FirstOrDefault(
          r => r.IsActive);
    }

    [Initialization(InitializationPass.Tenth, "Initialize Soulmates")]
    public static void LoadAll()
    {
      foreach(Asda2SoulmateRelationRecord soulmateRelationRecord in ActiveRecordBase<Asda2SoulmateRelationRecord>
        .FindAll())
      {
        if(!RecordsByAccId.ContainsKey(soulmateRelationRecord.AccId))
          RecordsByAccId.Add(soulmateRelationRecord.AccId,
            new List<Asda2SoulmateRelationRecord>());
        if(!RecordsByAccId.ContainsKey(soulmateRelationRecord.RelatedAccId))
          RecordsByAccId.Add(soulmateRelationRecord.RelatedAccId,
            new List<Asda2SoulmateRelationRecord>());
        RecordsByAccId[soulmateRelationRecord.AccId].Add(soulmateRelationRecord);
        RecordsByAccId[soulmateRelationRecord.RelatedAccId].Add(soulmateRelationRecord);
        RecordsBuFullId.Add(soulmateRelationRecord.DirectId, soulmateRelationRecord);
        RecordsBuFullId.Add(soulmateRelationRecord.RevercedId, soulmateRelationRecord);
      }
    }

    public static void CreateNewOrUpdateSoulmateRelation(Character chr, Character targetChr)
    {
      Asda2SoulmateRelationRecord soulmateRecord = GetSoulmateRecord(chr.AccId, targetChr.AccId);
      if(soulmateRecord == null)
      {
        Asda2SoulmateRelationRecord soulmateRelationRecord =
          new Asda2SoulmateRelationRecord(chr.AccId, targetChr.AccId)
          {
            IsActive = true
          };
        if(!RecordsByAccId.ContainsKey(soulmateRelationRecord.AccId))
          RecordsByAccId.Add(soulmateRelationRecord.AccId,
            new List<Asda2SoulmateRelationRecord>());
        if(!RecordsByAccId.ContainsKey(soulmateRelationRecord.RelatedAccId))
          RecordsByAccId.Add(soulmateRelationRecord.RelatedAccId,
            new List<Asda2SoulmateRelationRecord>());
        RecordsByAccId[soulmateRelationRecord.AccId].Add(soulmateRelationRecord);
        RecordsByAccId[soulmateRelationRecord.RelatedAccId].Add(soulmateRelationRecord);
        RecordsBuFullId.Add(soulmateRelationRecord.DirectId, soulmateRelationRecord);
        RecordsBuFullId.Add(soulmateRelationRecord.RevercedId, soulmateRelationRecord);
        soulmateRelationRecord.UpdateCharacters();
      }
      else
      {
        soulmateRecord.IsActive = true;
        soulmateRecord.UpdateCharacters();
      }
    }

    public static List<Asda2SoulmateRelationRecord> GetSoulmateRecords(uint accId)
    {
      List<Asda2SoulmateRelationRecord> soulmateRelationRecordList = new List<Asda2SoulmateRelationRecord>();
      Asda2SoulmateRelationRecord[] allByProperty1 =
        ActiveRecordBase<Asda2SoulmateRelationRecord>.FindAllByProperty("AccId", accId);
      Asda2SoulmateRelationRecord[] allByProperty2 =
        ActiveRecordBase<Asda2SoulmateRelationRecord>.FindAllByProperty("RelatedAccId", accId);
      soulmateRelationRecordList.AddRange(allByProperty1);
      soulmateRelationRecordList.AddRange(allByProperty2);
      return soulmateRelationRecordList;
    }
  }
}
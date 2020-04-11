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
            ulong key = ((ulong) accId << 32) + (ulong) relatedAccId;
            if (Asda2SoulmateMgr.RecordsBuFullId.ContainsKey(key))
                return Asda2SoulmateMgr.RecordsBuFullId[key];
            return (Asda2SoulmateRelationRecord) null;
        }

        public static Asda2SoulmateRelationRecord GetSoulmateRecord(uint accId)
        {
            if (!Asda2SoulmateMgr.RecordsByAccId.ContainsKey(accId))
                return (Asda2SoulmateRelationRecord) null;
            return Asda2SoulmateMgr.RecordsByAccId[accId]
                .FirstOrDefault<Asda2SoulmateRelationRecord>(
                    (Func<Asda2SoulmateRelationRecord, bool>) (r => r.IsActive));
        }

        [WCell.Core.Initialization.Initialization(InitializationPass.Tenth, "Initialize Soulmates")]
        public static void LoadAll()
        {
            foreach (Asda2SoulmateRelationRecord soulmateRelationRecord in ActiveRecordBase<Asda2SoulmateRelationRecord>
                .FindAll())
            {
                if (!Asda2SoulmateMgr.RecordsByAccId.ContainsKey(soulmateRelationRecord.AccId))
                    Asda2SoulmateMgr.RecordsByAccId.Add(soulmateRelationRecord.AccId,
                        new List<Asda2SoulmateRelationRecord>());
                if (!Asda2SoulmateMgr.RecordsByAccId.ContainsKey(soulmateRelationRecord.RelatedAccId))
                    Asda2SoulmateMgr.RecordsByAccId.Add(soulmateRelationRecord.RelatedAccId,
                        new List<Asda2SoulmateRelationRecord>());
                Asda2SoulmateMgr.RecordsByAccId[soulmateRelationRecord.AccId].Add(soulmateRelationRecord);
                Asda2SoulmateMgr.RecordsByAccId[soulmateRelationRecord.RelatedAccId].Add(soulmateRelationRecord);
                Asda2SoulmateMgr.RecordsBuFullId.Add(soulmateRelationRecord.DirectId, soulmateRelationRecord);
                Asda2SoulmateMgr.RecordsBuFullId.Add(soulmateRelationRecord.RevercedId, soulmateRelationRecord);
            }
        }

        public static void CreateNewOrUpdateSoulmateRelation(Character chr, Character targetChr)
        {
            Asda2SoulmateRelationRecord soulmateRecord = Asda2SoulmateMgr.GetSoulmateRecord(chr.AccId, targetChr.AccId);
            if (soulmateRecord == null)
            {
                Asda2SoulmateRelationRecord soulmateRelationRecord =
                    new Asda2SoulmateRelationRecord(chr.AccId, targetChr.AccId)
                    {
                        IsActive = true
                    };
                if (!Asda2SoulmateMgr.RecordsByAccId.ContainsKey(soulmateRelationRecord.AccId))
                    Asda2SoulmateMgr.RecordsByAccId.Add(soulmateRelationRecord.AccId,
                        new List<Asda2SoulmateRelationRecord>());
                if (!Asda2SoulmateMgr.RecordsByAccId.ContainsKey(soulmateRelationRecord.RelatedAccId))
                    Asda2SoulmateMgr.RecordsByAccId.Add(soulmateRelationRecord.RelatedAccId,
                        new List<Asda2SoulmateRelationRecord>());
                Asda2SoulmateMgr.RecordsByAccId[soulmateRelationRecord.AccId].Add(soulmateRelationRecord);
                Asda2SoulmateMgr.RecordsByAccId[soulmateRelationRecord.RelatedAccId].Add(soulmateRelationRecord);
                Asda2SoulmateMgr.RecordsBuFullId.Add(soulmateRelationRecord.DirectId, soulmateRelationRecord);
                Asda2SoulmateMgr.RecordsBuFullId.Add(soulmateRelationRecord.RevercedId, soulmateRelationRecord);
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
                ActiveRecordBase<Asda2SoulmateRelationRecord>.FindAllByProperty("AccId", (object) accId);
            Asda2SoulmateRelationRecord[] allByProperty2 =
                ActiveRecordBase<Asda2SoulmateRelationRecord>.FindAllByProperty("RelatedAccId", (object) accId);
            soulmateRelationRecordList.AddRange((IEnumerable<Asda2SoulmateRelationRecord>) allByProperty1);
            soulmateRelationRecordList.AddRange((IEnumerable<Asda2SoulmateRelationRecord>) allByProperty2);
            return soulmateRelationRecordList;
        }
    }
}
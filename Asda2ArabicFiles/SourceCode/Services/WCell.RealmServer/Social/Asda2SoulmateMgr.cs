using System.Collections.Generic;
using System.Linq;
using WCell.Core.Initialization;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Social
{
    public static class Asda2SoulmateMgr
    {
        public static Dictionary<uint,List<Asda2SoulmateRelationRecord>> RecordsByAccId = new Dictionary<uint, List<Asda2SoulmateRelationRecord>>();
        public static Dictionary<ulong, Asda2SoulmateRelationRecord> RecordsBuFullId = new Dictionary<ulong, Asda2SoulmateRelationRecord>();
        public static Asda2SoulmateRelationRecord GetSoulmateRecord(uint accId,uint relatedAccId)
        {
            var key = ((ulong) accId << 32) + relatedAccId;
            if (RecordsBuFullId.ContainsKey(key))
                return RecordsBuFullId[key];
            return null;
        }
        public static Asda2SoulmateRelationRecord GetSoulmateRecord(uint accId)
        {
            if (!RecordsByAccId.ContainsKey(accId))
                return null;
            var records = RecordsByAccId[accId];
            var rec = records.FirstOrDefault(r => r.IsActive);
            return rec;
        }
        [Initialization(InitializationPass.Tenth, "Initialize Soulmates")]
        public static void LoadAll()
        {
            var records = Asda2SoulmateRelationRecord.FindAll();
            foreach (var asda2SoulmateRelationRecord in records)
            {
                if(!RecordsByAccId.ContainsKey(asda2SoulmateRelationRecord.AccId))
                    RecordsByAccId.Add(asda2SoulmateRelationRecord.AccId,new List<Asda2SoulmateRelationRecord>());
                if (!RecordsByAccId.ContainsKey(asda2SoulmateRelationRecord.RelatedAccId))
                    RecordsByAccId.Add(asda2SoulmateRelationRecord.RelatedAccId, new List<Asda2SoulmateRelationRecord>());
                RecordsByAccId[asda2SoulmateRelationRecord.AccId].Add(asda2SoulmateRelationRecord);
                RecordsByAccId[asda2SoulmateRelationRecord.RelatedAccId].Add(asda2SoulmateRelationRecord);
                RecordsBuFullId.Add(asda2SoulmateRelationRecord.DirectId, asda2SoulmateRelationRecord);
                RecordsBuFullId.Add(asda2SoulmateRelationRecord.RevercedId, asda2SoulmateRelationRecord);
            }
        }
        public static void CreateNewOrUpdateSoulmateRelation(Character chr, Character targetChr)
        {
            var oldRecord = GetSoulmateRecord(chr.AccId, targetChr.AccId);
            if (oldRecord == null)
            {
                var newRecord = new Asda2SoulmateRelationRecord(chr.AccId, targetChr.AccId) {IsActive = true};
                if (!RecordsByAccId.ContainsKey(newRecord.AccId))
                    RecordsByAccId.Add(newRecord.AccId, new List<Asda2SoulmateRelationRecord>());
                if (!RecordsByAccId.ContainsKey(newRecord.RelatedAccId))
                    RecordsByAccId.Add(newRecord.RelatedAccId, new List<Asda2SoulmateRelationRecord>());
                RecordsByAccId[newRecord.AccId].Add(newRecord);
                RecordsByAccId[newRecord.RelatedAccId].Add(newRecord);
                RecordsBuFullId.Add(newRecord.DirectId, newRecord);
                RecordsBuFullId.Add(newRecord.RevercedId, newRecord);
                newRecord.UpdateCharacters();
            }
            else
            {
                oldRecord.IsActive = true;
                oldRecord.UpdateCharacters();
            }
        }

        public static List<Asda2SoulmateRelationRecord> GetSoulmateRecords(uint accId)
        {
            var r = new List<Asda2SoulmateRelationRecord>();
            var by1 = Asda2SoulmateRelationRecord.FindAllByProperty("AccId", accId);
            var by2 = Asda2SoulmateRelationRecord.FindAllByProperty("RelatedAccId", accId);
            r.AddRange(by1);
            r.AddRange(by2);
            return r;
        }
    }
}

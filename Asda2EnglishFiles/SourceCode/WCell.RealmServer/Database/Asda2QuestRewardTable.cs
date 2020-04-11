using Castle.ActiveRecord;
using NHibernate.Criterion;
using NLog;
using WCell.Core.Database;

namespace WCell.RealmServer.Database
{
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property, Table = "Asda2QuestRewardTable")]
    public class Asda2QuestRewardTable : WCellRecord<Asda2QuestRewardTable>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [PrimaryKey(PrimaryKeyType.Assigned, "id")]
        public int id { get; set; }

        [Property(NotNull = true)] public int questid { get; set; }

        [Property(NotNull = true)] public int Unk1 { get; set; }

        [Property(NotNull = true)] public int Unk2 { get; set; }

        [Property(NotNull = true)] public int questlvl { get; set; }

        [Property(NotNull = true)] public int questrewardgold { get; set; }

        [Property(NotNull = true)] public int questrewardexp { get; set; }

        [Property(NotNull = true)] public int Unk3 { get; set; }

        [Property(NotNull = true)] public int Unk4 { get; set; }

        [Property(NotNull = true)] public int Unk5 { get; set; }

        [Property(NotNull = true)] public int Unk6 { get; set; }

        [Property(NotNull = true)] public int Unk7 { get; set; }

        [Property(NotNull = true)] public int Unk8 { get; set; }

        [Property(NotNull = true)] public int Unk9 { get; set; }

        [Property(NotNull = true)] public int itemid1 { get; set; }

        [Property(NotNull = true)] public int itemamount1 { get; set; }

        [Property(NotNull = true)] public int itemid2 { get; set; }

        [Property(NotNull = true)] public int itemamount2 { get; set; }

        [Property(NotNull = true)] public int itemid3 { get; set; }

        [Property(NotNull = true)] public int itemamount3 { get; set; }

        [Property(NotNull = true)] public int Unk10 { get; set; }

        [Property(NotNull = true)] public int Unk11 { get; set; }

        [Property(NotNull = true)] public int Unk12 { get; set; }

        [Property(NotNull = true)] public int Unk13 { get; set; }

        [Property(NotNull = true)] public int Unk14 { get; set; }

        public static Asda2QuestRewardTable GetQuestReward(int questid)
        {
            return ActiveRecordBase<Asda2QuestRewardTable>.Find((object) Restrictions.Eq(nameof(questid),
                (object) questid));
        }
    }
}
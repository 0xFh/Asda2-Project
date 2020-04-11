using Castle.ActiveRecord;
using NHibernate.Criterion;
using NLog;
using WCell.Core.Database;

namespace WCell.RealmServer.Database
{
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property, Table = "Asda2QuestNpc")]
    public class Asda2QuestNpc : WCellRecord<Asda2QuestNpc>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [PrimaryKey(PrimaryKeyType.Assigned, "id")]
        public int id { get; set; }

        [Property(NotNull = true)] public int npcid { get; set; }

        [Property(NotNull = true)] public int questnum { get; set; }

        [Property(NotNull = true)] public int questid { get; set; }

        [Property(NotNull = true)] public string questname { get; set; }

        public static Asda2QuestNpc GetQuestId(int npcid, int questnum)
        {
            return ActiveRecordBase<Asda2QuestNpc>.FindOne(new ICriterion[1]
            {
                (ICriterion) ((AbstractCriterion) Restrictions.Eq(nameof(npcid), (object) npcid) &&
                              (AbstractCriterion) Restrictions.Eq(nameof(questnum), (object) questnum))
            });
        }
    }
}
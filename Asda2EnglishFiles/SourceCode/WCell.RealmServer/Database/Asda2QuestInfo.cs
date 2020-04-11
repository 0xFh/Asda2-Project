using Castle.ActiveRecord;
using NHibernate.Criterion;
using NLog;
using WCell.Core.Database;

namespace WCell.RealmServer.Database
{
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property, Table = "Asda2QuestInfo")]
    public class Asda2QuestInfo : WCellRecord<Asda2QuestInfo>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [PrimaryKey(PrimaryKeyType.Assigned, "id")]
        public int id { get; set; }

        [Property(NotNull = true)] public int questid { get; set; }

        [Property(NotNull = true)] public int questUnk1 { get; set; }

        [Property(NotNull = true)] public int npcId { get; set; }

        [Property(NotNull = true)] public string npcName { get; set; }

        [Property(NotNull = true)] public int questItemId1 { get; set; }

        [Property(NotNull = true)] public int questItemAmount1 { get; set; }

        [Property(NotNull = true)] public int questUnk20 { get; set; }

        [Property(NotNull = true)] public int questItemId2 { get; set; }

        [Property(NotNull = true)] public int questItemAmount2 { get; set; }

        [Property(NotNull = true)] public int questUnk21 { get; set; }

        [Property(NotNull = true)] public int questItemId3 { get; set; }

        [Property(NotNull = true)] public int questItemAmount3 { get; set; }

        [Property(NotNull = true)] public int questUnk22 { get; set; }

        [Property(NotNull = true)] public int questItemId4 { get; set; }

        [Property(NotNull = true)] public int questItemAmount4 { get; set; }

        [Property(NotNull = true)] public int questUnk23 { get; set; }

        [Property(NotNull = true)] public int questItemId5 { get; set; }

        [Property(NotNull = true)] public int questItemAmount5 { get; set; }

        [Property(NotNull = true)] public int questUnk24 { get; set; }

        [Property(NotNull = true)] public int questidentifier { get; set; }

        [Property(NotNull = true)] public int questsecondid { get; set; }

        [Property(NotNull = true)] public int questcompleteidentifier { get; set; }

        [Property(NotNull = true)] public int questcompleteid { get; set; }

        [Property(NotNull = true)] public int questlvl { get; set; }

        public static Asda2QuestInfo GetQuestInfo(int questid, int questUnk1)
        {
            return ActiveRecordBase<Asda2QuestInfo>.FindOne(new ICriterion[1]
            {
                (ICriterion) ((AbstractCriterion) Restrictions.Eq(nameof(questid), (object) questid) &&
                              (AbstractCriterion) Restrictions.Eq(nameof(questUnk1), (object) questUnk1))
            });
        }
    }
}
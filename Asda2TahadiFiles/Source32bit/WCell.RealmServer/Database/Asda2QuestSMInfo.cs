using Castle.ActiveRecord;
using NHibernate.Criterion;
using NLog;
using WCell.Core.Database;

namespace WCell.RealmServer.Database
{
  [ActiveRecord(Access = PropertyAccess.Property, Table = "Asda2QuestSMInfo")]
  public class Asda2QuestSMInfo : WCellRecord<Asda2QuestSMInfo>
  {
    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    [PrimaryKey(PrimaryKeyType.Assigned, "id")]
    public int id { get; set; }

    [Property(NotNull = true)]
    public int itemid { get; set; }

    [Property(NotNull = true)]
    public int amount { get; set; }

    public static Asda2QuestSMInfo GetAmountItem(int itemid)
    {
      return Find(Restrictions.Eq(nameof(itemid), itemid));
    }
  }
}
using Castle.ActiveRecord;
using NLog;
using WCell.Core.Database;

namespace WCell.RealmServer.Database
{
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property, Table = "asda2QuestItemDrop")]
    public class Asda2QuestItemDrop : WCellRecord<Asda2QuestItemDrop>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [PrimaryKey(PrimaryKeyType.Assigned, "id")]
        public int id { get; set; }

        [Property(NotNull = true)] public int Unk1 { get; set; }

        [Property(NotNull = true)] public int Unk2 { get; set; }

        [Property(NotNull = true)] public int Unk3 { get; set; }

        [Property(NotNull = true)] public int Unk4 { get; set; }

        [Property(NotNull = true)] public int questid1 { get; set; }

        [Property(NotNull = true)] public int Unk5 { get; set; }

        [Property(NotNull = true)] public int Unk6 { get; set; }

        [Property(NotNull = true)] public int questitemid1 { get; set; }

        [Property(NotNull = true)] public int chance1 { get; set; }

        [Property(NotNull = true)] public int questid2 { get; set; }

        [Property(NotNull = true)] public int Unk7 { get; set; }

        [Property(NotNull = true)] public int Unk8 { get; set; }

        [Property(NotNull = true)] public int questitemid2 { get; set; }

        [Property(NotNull = true)] public int chance2 { get; set; }

        [Property(NotNull = true)] public int questid3 { get; set; }

        [Property(NotNull = true)] public int Unk9 { get; set; }

        [Property(NotNull = true)] public int Unk10 { get; set; }

        [Property(NotNull = true)] public int questitemid3 { get; set; }

        [Property(NotNull = true)] public int chance3 { get; set; }

        [Property(NotNull = true)] public int questid4 { get; set; }

        [Property(NotNull = true)] public int Unk11 { get; set; }

        [Property(NotNull = true)] public int Unk12 { get; set; }

        [Property(NotNull = true)] public int questitemid4 { get; set; }

        [Property(NotNull = true)] public int chance4 { get; set; }

        [Property(NotNull = true)] public int questid5 { get; set; }

        [Property(NotNull = true)] public int Unk13 { get; set; }

        [Property(NotNull = true)] public int Unk14 { get; set; }

        [Property(NotNull = true)] public int questitemid5 { get; set; }

        [Property(NotNull = true)] public int chance5 { get; set; }

        [Property(NotNull = true)] public int questid6 { get; set; }

        [Property(NotNull = true)] public int Unk15 { get; set; }

        [Property(NotNull = true)] public int Unk16 { get; set; }

        [Property(NotNull = true)] public int questitemid6 { get; set; }

        [Property(NotNull = true)] public int chance6 { get; set; }
    }
}
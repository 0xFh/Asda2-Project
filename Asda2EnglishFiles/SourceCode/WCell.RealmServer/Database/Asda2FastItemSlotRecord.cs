using Castle.ActiveRecord;
using NHibernate.Criterion;
using NLog;
using System;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Asda2_Items;

namespace WCell.RealmServer.Database
{
    /// <summary>The DB-representation of an FastItemSlot</summary>
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property, Table = "Asda2FastItemSlot")]
    public class Asda2FastItemSlotRecord : WCellRecord<Asda2FastItemSlotRecord>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly NHIdGenerator IDGenerator =
            new NHIdGenerator(typeof(Asda2FastItemSlotRecord), nameof(Guid), 1L);

        /// <summary>Returns the next unique Id for a new Item</summary>
        public static long NextId()
        {
            return Asda2FastItemSlotRecord.IDGenerator.Next();
        }

        internal static Asda2FastItemSlotRecord CreateRecord()
        {
            try
            {
                Asda2FastItemSlotRecord fastItemSlotRecord = new Asda2FastItemSlotRecord();
                fastItemSlotRecord.Guid = (long) (uint) Asda2FastItemSlotRecord.IDGenerator.Next();
                fastItemSlotRecord.State = RecordState.New;
                return fastItemSlotRecord;
            }
            catch (Exception ex)
            {
                throw new WCellException(ex, "Unable to create new Asda2FastItemSlotRecord.", new object[0]);
            }
        }

        [Property(NotNull = true)] public uint OwnerId { get; set; }

        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public long Guid { get; set; }

        [Property(NotNull = true)] public byte PanelNum { get; set; }

        [Property(NotNull = true)] public byte PanelSlot { get; set; }

        [Property(NotNull = true)] public byte InventoryType { get; set; }

        [Property(NotNull = true)] public int ItemOrSkillId { get; set; }

        [Property(NotNull = true)] public short InventorySlot { get; set; }

        [Property(NotNull = true)] public short SrcInfo { get; set; }

        public static Asda2FastItemSlotRecord[] LoadItems(uint lowCharId)
        {
            return ActiveRecordBase<Asda2FastItemSlotRecord>.FindAll(new ICriterion[1]
            {
                (ICriterion) Restrictions.Eq("OwnerId", (object) lowCharId)
            });
        }

        public static Asda2FastItemSlotRecord GetRecordByID(long id)
        {
            return ActiveRecordBase<Asda2FastItemSlotRecord>.FindOne(new ICriterion[1]
            {
                (ICriterion) Restrictions.Eq("Guid", (object) id)
            });
        }

        [Property(NotNull = true)] public int Amount { get; set; }

        public static Asda2FastItemSlotRecord CreateRecord(byte panel, byte panelSlot, Asda2InventoryType invType,
            byte invSlot, short itemOrSkillId, int amount, uint ownerId, byte srcInfo)
        {
            Asda2FastItemSlotRecord record = Asda2FastItemSlotRecord.CreateRecord();
            record.PanelNum = panel;
            record.PanelSlot = panelSlot;
            record.InventoryType = (byte) invType;
            record.InventorySlot = (short) invSlot;
            record.ItemOrSkillId = (int) itemOrSkillId;
            record.Amount = amount;
            record.OwnerId = ownerId;
            record.SrcInfo = (short) srcInfo;
            return record;
        }

        public override string ToString()
        {
            return string.Format(
                "Asda2FastItemSlotRecord panel:{0} panelSlot:{1} invType:{2} invSlot:{3} itemOrSkillId:{4} amount:{5}",
                (object) this.PanelNum, (object) this.PanelSlot, (object) (Asda2InventoryType) this.InventoryType,
                (object) this.InventorySlot, (object) this.ItemOrSkillId, (object) this.Amount);
        }
    }
}
using System;
using Castle.ActiveRecord;
using NHibernate.Criterion;
using NLog;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Asda2_Items;
namespace WCell.RealmServer.Database
{
    /// <summary>
    /// The DB-representation of an FastItemSlot
    /// </summary>
    [ActiveRecord(Access = PropertyAccess.Property, Table = "Asda2FastItemSlot")]
    public class Asda2FastItemSlotRecord : WCellRecord<Asda2FastItemSlotRecord>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly NHIdGenerator IDGenerator =
            new NHIdGenerator(typeof(Asda2FastItemSlotRecord), "Guid");

        /// <summary>
        /// Returns the next unique Id for a new Item
        /// </summary>
        public static long NextId()
        {
            return IDGenerator.Next();
        }

        internal static Asda2FastItemSlotRecord CreateRecord()
        {
            try
            {
                var itemRecord = new Asda2FastItemSlotRecord
                {
                    Guid = (uint)IDGenerator.Next(),
                    State = RecordState.New
                };

                return itemRecord;
            }
            catch (Exception ex)
            {
                throw new WCellException(ex, "Unable to create new Asda2FastItemSlotRecord.");
            }
        }

        [Property(NotNull = true)]
        public uint OwnerId
        {
            get;
            set;
        }
        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public long Guid
        {
            get;
            set;
        }
        [Property(NotNull = true)]
        public byte PanelNum
        {
            get;
            set;
        }
        [Property(NotNull = true)]
        public byte PanelSlot
        {
            get;
            set;
        }
        [Property(NotNull = true)]
        public byte InventoryType
        {
            get;
            set;
        }
        [Property(NotNull = true)]
        public int ItemOrSkillId
        {
            get;
            set;
        }
        [Property(NotNull = true)]
        public short InventorySlot
        {
            get;
            set;
        }
        [Property(NotNull = true)]
        public short SrcInfo { get; set; }
        #region Loading
        public static Asda2FastItemSlotRecord[] LoadItems(uint lowCharId)
        {
            return FindAll(Restrictions.Eq("OwnerId", lowCharId));
        }

        public static Asda2FastItemSlotRecord GetRecordByID(long id)
        {
            return FindOne(Restrictions.Eq("Guid", id));}
        #endregion

        [Property(NotNull = true)]
        public int Amount { get; set; }
        public static Asda2FastItemSlotRecord CreateRecord(byte panel, byte panelSlot, Asda2InventoryType invType, byte invSlot, short itemOrSkillId, int amount,uint ownerId,byte srcInfo)
        {
            var item = CreateRecord();
            item.PanelNum = panel;
            item.PanelSlot = panelSlot;
            item.InventoryType = (byte) invType;
            item.InventorySlot = invSlot;
            item.ItemOrSkillId = itemOrSkillId;
            item.Amount = amount;
            item.OwnerId = ownerId;
            item.SrcInfo = srcInfo;
            return item;
        }

        public override string ToString()
        {
            return string.Format("Asda2FastItemSlotRecord panel:{0} panelSlot:{1} invType:{2} invSlot:{3} itemOrSkillId:{4} amount:{5}",PanelNum,PanelSlot,(Asda2InventoryType)InventoryType,InventorySlot,ItemOrSkillId,Amount);
        }
    }
}
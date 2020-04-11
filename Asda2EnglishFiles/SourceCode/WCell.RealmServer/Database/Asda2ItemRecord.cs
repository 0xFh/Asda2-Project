using Castle.ActiveRecord;
using NHibernate.Criterion;
using NLog;
using System;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Database
{
    /// <summary>
    /// The DB-representation of an Item
    /// TODO: Charges
    /// </summary>
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property, Table = "Asda2Item")]
    public class Asda2ItemRecord : WCellRecord<Asda2ItemRecord>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static readonly Order ContOrder = new Order("ContainerSlots", false);

        private static readonly NHIdGenerator _idGenerator =
            new NHIdGenerator(typeof(Asda2ItemRecord), nameof(Guid), 1L);

        /// <summary>Returns the next unique Id for a new Item</summary>
        public static long NextId()
        {
            return Asda2ItemRecord._idGenerator.Next();
        }

        internal static Asda2ItemRecord CreateRecord()
        {
            try
            {
                Asda2ItemRecord asda2ItemRecord = new Asda2ItemRecord();
                asda2ItemRecord.Guid = Asda2ItemRecord._idGenerator.Next();
                asda2ItemRecord.State = RecordState.New;
                return asda2ItemRecord;
            }
            catch (Exception ex)
            {
                throw new WCellException(ex, "Unable to create new ItemRecord.", new object[0]);
            }
        }

        [Property(NotNull = true)] public uint OwnerId { get; set; }

        public Asda2ItemTemplate Template
        {
            get { return Asda2ItemMgr.GetTemplate(this.ItemId); }
        }

        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public long Guid { get; set; }

        [Property(NotNull = true)] public int ItemId { get; set; }

        [Property(NotNull = true)] public byte InventoryType { get; set; }

        [Property(NotNull = true)] public short Slot { get; set; }

        [Property(NotNull = true)] public int CreatorId { get; set; }

        [Property(NotNull = true)] public byte Durability { get; set; }

        [Property(NotNull = true)] public int Duration { get; set; }

        [Property(NotNull = true)] public bool IsSoulBound { get; set; }

        public bool IsOwned
        {
            get
            {
                if (!this.IsAuctioned)
                    return this.MailId == 0;
                return false;
            }
        }

        /// <summary>if this is true, the item actually is auctioned</summary>
        [Property]
        public bool IsAuctioned { get; set; }

        /// <summary>
        /// The id of the mail that this Item is attached to (if any)
        /// </summary>
        [Property]
        public int MailId { get; set; }

        [Property] public int Soul1Id { get; set; }

        [Property] public int Soul2Id { get; set; }

        [Property] public int Soul3Id { get; set; }

        [Property] public int Soul4Id { get; set; }

        [Property] public byte Enchant { get; set; }

        [Property] public short Parametr1Type { get; set; }

        [Property] public short Parametr1Value { get; set; }

        [Property] public short Parametr2Type { get; set; }

        [Property] public short Parametr2Value { get; set; }

        [Property] public short Parametr3Type { get; set; }

        [Property] public ushort Parametr3Value { get; set; }

        [Property] public short Parametr4Type { get; set; }

        [Property] public short Parametr4Value { get; set; }

        [Property] public short Parametr5Type { get; set; }

        [Property] public short Parametr5Value { get; set; }

        [Property] public bool IsStackable { get; set; }

        public static Asda2ItemRecord[] LoadItems(uint lowCharId)
        {
            return ActiveRecordBase<Asda2ItemRecord>.FindAll(new ICriterion[1]
            {
                (ICriterion) Restrictions.Eq("OwnerId", (object) lowCharId)
            });
        }

        public static Asda2ItemRecord[] LoadItemsContainersFirst(uint lowChrId)
        {
            return ActiveRecordBase<Asda2ItemRecord>.FindAll(Asda2ItemRecord.ContOrder, new ICriterion[1]
            {
                (ICriterion) Restrictions.Eq("OwnerId", (object) lowChrId)
            });
        }

        public static Asda2ItemRecord[] LoadAuctionedItems()
        {
            return ActiveRecordBase<Asda2ItemRecord>.FindAll(new ICriterion[1]
            {
                (ICriterion) Restrictions.Eq("IsAuctioned", (object) true)
            });
        }

        public static Asda2ItemRecord GetRecordByID(long id)
        {
            return ActiveRecordBase<Asda2ItemRecord>.FindOne(new ICriterion[1]
            {
                (ICriterion) Restrictions.Eq("Guid", (object) id)
            });
        }

        [Property(NotNull = true)] public long CreatorEntityId { get; set; }

        [Property(NotNull = true)] public ushort Weight { get; set; }

        [Property(NotNull = true)] public byte SealCount { get; set; }

        public static Asda2ItemRecord GetRecordByID(uint itemLowId)
        {
            return Asda2ItemRecord.GetRecordByID((long) itemLowId);
        }

        [Property(NotNull = true)] public int Amount { get; set; }

        [Property] public int AuctionPrice { get; set; }

        [Property] public DateTime AuctionEndTime { get; set; }

        public int AuctionId
        {
            get { return (int) this.Guid; }
        }

        [Property] public string OwnerName { get; set; }

        [Property] public bool IsCrafted { get; set; }

        public static Asda2ItemRecord CreateRecord(Asda2ItemTemplate templ)
        {
            Asda2ItemRecord record = Asda2ItemRecord.CreateRecord();
            record.ItemId = (int) templ.Id;
            record.Amount = templ.MaxAmount;
            record.Durability = templ.MaxDurability;
            record.Duration = templ.Duration;
            record.Soul1Id = (int) templ.DefaultSoul1Id;
            return record;
        }

        public override string ToString()
        {
            return string.Format("ItemRecord \"{0}\" ({1}) #{2}", (object) this.ItemId, (object) this.Guid,
                (object) this.CreatorId);
        }
    }
}
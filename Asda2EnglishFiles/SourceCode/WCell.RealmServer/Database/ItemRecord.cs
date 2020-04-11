using Castle.ActiveRecord;
using NHibernate.Criterion;
using NLog;
using System;
using WCell.Constants.Items;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Items;
using WCell.RealmServer.Items.Enchanting;

namespace WCell.RealmServer.Database
{
    /// <summary>
    /// The DB-representation of an Item
    /// TODO: Charges
    /// </summary>
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property)]
    public class ItemRecord : WCellRecord<ItemRecord>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static readonly Order ContOrder = new Order("ContainerSlots", false);

        private static readonly NHIdGenerator _idGenerator =
            new NHIdGenerator(typeof(Asda2ItemRecord), nameof(Guid), 1L);

        [Field("EntryId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _entryId;

        [Field("DisplayId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _displayId;

        [Field("ContainerSlot", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private byte _containerSlot;

        [Field("ItemFlags", NotNull = true)] private int flags;

        [Field("ItemTextId", Access = PropertyAccess.FieldCamelcase)]
        private int m_ItemTextId;

        /// <summary>Returns the next unique Id for a new Item</summary>
        public static long NextId()
        {
            return ItemRecord._idGenerator.Next();
        }

        internal static ItemRecord CreateRecord()
        {
            try
            {
                ItemRecord itemRecord = new ItemRecord();
                itemRecord.Guid = (long) (uint) ItemRecord._idGenerator.Next();
                itemRecord.State = RecordState.New;
                return itemRecord;
            }
            catch (Exception ex)
            {
                throw new WCellException(ex, "Unable to create new ItemRecord.", new object[0]);
            }
        }

        private void InitItemRecord()
        {
            ActiveRecordMediator.GetSessionFactoryHolder().GetConfiguration(typeof(ActiveRecordBase));
        }

        [Property(NotNull = true)] public int OwnerId { get; set; }

        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public long Guid { get; set; }

        public uint EntityLowId
        {
            get { return (uint) this.Guid; }
            set { this.Guid = (long) value; }
        }

        public uint EntryId
        {
            get { return (uint) this._entryId; }
            set { this._entryId = (int) value; }
        }

        public EntityId EntityId
        {
            get { return EntityId.GetItemId(this.EntityLowId); }
        }

        public uint DisplayId
        {
            get { return (uint) this._displayId; }
            set { this._displayId = (int) value; }
        }

        /// <summary>The slot of the container, holding this Item</summary>
        public byte ContainerSlot
        {
            get { return this._containerSlot; }
            set { this._containerSlot = value; }
        }

        [Property(NotNull = true)] public int Slot { get; set; }

        [Property(NotNull = true)] public long CreatorEntityId { get; set; }

        [Property(NotNull = true)] public long GiftCreatorEntityId { get; set; }

        [Property(NotNull = true)] public int Durability { get; set; }

        [Property(NotNull = true)] public int Duration { get; set; }

        public ItemFlags Flags
        {
            get { return (ItemFlags) this.flags; }
            set { this.flags = (int) value; }
        }

        public uint ItemTextId
        {
            get { return (uint) this.m_ItemTextId; }
            set { this.m_ItemTextId = (int) value; }
        }

        [Property] public string ItemText { get; set; }

        [Property(NotNull = true)] public int RandomProperty { get; set; }

        [Property(NotNull = true)] public int RandomSuffix { get; set; }

        /// <summary>Charges of the Use-spell</summary>
        [Property(NotNull = true)]
        public short Charges { get; set; }

        [Property(NotNull = true)] public int Amount { get; set; }

        public bool IsContainer
        {
            get { return this.ContSlots > 0; }
        }

        public bool IsEquippedContainer
        {
            get
            {
                if (this.ContSlots > 0 && this._containerSlot == byte.MaxValue)
                    return ItemMgr.ContainerSlotsWithBank[this.Slot];
                return false;
            }
        }

        public bool IsSoulbound
        {
            get { return this.Flags.HasFlag((Enum) ItemFlags.Soulbound); }
        }

        /// <summary>
        /// If this &gt; 0, we have a container with this amount of slots.
        /// </summary>
        [Property(NotNull = true)]
        public int ContSlots { get; set; }

        [Property] public int[] EnchantIds { get; set; }

        [Property] public int EnchantTempTime { get; set; }

        public ItemEnchantmentEntry GetEnchant(EnchantSlot slot)
        {
            if (this.EnchantIds != null)
                return EnchantMgr.GetEnchantmentEntry((uint) this.EnchantIds[(int) slot]);
            return (ItemEnchantmentEntry) null;
        }

        internal void SetEnchant(EnchantSlot slot, int id, int timeLeft)
        {
            if (this.EnchantIds == null)
                this.EnchantIds = new int[12];
            this.EnchantIds[(int) slot] = id;
            if (slot != EnchantSlot.Temporary)
                return;
            this.EnchantTempTime = timeLeft;
        }

        /// <summary>The life-time of the Item in seconds</summary>
        public ItemTemplate Template
        {
            get { return ItemMgr.GetTemplate(this.EntryId); }
        }

        public bool IsInventory
        {
            get
            {
                ItemTemplate template = this.Template;
                if (template == null)
                    return false;
                return template.IsInventory;
            }
        }

        public bool IsOwned
        {
            get
            {
                if (!this.IsAuctioned)
                    return this.MailId == 0L;
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
        public long MailId { get; set; }

        public static ItemRecord[] LoadItems(uint lowCharId)
        {
            return ActiveRecordBase<ItemRecord>.FindAll(new ICriterion[1]
            {
                (ICriterion) Restrictions.Eq("OwnerId", (object) (int) lowCharId)
            });
        }

        public static ItemRecord[] LoadItemsContainersFirst(uint lowChrId)
        {
            return ActiveRecordBase<ItemRecord>.FindAll(ItemRecord.ContOrder, new ICriterion[1]
            {
                (ICriterion) Restrictions.Eq("OwnerId", (object) (int) lowChrId)
            });
        }

        public static ItemRecord[] LoadAuctionedItems()
        {
            return ActiveRecordBase<ItemRecord>.FindAll(new ICriterion[1]
            {
                (ICriterion) Restrictions.Eq("IsAuctioned", (object) true)
            });
        }

        public static ItemRecord GetRecordByID(long id)
        {
            return ActiveRecordBase<ItemRecord>.FindOne(new ICriterion[1]
            {
                (ICriterion) Restrictions.Eq("Guid", (object) id)
            });
        }

        public static ItemRecord GetRecordByID(uint itemLowId)
        {
            return ItemRecord.GetRecordByID((long) itemLowId);
        }

        protected void OnLoad()
        {
            ItemTemplate template = this.Template;
            if (template != null)
                template.NotifyCreated(this);
            else
                ItemRecord.log.Warn("ItemRecord has invalid EntryId: " + (object) this);
        }

        public static ItemRecord CreateRecord(ItemTemplate templ)
        {
            ItemRecord record = ItemRecord.CreateRecord();
            record.EntryId = templ.Id;
            record.Amount = templ.MaxAmount;
            record.Durability = templ.MaxDurability;
            record.Flags = templ.Flags;
            record.ItemTextId = templ.PageTextId;
            record.RandomProperty = templ.RandomPropertiesId != 0U
                ? (int) templ.RandomPropertiesId
                : (int) templ.RandomSuffixId;
            record.RandomSuffix = (int) templ.RandomSuffixId;
            record.Duration = templ.Duration;
            if (templ.UseSpell != null)
                record.Charges = templ.UseSpell.Charges;
            return record;
        }

        public override string ToString()
        {
            return string.Format("ItemRecord \"{0}\" ({1}) #{2}", (object) this.EntityId, (object) this._entryId,
                (object) this.EntityLowId);
        }
    }
}
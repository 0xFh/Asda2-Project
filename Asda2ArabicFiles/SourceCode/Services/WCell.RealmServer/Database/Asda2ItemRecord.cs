using System;
using Castle.ActiveRecord;
using NHibernate.Criterion;
using NLog;
using WCell.Constants.Items;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Items;
using WCell.RealmServer.Items.Enchanting;

namespace WCell.RealmServer.Database
{
	/// <summary>
	/// The DB-representation of an Item
	/// TODO: Charges
	/// </summary>
    [ActiveRecord(Access = PropertyAccess.Property, Table = "Asda2Item")]
    public class Asda2ItemRecord : WCellRecord<Asda2ItemRecord>
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		static readonly Order ContOrder = new Order("ContainerSlots", false);
		private static readonly NHIdGenerator _idGenerator =
            new NHIdGenerator(typeof(Asda2ItemRecord), "Guid");


	    /// <summary>
		/// Returns the next unique Id for a new Item
		/// </summary>
		public static long NextId()
		{
			return _idGenerator.Next();
		}

        internal static Asda2ItemRecord CreateRecord()
		{
			try
			{
                var itemRecord = new Asda2ItemRecord
				{
					Guid = _idGenerator.Next(),
					State = RecordState.New
				};

				//s_log.Debug("creating new item with EntityId {0}", itemRecord.EntityId);
				//itemRecord.ItemRecordGuid = Guid.NewGuid();
				//itemRecord.EntityId = EntityIdSetter.GetItemEntityId();

				return itemRecord;
			}
			catch (Exception ex)
			{
				throw new WCellException(ex, "Unable to create new ItemRecord.");
			}
		}

        [Property(NotNull = true)]
        public uint OwnerId
        {
            get;
            set;
        }
        public Asda2ItemTemplate Template
        {
            get { return Asda2ItemMgr.GetTemplate(ItemId); }
        }
        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
		public long Guid
		{
			get;
			set;
		}
        [Property(NotNull = true)]
        public int ItemId
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
		public short Slot
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public int CreatorId
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public byte Durability
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public int Duration
		{
			get;
			set;
		}
        [Property(NotNull = true)]
        public bool IsSoulBound
        {
            get;
            set;
        }
		
	
		#region Non-Inventory Items
		public bool IsOwned
		{
			get { return !IsAuctioned && MailId == 0; }
		}

		/// <summary>
		/// if this is true, the item actually is auctioned
		/// </summary>
		[Property]
		public bool IsAuctioned
		{
			get;
			set;
		}

		/// <summary>
		/// The id of the mail that this Item is attached to (if any)
		/// </summary>
		[Property]
		public int MailId
		{
			get;
			set;
		}
        [Property]
        public int Soul1Id { get; set; }
        [Property]
        public int Soul2Id { get; set; }
        [Property]
        public int Soul3Id { get; set; }
        [Property]
        public int Soul4Id { get; set; }
        [Property]
        public byte Enchant { get; set; }
        [Property]
        public short Parametr1Type { get; set; }
        [Property]
        public short Parametr1Value { get; set; }
        [Property]
        public short Parametr2Type { get; set; }
        [Property]
        public short Parametr2Value { get; set; }
        [Property]
        public short Parametr3Type { get; set; }
        [Property]
        public ushort Parametr3Value { get; set; }
        [Property]
        public short Parametr4Type { get; set; }
        [Property]
        public short Parametr4Value { get; set; }
        [Property]
        public short Parametr5Type { get; set; }
        [Property]
        public short Parametr5Value { get; set; }
        [Property]
	    public bool IsStackable { get; set; }

	    #endregion

		#region Loading
		public static Asda2ItemRecord[] LoadItems(uint lowCharId)
		{
			return FindAll(Restrictions.Eq("OwnerId", lowCharId));
		}

        public static Asda2ItemRecord[] LoadItemsContainersFirst(uint lowChrId)
		{
			// containers first
			return FindAll(ContOrder, Restrictions.Eq("OwnerId", lowChrId));
		}

        public static Asda2ItemRecord[] LoadAuctionedItems()
		{
			return FindAll(Restrictions.Eq("IsAuctioned", true));
		}

        public static Asda2ItemRecord GetRecordByID(long id)
		{
			return FindOne(Restrictions.Eq("Guid", id));
		}
        [Property(NotNull = true)]
        public long CreatorEntityId
        {
            get;
            set;
        }
        [Property(NotNull = true)]
	    public ushort Weight { get; set; }

        [Property(NotNull = true)]
	    public byte SealCount { get; set; }

	    public static Asda2ItemRecord GetRecordByID(uint itemLowId)
		{
			return GetRecordByID((long)itemLowId);
		}
		#endregion

        [Property(NotNull = true)]
	    public int Amount { get; set; }
        [Property]
	    public int AuctionPrice { get; set; }
        [Property]
	    public DateTime AuctionEndTime { get; set; }

	    public int AuctionId
	    {
            get { return (int) Guid; }
	    }
        [Property]
        public string OwnerName { get; set; }
        [Property]
	    public bool IsCrafted { get; set; }

	    public static Asda2ItemRecord CreateRecord(Asda2ItemTemplate templ)
		{
			var item = CreateRecord();
			item.ItemId = (int) templ.Id;
			item.Amount =  templ.MaxAmount;
			item.Durability = templ.MaxDurability;
			item.Duration = templ.Duration;
		    item.Soul1Id = templ.DefaultSoul1Id;
			return item;
		}

		public override string ToString()
		{
			return string.Format("ItemRecord \"{0}\" ({1}) #{2}", ItemId, Guid, CreatorId);
		}
	}
}
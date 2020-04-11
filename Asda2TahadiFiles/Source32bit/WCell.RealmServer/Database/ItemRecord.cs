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
  [ActiveRecord(Access = PropertyAccess.Property)]
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

    [Field("ItemFlags", NotNull = true)]private int flags;

    [Field("ItemTextId", Access = PropertyAccess.FieldCamelcase)]
    private int m_ItemTextId;

    /// <summary>Returns the next unique Id for a new Item</summary>
    public static long NextId()
    {
      return _idGenerator.Next();
    }

    internal static ItemRecord CreateRecord()
    {
      try
      {
        ItemRecord itemRecord = new ItemRecord();
        itemRecord.Guid = (uint) _idGenerator.Next();
        itemRecord.State = RecordState.New;
        return itemRecord;
      }
      catch(Exception ex)
      {
        throw new WCellException(ex, "Unable to create new ItemRecord.");
      }
    }

    private void InitItemRecord()
    {
      ActiveRecordMediator.GetSessionFactoryHolder().GetConfiguration(typeof(ActiveRecordBase));
    }

    [Property(NotNull = true)]
    public int OwnerId { get; set; }

    [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
    public long Guid { get; set; }

    public uint EntityLowId
    {
      get { return (uint) Guid; }
      set { Guid = value; }
    }

    public uint EntryId
    {
      get { return (uint) _entryId; }
      set { _entryId = (int) value; }
    }

    public EntityId EntityId
    {
      get { return EntityId.GetItemId(EntityLowId); }
    }

    public uint DisplayId
    {
      get { return (uint) _displayId; }
      set { _displayId = (int) value; }
    }

    /// <summary>The slot of the container, holding this Item</summary>
    public byte ContainerSlot
    {
      get { return _containerSlot; }
      set { _containerSlot = value; }
    }

    [Property(NotNull = true)]
    public int Slot { get; set; }

    [Property(NotNull = true)]
    public long CreatorEntityId { get; set; }

    [Property(NotNull = true)]
    public long GiftCreatorEntityId { get; set; }

    [Property(NotNull = true)]
    public int Durability { get; set; }

    [Property(NotNull = true)]
    public int Duration { get; set; }

    public ItemFlags Flags
    {
      get { return (ItemFlags) flags; }
      set { flags = (int) value; }
    }

    public uint ItemTextId
    {
      get { return (uint) m_ItemTextId; }
      set { m_ItemTextId = (int) value; }
    }

    [Property]
    public string ItemText { get; set; }

    [Property(NotNull = true)]
    public int RandomProperty { get; set; }

    [Property(NotNull = true)]
    public int RandomSuffix { get; set; }

    /// <summary>Charges of the Use-spell</summary>
    [Property(NotNull = true)]
    public short Charges { get; set; }

    [Property(NotNull = true)]
    public int Amount { get; set; }

    public bool IsContainer
    {
      get { return ContSlots > 0; }
    }

    public bool IsEquippedContainer
    {
      get
      {
        if(ContSlots > 0 && _containerSlot == byte.MaxValue)
          return ItemMgr.ContainerSlotsWithBank[Slot];
        return false;
      }
    }

    public bool IsSoulbound
    {
      get { return Flags.HasFlag(ItemFlags.Soulbound); }
    }

    /// <summary>
    /// If this &gt; 0, we have a container with this amount of slots.
    /// </summary>
    [Property(NotNull = true)]
    public int ContSlots { get; set; }

    [Property]
    public int[] EnchantIds { get; set; }

    [Property]
    public int EnchantTempTime { get; set; }

    public ItemEnchantmentEntry GetEnchant(EnchantSlot slot)
    {
      if(EnchantIds != null)
        return EnchantMgr.GetEnchantmentEntry((uint) EnchantIds[(int) slot]);
      return null;
    }

    internal void SetEnchant(EnchantSlot slot, int id, int timeLeft)
    {
      if(EnchantIds == null)
        EnchantIds = new int[12];
      EnchantIds[(int) slot] = id;
      if(slot != EnchantSlot.Temporary)
        return;
      EnchantTempTime = timeLeft;
    }

    /// <summary>The life-time of the Item in seconds</summary>
    public ItemTemplate Template
    {
      get { return ItemMgr.GetTemplate(EntryId); }
    }

    public bool IsInventory
    {
      get
      {
        ItemTemplate template = Template;
        if(template == null)
          return false;
        return template.IsInventory;
      }
    }

    public bool IsOwned
    {
      get
      {
        if(!IsAuctioned)
          return MailId == 0L;
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
      return FindAll((ICriterion) Restrictions.Eq("OwnerId", (int) lowCharId));
    }

    public static ItemRecord[] LoadItemsContainersFirst(uint lowChrId)
    {
      return FindAll(ContOrder, (ICriterion) Restrictions.Eq("OwnerId", (int) lowChrId));
    }

    public static ItemRecord[] LoadAuctionedItems()
    {
      return FindAll((ICriterion) Restrictions.Eq("IsAuctioned", true));
    }

    public static ItemRecord GetRecordByID(long id)
    {
      return FindOne((ICriterion) Restrictions.Eq("Guid", id));
    }

    public static ItemRecord GetRecordByID(uint itemLowId)
    {
      return GetRecordByID((long) itemLowId);
    }

    protected void OnLoad()
    {
      ItemTemplate template = Template;
      if(template != null)
        template.NotifyCreated(this);
      else
        log.Warn("ItemRecord has invalid EntryId: " + this);
    }

    public static ItemRecord CreateRecord(ItemTemplate templ)
    {
      ItemRecord record = CreateRecord();
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
      if(templ.UseSpell != null)
        record.Charges = templ.UseSpell.Charges;
      return record;
    }

    public override string ToString()
    {
      return string.Format("ItemRecord \"{0}\" ({1}) #{2}", EntityId, _entryId,
        EntityLowId);
    }
  }
}
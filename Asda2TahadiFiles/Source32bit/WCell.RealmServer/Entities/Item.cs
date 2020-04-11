using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Database;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Items.Enchanting;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.Network;
using WCell.RealmServer.Quests;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.UpdateFields;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.Threading;

namespace WCell.RealmServer.Entities
{
  public class Item : ObjectBase, IOwned, IWeapon, INamed, ILockable, ILootable, IQuestHolder, IEntity,
    IMountableItem, IContextHandler
  {
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    public static readonly UpdateFieldCollection UpdateFieldInfos = UpdateFieldMgr.Get(ObjectTypeId.Item);
    public static readonly Item PlaceHolder = new Item();
    protected ItemTemplate m_template;
    protected internal bool m_isInWorld;

    /// <summary>
    /// Items are unknown when a creation update
    /// has not been sent to the Owner yet.
    /// </summary>
    internal bool m_unknown;

    protected internal Character m_owner;
    protected BaseInventory m_container;
    protected ItemEnchantment[] m_enchantments;
    protected IProcHandler m_hitProc;
    protected ItemRecord m_record;

    public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
    {
      get { return UpdateFieldHandler.DynamicItemFieldHandlers; }
    }

    protected override UpdateType GetCreationUpdateType(UpdateFieldFlags relation)
    {
      return UpdateType.Create;
    }

    public override UpdateFlags UpdateFlags
    {
      get { return UpdateFlags.Flag_0x10; }
    }

    public override void RequestUpdate()
    {
      OwningCharacter.AddItemToUpdate(this);
      m_requiresUpdate = true;
    }

    public override UpdateFieldFlags GetUpdateFieldVisibilityFor(Character chr)
    {
      return chr == m_owner
        ? UpdateFieldFlags.Public | UpdateFieldFlags.Private | UpdateFieldFlags.OwnerOnly |
          UpdateFieldFlags.GroupOnly
        : UpdateFieldFlags.Public;
    }

    protected override void WriteUpdateFlag_0x10(PrimitiveWriter writer, UpdateFieldFlags relation)
    {
      writer.Write(2f);
    }

    protected override UpdateFieldCollection _UpdateFieldInfos
    {
      get { return UpdateFieldInfos; }
    }

    public static Item CreateItem(uint templateId, Character owner, int amount)
    {
      ItemTemplate template = ItemMgr.GetTemplate(templateId);
      if(template != null)
        return CreateItem(template, owner, amount);
      return null;
    }

    public static Item CreateItem(Asda2ItemId templateId, Character owner, int amount)
    {
      ItemTemplate template = ItemMgr.GetTemplate(templateId);
      if(template != null)
        return CreateItem(template, owner, amount);
      return null;
    }

    public static Item CreateItem(ItemTemplate template, Character owner, int amount)
    {
      Item obj = template.Create();
      obj.InitItem(template, owner, amount);
      return obj;
    }

    public static Item CreateItem(ItemRecord record, Character owner)
    {
      ItemTemplate template = record.Template;
      if(template == null)
      {
        log.Warn("{0} had an ItemRecord with invalid ItemId: {1}", owner, record);
        return null;
      }

      Item obj = template.Create();
      obj.LoadItem(record, owner, template);
      return obj;
    }

    public static Item CreateItem(ItemRecord record, Character owner, ItemTemplate template)
    {
      Item obj = template.Create();
      obj.LoadItem(record, owner, template);
      return obj;
    }

    public static Item CreateItem(ItemRecord record, ItemTemplate template)
    {
      Item obj = template.Create();
      obj.LoadItem(record, template);
      return obj;
    }

    protected internal Item()
    {
    }

    /// <summary>Initializes a new Item</summary>
    internal void InitItem(ItemTemplate template, Character owner, int amount)
    {
      m_record = ItemRecord.CreateRecord();
      EntryId = m_record.EntryId = template.Id;
      Type |= ObjectTypes.Item;
      m_template = template;
      Durability = m_template.MaxDurability;
      MaxDurability = m_template.MaxDurability;
      Flags = m_template.Flags;
      TextId = m_template.PageTextId;
      Amount = amount;
      OwningCharacter = owner;
      EntityId = m_record.EntityId;
      if(m_template.UseSpell != null && m_template.UseSpell.HasCharges)
      {
        m_record.Charges = m_template.UseSpell.Charges;
        SetSpellCharges(m_template.UseSpell.Index, m_template.UseSpell.Charges);
      }

      template.NotifyCreated(m_record);
      OnInit();
    }

    /// <summary>Loads an already created item</summary>
    internal void LoadItem(ItemRecord record, Character owner, ItemTemplate template)
    {
      m_record = record;
      OwningCharacter = owner;
      LoadItem(record, template);
    }

    /// <summary>Loads an already created item without owner</summary>
    /// <param name="record"></param>
    /// <param name="template"></param>
    internal void LoadItem(ItemRecord record, ItemTemplate template)
    {
      m_record = record;
      EntityId = record.EntityId;
      m_template = template;
      EntryId = m_template.Id;
      Type |= ObjectTypes.Item;
      SetUInt32(ItemFields.FLAGS, (uint) record.Flags);
      SetInt32(ItemFields.DURABILITY, record.Durability);
      SetInt32(ItemFields.DURATION, record.Duration);
      SetInt32(ItemFields.STACK_COUNT, record.Amount);
      SetInt32(ItemFields.PROPERTY_SEED, record.RandomSuffix);
      SetInt32(ItemFields.RANDOM_PROPERTIES_ID, record.RandomProperty);
      SetInt64(ItemFields.CREATOR, record.CreatorEntityId);
      SetInt64(ItemFields.GIFTCREATOR, record.GiftCreatorEntityId);
      ItemText = record.ItemText;
      if(m_template.UseSpell != null)
        SetSpellCharges(m_template.UseSpell.Index, record.Charges);
      MaxDurability = m_template.MaxDurability;
      OnLoad();
    }

    /// <summary>
    /// Called after initializing a newly created Item (Owner might be null)
    /// </summary>
    protected virtual void OnInit()
    {
    }

    /// <summary>Called after loading an Item (Owner might be null)</summary>
    protected virtual void OnLoad()
    {
    }

    public ItemTemplate Template
    {
      get { return m_template; }
    }

    public LockEntry Lock
    {
      get { return m_template.Lock; }
    }

    public override bool IsInWorld
    {
      get { return m_isInWorld; }
    }

    /// <summary>Whether this object has already been deleted.</summary>
    public bool IsDeleted { get; internal set; }

    /// <summary>Checks whether this Item can currently be used</summary>
    public bool CanBeUsed
    {
      get
      {
        if(MaxDurability == 0 || Durability > 0)
          return m_loot == null;
        return false;
      }
    }

    /// <summary>The name of this item</summary>
    public string Name
    {
      get
      {
        if(m_template != null)
          return m_template.DefaultName;
        return "";
      }
    }

    public override ObjectTypeId ObjectTypeId
    {
      get { return ObjectTypeId.Item; }
    }

    public bool IsContainer
    {
      get { return ObjectTypeId == ObjectTypeId.Container; }
    }

    public bool CanBeTraded
    {
      get
      {
        if(m_template.MaxDurability != 0)
          return Durability > 0;
        return true;
      }
    }

    /// <summary>See IUsable.Owner</summary>
    public Unit Owner
    {
      get { return m_owner; }
    }

    /// <summary>Whether this Item is currently equipped.</summary>
    public bool IsEquipped
    {
      get
      {
        if(m_container == m_owner.Inventory)
          return m_record.Slot <= 22;
        return false;
      }
    }

    /// <summary>
    /// Whether this Item is currently equipped and is not a kind of container.
    /// </summary>
    public bool IsEquippedItem
    {
      get
      {
        if(m_container != null && m_container == m_owner.Inventory)
          return m_record.Slot < 19;
        return false;
      }
    }

    /// <summary>
    /// Whether this is a Container and it is currently
    /// equipped or in a bankbag slot (so Items can be put into it).
    /// </summary>
    public bool IsEquippedContainer
    {
      get
      {
        if(m_container == m_owner.Inventory)
          return ItemMgr.ContainerSlotsWithBank[Slot];
        return false;
      }
    }

    /// <summary>Wheter this item's bonuses are applied</summary>
    public bool IsApplied { get; private set; }

    public bool IsBuyback
    {
      get
      {
        if(m_record.Slot >= 74 && m_record.Slot <= 85)
          return m_container == m_owner.Inventory;
        return false;
      }
    }

    public InventorySlotTypeMask InventorySlotMask
    {
      get { return m_template.InventorySlotMask; }
    }

    /// <summary>
    /// Called when this Item was added to someone's inventory
    /// </summary>
    protected internal void OnAdd()
    {
      if(m_template.BondType == ItemBondType.OnPickup || m_template.BondType == ItemBondType.Quest)
        Flags |= ItemFlags.Soulbound;
      m_owner = m_container.Owner;
      for(EnchantSlot slot = EnchantSlot.Permanent; slot < EnchantSlot.End; ++slot)
      {
        ItemEnchantment enchantment = GetEnchantment(slot);
        if(enchantment != null)
          OnOwnerReceivedNewEnchant(enchantment);
      }
    }

    /// <summary>
    /// Saves all recent changes that were made to this Item to the DB
    /// </summary>
    public void Save()
    {
      if(IsDeleted)
        LogUtil.ErrorException(
          new InvalidOperationException("Trying to save deleted Item: " + this));
      else
        m_record.SaveAndFlush();
    }

    /// <summary>
    /// Subtracts the given amount from this item and creates a new item, with that amount.
    /// WARNING: Make sure that this item is belonging to someone and that amount is valid!
    /// </summary>
    /// <param name="amount">The amount of the newly created item</param>
    public Item Split(int amount)
    {
      Amount -= amount;
      return CreateItem(m_template, OwningCharacter, amount);
    }

    /// <summary>
    /// Creates a new Item of this type with the given amount.
    /// Usually only used on stackable Items that do not have individual
    /// properties (like durability, enchants etc).
    /// WARNING: Make sure that this item is belonging to someone and that amount is valid!
    /// </summary>
    /// <param name="amount">The amount of the newly created item</param>
    public Item CreateNew(int amount)
    {
      return CreateItem(m_template, OwningCharacter, amount);
    }

    /// <summary>TODO: Random properties</summary>
    public bool CanStackWith(Item otherItem)
    {
      if(m_template.IsStackable)
        return m_template == otherItem.m_template;
      return false;
    }

    /// <summary>A chest was looted empty</summary>
    public override void OnFinishedLooting()
    {
      Destroy();
    }

    public override uint GetLootId(Asda2LootEntryType type)
    {
      return m_template.Id;
    }

    /// <summary>
    /// All applied Enchantments.
    /// Could return null if it doesn't have any.
    /// </summary>
    public ItemEnchantment[] Enchantments
    {
      get { return m_enchantments; }
    }

    public bool HasGems
    {
      get
      {
        if(m_enchantments != null && m_template.HasSockets)
        {
          for(int index = 0; index < 3; ++index)
          {
            if(m_enchantments[index + 2] != null)
              return true;
          }
        }

        return false;
      }
    }

    public bool HasGem(Asda2ItemId id)
    {
      if(m_enchantments != null && m_template.HasSockets)
      {
        for(EnchantSlot enchantSlot = EnchantSlot.Socket1; enchantSlot <= EnchantSlot.Bonus; ++enchantSlot)
        {
          if(m_enchantments[(int) enchantSlot] != null &&
             m_enchantments[(int) enchantSlot].Entry.GemTemplate != null &&
             m_enchantments[(int) enchantSlot].Entry.GemTemplate.ItemId == id)
            return true;
        }
      }

      return false;
    }

    public bool IsEnchanted
    {
      get
      {
        if(m_enchantments[0] == null)
          return m_enchantments[1] != null;
        return true;
      }
    }

    public IEnumerable<ItemEnchantment> GetAllEnchantments()
    {
      for(EnchantSlot slot = EnchantSlot.Permanent; slot < EnchantSlot.End; ++slot)
      {
        ItemEnchantment enchant = GetEnchantment(slot);
        if(enchant != null)
          yield return enchant;
      }
    }

    private static int GetEnchantSlot(EnchantSlot slot, EnchantInfoOffset offset)
    {
      return (int) (22 + (int) slot * 3 + offset);
    }

    public void SetEnchantId(EnchantSlot slot, uint value)
    {
      SetUInt32(GetEnchantSlot(slot, EnchantInfoOffset.Id), value);
    }

    public void SetEnchantDuration(EnchantSlot slot, int value)
    {
      SetInt32(GetEnchantSlot(slot, EnchantInfoOffset.Duration) + 1, value);
    }

    public void SetEnchantCharges(EnchantSlot slot, int value)
    {
      SetInt32(GetEnchantSlot(slot, EnchantInfoOffset.Charges) + 2, value);
    }

    /// <summary>
    /// The time until the given Enchantment expires or <see cref="F:System.TimeSpan.Zero" /> if not temporary
    /// </summary>
    /// <param name="enchantSlot"></param>
    /// <returns></returns>
    public TimeSpan GetRemainingEnchantDuration(EnchantSlot enchantSlot)
    {
      return m_enchantments[(uint) enchantSlot].RemainingTime;
    }

    private void EnsureEnchantments()
    {
      if(m_enchantments != null)
        return;
      m_enchantments = new ItemEnchantment[12];
    }

    public ItemEnchantment GetEnchantment(EnchantSlot slot)
    {
      if(m_enchantments == null)
        return null;
      return m_enchantments[(uint) slot];
    }

    public void ApplyEnchant(int enchantEntryId, EnchantSlot enchantSlot, int duration, int charges, bool applyBoni)
    {
      if(enchantEntryId == 0)
        return;
      ItemEnchantmentEntry enchantmentEntry = EnchantMgr.GetEnchantmentEntry((uint) enchantEntryId);
      if(enchantmentEntry == null)
        return;
      ApplyEnchant(enchantmentEntry, enchantSlot, duration, charges, applyBoni);
    }

    /// <summary>
    /// Adds a new the <see cref="T:WCell.RealmServer.Items.Enchanting.ItemEnchantment" /> to the given Slot.
    /// Will remove any existing Enchantment in that slot.
    /// </summary>
    /// <param name="enchantSlot"></param>
    public void ApplyEnchant(ItemEnchantmentEntry enchantEntry, EnchantSlot enchantSlot, int duration, int charges,
      bool applyBoni)
    {
      if(m_enchantments == null)
        m_enchantments = new ItemEnchantment[12];
      if(m_enchantments[(int) enchantSlot] != null)
        RemoveEnchant(enchantSlot);
      ItemEnchantment enchant = new ItemEnchantment(enchantEntry, enchantSlot, DateTime.Now, duration);
      m_enchantments[(int) enchantSlot] = enchant;
      m_record.SetEnchant(enchantSlot, (int) enchant.Entry.Id, duration);
      SetEnchantId(enchantSlot, enchantEntry.Id);
      SetEnchantDuration(enchantSlot, duration);
      if(charges > 0)
        SetEnchantCharges(enchantSlot, charges - 1);
      Character owningCharacter = OwningCharacter;
      if(owningCharacter == null)
        return;
      EnchantMgr.ApplyEnchantToItem(this, enchant);
      if(enchant.Entry.GemTemplate != null)
        owningCharacter.Inventory.ModUniqueCount(enchant.Entry.GemTemplate, 1);
      OnOwnerReceivedNewEnchant(enchant);
      if(!applyBoni || !IsEquippedItem)
        return;
      SetEnchantEquipped(enchant);
    }

    /// <summary>
    /// Called when owner learns about new enchant:
    /// When enchant gets added and when receiving an enchanted item
    /// </summary>
    private void OnOwnerReceivedNewEnchant(ItemEnchantment enchant)
    {
      Character owner = OwningCharacter;
      ItemHandler.SendEnchantLog(owner, (Asda2ItemId) EntryId, enchant.Entry.Id);
      if(enchant.Duration == 0)
        return;
      int totalMilliseconds = (int) enchant.RemainingTime.TotalMilliseconds;
      owner.CallDelayed(totalMilliseconds, obj =>
      {
        if(IsDeleted || Owner != owner)
          return;
        RemoveEnchant(enchant);
      });
      ItemHandler.SendEnchantTimeUpdate(owner, this, enchant.Duration);
    }

    /// <summary>
    /// Removes the <see cref="T:WCell.RealmServer.Items.Enchanting.ItemEnchantment" /> from the given Slot.
    /// </summary>
    /// <param name="enchantSlot"></param>
    public void RemoveEnchant(EnchantSlot enchantSlot)
    {
      ItemEnchantment enchantment;
      if(m_enchantments == null || (enchantment = m_enchantments[(int) enchantSlot]) == null)
        log.Error("Tried to remove Enchantment from unoccupied EnchantmentSlot {0} on Item {1}",
          enchantSlot, this);
      else
        RemoveEnchant(enchantment);
    }

    public void RemoveEnchant(ItemEnchantment enchant)
    {
      m_enchantments[(int) enchant.Slot] = null;
      m_record.SetEnchant(enchant.Slot, 0, 0);
      Character owningCharacter = OwningCharacter;
      if(owningCharacter == null)
        return;
      EnchantMgr.RemoveEnchantFromItem(this, enchant);
      if(IsEquipped)
        SetEnchantUnequipped(enchant);
      if(enchant.Entry.GemTemplate == null)
        return;
      owningCharacter.Inventory.ModUniqueCount(enchant.Entry.GemTemplate, -1);
    }

    /// <summary>
    /// Applies the given gems to this Item.
    /// Each gem will be matched to the socket of the same index.
    /// </summary>
    /// <param name="gems"></param>
    public void ApplyGems<T>(T[] gems) where T : class, IMountableItem
    {
      if(!CheckGems(gems))
        return;
      EnsureEnchantments();
      bool flag = true;
      for(uint index = 0; (long) index < (long) gems.Length; ++index)
      {
        T gem = gems[index];
        if(gem != null && gem.Template.GemProperties != null)
        {
          if((object) gem is Item && ((Item) (object) gem).IsDeleted)
            return;
          SocketInfo socketInfo = m_template.Sockets.Get(index);
          if(socketInfo.Color != SocketColor.None)
          {
            ApplyEnchant(gem.Template.GemProperties.Enchantment, (EnchantSlot) (2 + (int) index), 0, 0,
              true);
            if((object) gem is Item)
              ((Item) (object) gem).Destroy();
            flag = flag && gem.Template.GemProperties.Color.HasAnyFlag(socketInfo.Color);
          }
        }
        else
        {
          SocketInfo socketInfo = m_template.Sockets.Get(index);
          flag = flag && (socketInfo.Color == SocketColor.None || m_enchantments[2U + index] != null &&
                          m_enchantments[2U + index].Entry.GemTemplate.GemProperties.Color
                            .HasAnyFlag(socketInfo.Color));
        }
      }

      if(flag)
      {
        if(GetEnchantment(EnchantSlot.Bonus) != null)
          return;
        ApplyEnchant(m_template.SocketBonusEnchant, EnchantSlot.Bonus, 0, 0, true);
      }
      else
      {
        if(GetEnchantment(EnchantSlot.Bonus) == null)
          return;
        RemoveEnchant(EnchantSlot.Bonus);
      }
    }

    /// <summary>
    /// Applies a set of random enchants in the prop slots between from and to
    /// </summary>
    public bool ApplyRandomEnchants(List<ItemRandomEnchantEntry> entries, EnchantSlot from, EnchantSlot to)
    {
      EnchantSlot enchantSlot = from;
      if(m_enchantments != null)
      {
        do
          ;
        while(m_enchantments[(int) enchantSlot] != null &&
              m_enchantments.Length > (int) ++enchantSlot);
        if(enchantSlot > to)
          return false;
      }

      bool flag = false;
      foreach(ItemRandomEnchantEntry entry in entries)
      {
        if(Utility.Random(0.0f, 100f) < (double) entry.ChancePercent)
        {
          ItemEnchantmentEntry enchantmentEntry = EnchantMgr.GetEnchantmentEntry(entry.EnchantId);
          if(enchantmentEntry != null)
          {
            ApplyEnchant(enchantmentEntry, enchantSlot, 0, 0, true);
            flag = true;
            do
              ;
            while(m_enchantments[(int) enchantSlot] != null && ++enchantSlot <= to);
            if(enchantSlot > to)
              return true;
          }
        }
      }

      return flag;
    }

    /// <summary>
    /// Activate bonus enchant if all sockets have matching gems.
    /// </summary>
    internal void CheckSocketColors()
    {
      if(!m_template.HasSockets || m_template.SocketBonusEnchant == null)
        return;
      bool flag = true;
      for(uint index = 0; index < 3U; ++index)
      {
        SocketInfo socketInfo = m_template.Sockets.Get(index);
        flag = flag && (socketInfo.Color == SocketColor.None || m_enchantments[2U + index] != null &&
                        (m_enchantments[2U + index].Entry.GemTemplate.GemProperties.Color &
                         socketInfo.Color) != SocketColor.None);
      }

      if(flag)
      {
        if(GetEnchantment(EnchantSlot.Bonus) != null)
          return;
        ApplyEnchant(m_template.SocketBonusEnchant, EnchantSlot.Bonus, 0, 0, false);
      }
      else
      {
        if(GetEnchantment(EnchantSlot.Bonus) == null)
          return;
        RemoveEnchant(EnchantSlot.Bonus);
      }
    }

    /// <summary>
    /// Check whether the given gems match the color of the socket of the corresponding index within
    /// the gems-array.
    /// Check for unique count.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="gems"></param>
    /// <returns></returns>
    private bool CheckGems<T>(T[] gems) where T : class, IMountableItem
    {
      for(uint index = 0; (long) index < (long) gems.Length; ++index)
      {
        T gem = gems[index];
        if(gem != null)
        {
          SocketInfo socketInfo = m_template.Sockets.Get(index);
          if(socketInfo.Color != SocketColor.None)
          {
            if(socketInfo.Color == SocketColor.Meta !=
               (gem.Template.GemProperties.Color == SocketColor.Meta))
              return false;
            if(IsEquipped && !m_owner.Inventory.CheckEquippedGems(gem.Template))
            {
              ItemHandler.SendInventoryError(m_owner, this, null,
                InventoryError.ITEM_MAX_COUNT_EQUIPPED_SOCKETED);
              return false;
            }
          }
        }
      }

      return true;
    }

    private void SetEnchantEquipped(ItemEnchantment enchant)
    {
      if(enchant.Slot == EnchantSlot.Permanent)
        Owner.SetUInt16Low((PlayerFields) (284 + Slot * 2),
          (ushort) enchant.Entry.Id);
      else if(enchant.Slot == EnchantSlot.Temporary)
        Owner.SetUInt16High((PlayerFields) (284 + Slot * 2),
          (ushort) enchant.Entry.Id);
      for(int index = 0; index < enchant.Entry.Effects.Length; ++index)
        EnchantMgr.ApplyEquippedEffect(this, enchant.Entry.Effects[index]);
    }

    private void SetEnchantUnequipped(ItemEnchantment enchant)
    {
      if(enchant.Slot == EnchantSlot.Permanent)
        Owner.SetUInt16Low((PlayerFields) (284 + Slot * 2), 0);
      else if(enchant.Slot == EnchantSlot.Temporary)
        Owner.SetUInt16High((PlayerFields) (284 + Slot * 2), 0);
      for(int index = 0; index < enchant.Entry.Effects.Length; ++index)
        EnchantMgr.RemoveEffect(this, enchant.Entry.Effects[index]);
    }

    /// <summary>Tries to equip this Item</summary>
    public InventoryError Equip()
    {
      return m_owner.Inventory.TryEquip(m_container, Slot);
    }

    public bool Unequip()
    {
      PlayerInventory inventory = m_owner.Inventory;
      SimpleSlotId freeSlot = inventory.FindFreeSlot(this, Amount);
      if(freeSlot.Slot == byte.MaxValue)
        return false;
      inventory.SwapUnchecked(m_container, Slot, freeSlot.Container, freeSlot.Slot);
      return true;
    }

    public InventoryError CheckEquip(Character user)
    {
      return m_template.CheckEquip(user);
    }

    internal void OnEquipDecision()
    {
      OnEquip();
    }

    internal void OnUnequipDecision(InventorySlot slot)
    {
      if(m_template.IsWeapon && slot == InventorySlot.AvLeftHead)
        m_owner.MainWeapon = null;
      else
        OnUnEquip(slot);
    }

    /// <summary>
    /// Called when this Item gets equipped.
    /// Requires map context.
    /// </summary>
    public void OnEquip()
    {
      if(IsApplied)
        return;
      IsApplied = true;
      InventorySlot slot = (InventorySlot) Slot;
      Character owningCharacter = OwningCharacter;
      if(slot < InventorySlot.Bag1)
      {
        int num = m_template.IsAmmo ? 1 : 0;
      }

      m_template.ApplyStatMods(owningCharacter);
      if(m_template.BondType == ItemBondType.OnEquip)
        Flags |= ItemFlags.Soulbound;
      if(owningCharacter.IsUsingSpell)
        owningCharacter.SpellCast.Cancel(SpellFailedReason.Interrupted);
      for(int index = 0; index < m_template.Resistances.Length; ++index)
      {
        int resistance = m_template.Resistances[index];
        if(resistance > 0)
          owningCharacter.ModBaseResistance((DamageSchool) index, resistance);
      }

      if(slot == InventorySlot.Invalid)
        owningCharacter.UpdateRangedDamage();
      else if(m_template.InventorySlotType == InventorySlotType.Shield)
        owningCharacter.UpdateBlockChance();
      if(m_template.EquipSpells != null)
        owningCharacter.SpellCast.TriggerAll(owningCharacter, m_template.EquipSpells);
      if(m_template.Set != null)
      {
        Spell[] spellArray =
          m_template.Set.Boni.Get(owningCharacter.Inventory.GetSetCount(m_template.Set) -
                                  1U);
        if(spellArray != null)
          owningCharacter.SpellCast.TriggerAll(owningCharacter, spellArray);
      }

      m_owner.PlayerAuras.OnEquip(this);
      if(m_owner.Inventory.m_ItemEquipmentEventHandlers != null)
      {
        foreach(IItemEquipmentEventHandler equipmentEventHandler in m_owner.Inventory
          .m_ItemEquipmentEventHandlers)
          equipmentEventHandler.OnEquip(this);
      }

      m_template.NotifyEquip(this);
    }

    /// <summary>
    /// Called when this Item gets unequipped.
    /// Requires map context.
    /// </summary>
    public void OnUnEquip(InventorySlot slot)
    {
      if(!IsApplied)
        return;
      IsApplied = false;
      if(!m_template.IsAmmo)
        m_owner.SetVisibleItem(slot, null);
      m_template.RemoveStatMods(m_owner);
      if(m_template.EquipSpells != null)
      {
        foreach(Spell equipSpell in m_template.EquipSpells)
        {
          if(equipSpell.IsAura)
            m_owner.Auras.Remove(equipSpell);
        }
      }

      for(int index = 0; index < m_template.Resistances.Length; ++index)
      {
        int resistance = m_template.Resistances[index];
        if(resistance > 0)
          m_owner.ModBaseResistance((DamageSchool) index, -resistance);
      }

      if(slot == InventorySlot.Invalid)
        m_owner.UpdateRangedDamage();
      else if(m_template.InventorySlotType == InventorySlotType.Shield)
        m_owner.UpdateBlockChance();
      if(m_template.Set != null)
      {
        Spell[] spellArray =
          m_template.Set.Boni.Get(m_owner.Inventory.GetSetCount(m_template.Set) - 1U);
        if(spellArray != null)
        {
          foreach(Spell index in spellArray)
          {
            Aura aura = m_owner.Auras[index, true];
            if(aura != null)
              aura.Remove(false);
          }
        }
      }

      if(m_hitProc != null)
      {
        m_owner.RemoveProcHandler(m_hitProc);
        m_hitProc = null;
      }

      m_owner.PlayerAuras.OnBeforeUnEquip(this);
      if(m_owner.Inventory.m_ItemEquipmentEventHandlers != null)
      {
        foreach(IItemEquipmentEventHandler equipmentEventHandler in m_owner.Inventory
          .m_ItemEquipmentEventHandlers)
          equipmentEventHandler.OnBeforeUnEquip(this);
      }

      m_template.NotifyUnequip(this);
    }

    /// <summary>
    /// Called whenever an item is used.
    /// Make sure to only call on Items whose Template has a UseSpell.
    /// </summary>
    internal void OnUse()
    {
      if(m_template.BondType == ItemBondType.OnUse)
        Flags |= ItemFlags.Soulbound;
      if(m_template.UseSpell != null && m_template.UseSpell.HasCharges)
        SpellCharges = SpellCharges < 0 ? SpellCharges + 1 : SpellCharges - 1;
      m_template.NotifyUsed(this);
    }

    /// <summary>
    /// Destroys the Item without further checks.
    /// Also destroys all contained Items if this is a Container.
    /// </summary>
    public void Destroy()
    {
      if(m_container != null && m_container.IsValidSlot(Slot))
        m_container.Destroy(Slot);
      else
        DoDestroy();
    }

    /// <summary>Called by the container to</summary>
    protected internal virtual void DoDestroy()
    {
      ItemRecord record = m_record;
      m_owner.Inventory.OnAmountChanged(this, -Amount);
      if(record == null)
        return;
      record.OwnerId = 0;
      record.DeleteLater();
      m_record = null;
      Dispose();
    }

    /// <summary>
    /// Removes this Item from its old Container (if it was added to any).
    /// After calling this method,
    /// make sure to either Dispose the item after removing (in this case you can also simply use <see cref="M:WCell.RealmServer.Entities.Item.Destroy" />
    /// or re-add it somewhere else.
    /// </summary>
    public void Remove(bool ownerChange)
    {
      if(m_container == null)
        return;
      m_container.Remove(this, ownerChange);
    }

    public QuestHolderInfo QuestHolderInfo
    {
      get { return m_template.QuestHolderInfo; }
    }

    public bool CanGiveQuestTo(Character chr)
    {
      return m_owner == chr;
    }

    public void OnQuestGiverStatusQuery(Character chr)
    {
    }

    public override void Dispose(bool disposing)
    {
      m_owner = null;
      m_isInWorld = false;
      IsDeleted = true;
    }

    public override string ToString()
    {
      return string.Format("{0}{1} in Slot {4} (Templ: {2}, Id: {3})",
        Amount != 1 ? (object) (Amount + "x ") : (object) "",
        (object) Template.DefaultName, (object) m_template.Id, (object) EntityId,
        (object) Slot);
    }

    public bool IsInContext
    {
      get
      {
        Unit owner = Owner;
        if(owner != null)
        {
          IContextHandler contextHandler = owner.ContextHandler;
          if(contextHandler != null)
            return contextHandler.IsInContext;
        }

        return false;
      }
    }

    public void AddMessage(IMessage message)
    {
      Unit owner = Owner;
      if(owner == null)
        return;
      owner.AddMessage(message);
    }

    public void AddMessage(Action action)
    {
      Unit owner = Owner;
      if(owner == null)
        return;
      owner.AddMessage(action);
    }

    public bool ExecuteInContext(Action action)
    {
      Unit owner = Owner;
      if(owner != null)
        return owner.ExecuteInContext(action);
      return false;
    }

    public void EnsureContext()
    {
      Unit owner = Owner;
      if(owner == null)
        return;
      owner.EnsureContext();
    }

    public Character OwningCharacter
    {
      get { return m_owner; }
      internal set
      {
        m_owner = value;
        if(m_owner != null)
        {
          m_isInWorld = m_unknown = true;
          SetEntityId(ItemFields.OWNER, value.EntityId);
          m_record.OwnerId = (int) value.EntityId.Low;
        }
        else
        {
          SetEntityId(ItemFields.OWNER, EntityId.Zero);
          m_record.OwnerId = 0;
        }
      }
    }

    /// <summary>
    /// The Inventory of the Container that contains this Item
    /// </summary>
    public BaseInventory Container
    {
      get { return m_container; }
      internal set
      {
        if(m_container == value)
          return;
        if(value != null)
        {
          IContainer container = value.Container;
          SetEntityId(ItemFields.CONTAINED, container.EntityId);
          m_record.ContainerSlot = container.BaseInventory.Slot;
        }
        else
        {
          SetEntityId(ItemFields.CONTAINED, EntityId.Zero);
          m_record.ContainerSlot = 0;
        }

        m_container = value;
      }
    }

    /// <summary>The life-time of this Item in seconds</summary>
    public EntityId Creator
    {
      get { return new EntityId((ulong) m_record.CreatorEntityId); }
      set
      {
        SetEntityId(ItemFields.CREATOR, value);
        m_record.CreatorEntityId = (long) value.Full;
      }
    }

    public EntityId GiftCreator
    {
      get { return new EntityId((ulong) m_record.GiftCreatorEntityId); }
      set
      {
        SetEntityId(ItemFields.GIFTCREATOR, value);
        m_record.GiftCreatorEntityId = (long) value.Full;
      }
    }

    /// <summary>
    /// The Slot of this Item within its <see cref="P:WCell.RealmServer.Entities.Item.Container">Container</see>.
    /// </summary>
    public int Slot
    {
      get { return m_record.Slot; }
      internal set { m_record.Slot = value; }
    }

    /// <summary>
    /// Modifies the amount of this item (size of this stack).
    /// Ensures that new value won't exceed UniqueCount.
    /// Returns how many items actually got added.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public int ModAmount(int value)
    {
      if(value == 0)
        return 0;
      if(m_owner != null)
      {
        if(value > 0 && m_template.UniqueCount > 0)
        {
          int uniqueCount = m_owner.Inventory.GetUniqueCount(m_template.ItemId);
          if(value > uniqueCount)
            value = uniqueCount;
        }

        m_owner.Inventory.OnAmountChanged(this, value);
      }

      m_record.Amount += value;
      SetInt32(ItemFields.STACK_COUNT, m_record.Amount);
      return value;
    }

    /// <summary>
    /// Current amount of items in this stack.
    /// Setting the Amount to 0 will destroy the Item.
    /// Keep in mind that this is uint and thus can never become smaller than 0!
    /// </summary>
    public int Amount
    {
      get { return m_record.Amount; }
      set
      {
        if(value <= 0)
        {
          Destroy();
        }
        else
        {
          int diff = value - m_record.Amount;
          if(diff == 0)
            return;
          if(m_owner != null)
            m_owner.Inventory.OnAmountChanged(this, diff);
          SetInt32(ItemFields.STACK_COUNT, value);
          m_record.Amount = value;
        }
      }
    }

    public uint Duration
    {
      get { return (uint) m_record.Duration; }
      set
      {
        SetUInt32(ItemFields.DURATION, value);
        m_record.Duration = (int) value;
      }
    }

    /// <summary>
    /// Charges of the <c>UseSpell</c> of this Item.
    /// </summary>
    public int SpellCharges
    {
      get { return m_record.Charges; }
      set
      {
        if(value == 0 && m_record.Charges < 0)
        {
          Destroy();
        }
        else
        {
          m_record.Charges = (short) value;
          if(m_template.UseSpell == null)
            return;
          SetSpellCharges(m_template.UseSpell.Index, value);
        }
      }
    }

    public uint GetSpellCharges(uint index)
    {
      return GetUInt32((ItemFields) (16 + (int) index));
    }

    public void ModSpellCharges(uint index, int delta)
    {
      SetUInt32(16 + (int) index, (uint) (GetSpellCharges(index) + (ulong) delta));
    }

    public void SetSpellCharges(uint index, int value)
    {
      SetUInt32(16 + (int) index, (uint) Math.Abs(value));
    }

    public ItemFlags Flags
    {
      get { return m_record.Flags; }
      set
      {
        SetUInt32(ItemFields.FLAGS, (uint) value);
        m_record.Flags = value;
      }
    }

    public bool IsAuctioned
    {
      get { return m_record.IsAuctioned; }
      set { m_record.IsAuctioned = true; }
    }

    public bool IsSoulbound
    {
      get { return Flags.HasFlag(ItemFlags.Soulbound); }
    }

    public bool IsGiftWrapped
    {
      get { return Flags.HasFlag(ItemFlags.GiftWrapped); }
    }

    public bool IsConjured
    {
      get { return Flags.HasFlag(ItemFlags.Conjured); }
    }

    public uint PropertySeed
    {
      get { return GetUInt32(ItemFields.PROPERTY_SEED); }
      set
      {
        SetUInt32(ItemFields.PROPERTY_SEED, value);
        m_record.RandomSuffix = (int) value;
      }
    }

    public uint RandomPropertiesId
    {
      get { return (uint) m_record.RandomProperty; }
      set
      {
        SetUInt32(ItemFields.RANDOM_PROPERTIES_ID, value);
        m_record.RandomProperty = (int) value;
      }
    }

    public int Durability
    {
      get { return m_record.Durability; }
      set
      {
        SetInt32(ItemFields.DURABILITY, value);
        m_record.Durability = value;
      }
    }

    public int MaxDurability
    {
      get { return GetInt32(ItemFields.MAXDURABILITY); }
      protected set { SetInt32(ItemFields.MAXDURABILITY, value); }
    }

    public void RepairDurability()
    {
      Durability = MaxDurability;
    }

    public uint TextId
    {
      get { return m_record.ItemTextId; }
      internal set { m_record.ItemTextId = value; }
    }

    public string ItemText
    {
      get { return m_record.ItemText; }
      internal set { m_record.ItemText = value; }
    }

    public DamageInfo[] Damages
    {
      get { return m_template.Damages; }
    }

    public int BonusDamage { get; set; }

    public SkillId Skill
    {
      get { return m_template.ItemProfession; }
    }

    public bool IsRanged
    {
      get { return m_template.RangeModifier > 0.0; }
    }

    public bool IsMelee
    {
      get { return m_template.RangeModifier == 0.0; }
    }

    /// <summary>
    /// The minimum Range of this weapon
    /// TODO: temporary values
    /// </summary>
    public float MinRange
    {
      get
      {
        if(IsMelee)
          return 0.0f;
        return Unit.DefaultMeleeAttackRange;
      }
    }

    /// <summary>
    /// The maximum Range of this Weapon
    /// TODO: temporary values
    /// </summary>
    public float MaxRange
    {
      get
      {
        if(IsMelee)
          return Unit.DefaultMeleeAttackRange;
        return Unit.DefaultRangedAttackRange;
      }
    }

    /// <summary>The time in milliseconds between 2 attacks</summary>
    public int AttackTime
    {
      get { return m_template.AttackTime; }
    }

    public ItemRecord Record
    {
      get { return m_record; }
    }

    public override ObjectTypeCustom CustomType
    {
      get { return ObjectTypeCustom.Object | ObjectTypeCustom.Item; }
    }

    public short Soul1Id { get; set; }

    public short Soul2Id { get; set; }

    public short Soul3Id { get; set; }

    public short Soul4Id { get; set; }

    public short Enchant { get; set; }

    public short Parametr1Type { get; set; }

    public short Parametr1Value { get; set; }

    public short Parametr2Type { get; set; }

    public short Parametr2Value { get; set; }

    public short Parametr3Type { get; set; }

    public short Parametr3Value { get; set; }

    public short Parametr4Type { get; set; }

    public short Parametr4Value { get; set; }

    public short Parametr5Type { get; set; }

    public short Parametr5Value { get; set; }
  }
}
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
            this.OwningCharacter.AddItemToUpdate(this);
            this.m_requiresUpdate = true;
        }

        public override UpdateFieldFlags GetUpdateFieldVisibilityFor(Character chr)
        {
            return chr == this.m_owner
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
            get { return Item.UpdateFieldInfos; }
        }

        public static Item CreateItem(uint templateId, Character owner, int amount)
        {
            ItemTemplate template = ItemMgr.GetTemplate(templateId);
            if (template != null)
                return Item.CreateItem(template, owner, amount);
            return (Item) null;
        }

        public static Item CreateItem(Asda2ItemId templateId, Character owner, int amount)
        {
            ItemTemplate template = ItemMgr.GetTemplate(templateId);
            if (template != null)
                return Item.CreateItem(template, owner, amount);
            return (Item) null;
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
            if (template == null)
            {
                Item.log.Warn("{0} had an ItemRecord with invalid ItemId: {1}", (object) owner, (object) record);
                return (Item) null;
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
            this.m_record = ItemRecord.CreateRecord();
            this.EntryId = this.m_record.EntryId = template.Id;
            this.Type |= ObjectTypes.Item;
            this.m_template = template;
            this.Durability = this.m_template.MaxDurability;
            this.MaxDurability = this.m_template.MaxDurability;
            this.Flags = this.m_template.Flags;
            this.TextId = this.m_template.PageTextId;
            this.Amount = amount;
            this.OwningCharacter = owner;
            this.EntityId = this.m_record.EntityId;
            if (this.m_template.UseSpell != null && this.m_template.UseSpell.HasCharges)
            {
                this.m_record.Charges = this.m_template.UseSpell.Charges;
                this.SetSpellCharges(this.m_template.UseSpell.Index, (int) this.m_template.UseSpell.Charges);
            }

            template.NotifyCreated(this.m_record);
            this.OnInit();
        }

        /// <summary>Loads an already created item</summary>
        internal void LoadItem(ItemRecord record, Character owner, ItemTemplate template)
        {
            this.m_record = record;
            this.OwningCharacter = owner;
            this.LoadItem(record, template);
        }

        /// <summary>Loads an already created item without owner</summary>
        /// <param name="record"></param>
        /// <param name="template"></param>
        internal void LoadItem(ItemRecord record, ItemTemplate template)
        {
            this.m_record = record;
            this.EntityId = record.EntityId;
            this.m_template = template;
            this.EntryId = this.m_template.Id;
            this.Type |= ObjectTypes.Item;
            this.SetUInt32((UpdateFieldId) ItemFields.FLAGS, (uint) record.Flags);
            this.SetInt32((UpdateFieldId) ItemFields.DURABILITY, record.Durability);
            this.SetInt32((UpdateFieldId) ItemFields.DURATION, record.Duration);
            this.SetInt32((UpdateFieldId) ItemFields.STACK_COUNT, record.Amount);
            this.SetInt32((UpdateFieldId) ItemFields.PROPERTY_SEED, record.RandomSuffix);
            this.SetInt32((UpdateFieldId) ItemFields.RANDOM_PROPERTIES_ID, record.RandomProperty);
            this.SetInt64((UpdateFieldId) ItemFields.CREATOR, record.CreatorEntityId);
            this.SetInt64((UpdateFieldId) ItemFields.GIFTCREATOR, record.GiftCreatorEntityId);
            this.ItemText = record.ItemText;
            if (this.m_template.UseSpell != null)
                this.SetSpellCharges(this.m_template.UseSpell.Index, (int) record.Charges);
            this.MaxDurability = this.m_template.MaxDurability;
            this.OnLoad();
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
            get { return this.m_template; }
        }

        public LockEntry Lock
        {
            get { return this.m_template.Lock; }
        }

        public override bool IsInWorld
        {
            get { return this.m_isInWorld; }
        }

        /// <summary>Whether this object has already been deleted.</summary>
        public bool IsDeleted { get; internal set; }

        /// <summary>Checks whether this Item can currently be used</summary>
        public bool CanBeUsed
        {
            get
            {
                if (this.MaxDurability == 0 || this.Durability > 0)
                    return this.m_loot == null;
                return false;
            }
        }

        /// <summary>The name of this item</summary>
        public string Name
        {
            get
            {
                if (this.m_template != null)
                    return this.m_template.DefaultName;
                return "";
            }
        }

        public override ObjectTypeId ObjectTypeId
        {
            get { return ObjectTypeId.Item; }
        }

        public bool IsContainer
        {
            get { return this.ObjectTypeId == ObjectTypeId.Container; }
        }

        public bool CanBeTraded
        {
            get
            {
                if (this.m_template.MaxDurability != 0)
                    return this.Durability > 0;
                return true;
            }
        }

        /// <summary>See IUsable.Owner</summary>
        public Unit Owner
        {
            get { return (Unit) this.m_owner; }
        }

        /// <summary>Whether this Item is currently equipped.</summary>
        public bool IsEquipped
        {
            get
            {
                if (this.m_container == this.m_owner.Inventory)
                    return this.m_record.Slot <= 22;
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
                if (this.m_container != null && this.m_container == this.m_owner.Inventory)
                    return this.m_record.Slot < 19;
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
                if (this.m_container == this.m_owner.Inventory)
                    return ItemMgr.ContainerSlotsWithBank[this.Slot];
                return false;
            }
        }

        /// <summary>Wheter this item's bonuses are applied</summary>
        public bool IsApplied { get; private set; }

        public bool IsBuyback
        {
            get
            {
                if (this.m_record.Slot >= 74 && this.m_record.Slot <= 85)
                    return this.m_container == this.m_owner.Inventory;
                return false;
            }
        }

        public InventorySlotTypeMask InventorySlotMask
        {
            get { return this.m_template.InventorySlotMask; }
        }

        /// <summary>
        /// Called when this Item was added to someone's inventory
        /// </summary>
        protected internal void OnAdd()
        {
            if (this.m_template.BondType == ItemBondType.OnPickup || this.m_template.BondType == ItemBondType.Quest)
                this.Flags |= ItemFlags.Soulbound;
            this.m_owner = this.m_container.Owner;
            for (EnchantSlot slot = EnchantSlot.Permanent; slot < EnchantSlot.End; ++slot)
            {
                ItemEnchantment enchantment = this.GetEnchantment(slot);
                if (enchantment != null)
                    this.OnOwnerReceivedNewEnchant(enchantment);
            }
        }

        /// <summary>
        /// Saves all recent changes that were made to this Item to the DB
        /// </summary>
        public void Save()
        {
            if (this.IsDeleted)
                LogUtil.ErrorException(
                    (Exception) new InvalidOperationException("Trying to save deleted Item: " + (object) this));
            else
                this.m_record.SaveAndFlush();
        }

        /// <summary>
        /// Subtracts the given amount from this item and creates a new item, with that amount.
        /// WARNING: Make sure that this item is belonging to someone and that amount is valid!
        /// </summary>
        /// <param name="amount">The amount of the newly created item</param>
        public Item Split(int amount)
        {
            this.Amount -= amount;
            return Item.CreateItem(this.m_template, this.OwningCharacter, amount);
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
            return Item.CreateItem(this.m_template, this.OwningCharacter, amount);
        }

        /// <summary>TODO: Random properties</summary>
        public bool CanStackWith(Item otherItem)
        {
            if (this.m_template.IsStackable)
                return this.m_template == otherItem.m_template;
            return false;
        }

        /// <summary>A chest was looted empty</summary>
        public override void OnFinishedLooting()
        {
            this.Destroy();
        }

        public override uint GetLootId(Asda2LootEntryType type)
        {
            return this.m_template.Id;
        }

        /// <summary>
        /// All applied Enchantments.
        /// Could return null if it doesn't have any.
        /// </summary>
        public ItemEnchantment[] Enchantments
        {
            get { return this.m_enchantments; }
        }

        public bool HasGems
        {
            get
            {
                if (this.m_enchantments != null && this.m_template.HasSockets)
                {
                    for (int index = 0; index < 3; ++index)
                    {
                        if (this.m_enchantments[index + 2] != null)
                            return true;
                    }
                }

                return false;
            }
        }

        public bool HasGem(Asda2ItemId id)
        {
            if (this.m_enchantments != null && this.m_template.HasSockets)
            {
                for (EnchantSlot enchantSlot = EnchantSlot.Socket1; enchantSlot <= EnchantSlot.Bonus; ++enchantSlot)
                {
                    if (this.m_enchantments[(int) enchantSlot] != null &&
                        this.m_enchantments[(int) enchantSlot].Entry.GemTemplate != null &&
                        this.m_enchantments[(int) enchantSlot].Entry.GemTemplate.ItemId == id)
                        return true;
                }
            }

            return false;
        }

        public bool IsEnchanted
        {
            get
            {
                if (this.m_enchantments[0] == null)
                    return this.m_enchantments[1] != null;
                return true;
            }
        }

        public IEnumerable<ItemEnchantment> GetAllEnchantments()
        {
            for (EnchantSlot slot = EnchantSlot.Permanent; slot < EnchantSlot.End; ++slot)
            {
                ItemEnchantment enchant = this.GetEnchantment(slot);
                if (enchant != null)
                    yield return enchant;
            }
        }

        private static int GetEnchantSlot(EnchantSlot slot, EnchantInfoOffset offset)
        {
            return (int) (22 + (int) slot * 3 + offset);
        }

        public void SetEnchantId(EnchantSlot slot, uint value)
        {
            this.SetUInt32(Item.GetEnchantSlot(slot, EnchantInfoOffset.Id), value);
        }

        public void SetEnchantDuration(EnchantSlot slot, int value)
        {
            this.SetInt32(Item.GetEnchantSlot(slot, EnchantInfoOffset.Duration) + 1, value);
        }

        public void SetEnchantCharges(EnchantSlot slot, int value)
        {
            this.SetInt32(Item.GetEnchantSlot(slot, EnchantInfoOffset.Charges) + 2, value);
        }

        /// <summary>
        /// The time until the given Enchantment expires or <see cref="F:System.TimeSpan.Zero" /> if not temporary
        /// </summary>
        /// <param name="enchantSlot"></param>
        /// <returns></returns>
        public TimeSpan GetRemainingEnchantDuration(EnchantSlot enchantSlot)
        {
            return this.m_enchantments[(uint) enchantSlot].RemainingTime;
        }

        private void EnsureEnchantments()
        {
            if (this.m_enchantments != null)
                return;
            this.m_enchantments = new ItemEnchantment[12];
        }

        public ItemEnchantment GetEnchantment(EnchantSlot slot)
        {
            if (this.m_enchantments == null)
                return (ItemEnchantment) null;
            return this.m_enchantments[(uint) slot];
        }

        public void ApplyEnchant(int enchantEntryId, EnchantSlot enchantSlot, int duration, int charges, bool applyBoni)
        {
            if (enchantEntryId == 0)
                return;
            ItemEnchantmentEntry enchantmentEntry = EnchantMgr.GetEnchantmentEntry((uint) enchantEntryId);
            if (enchantmentEntry == null)
                return;
            this.ApplyEnchant(enchantmentEntry, enchantSlot, duration, charges, applyBoni);
        }

        /// <summary>
        /// Adds a new the <see cref="T:WCell.RealmServer.Items.Enchanting.ItemEnchantment" /> to the given Slot.
        /// Will remove any existing Enchantment in that slot.
        /// </summary>
        /// <param name="enchantSlot"></param>
        public void ApplyEnchant(ItemEnchantmentEntry enchantEntry, EnchantSlot enchantSlot, int duration, int charges,
            bool applyBoni)
        {
            if (this.m_enchantments == null)
                this.m_enchantments = new ItemEnchantment[12];
            if (this.m_enchantments[(int) enchantSlot] != null)
                this.RemoveEnchant(enchantSlot);
            ItemEnchantment enchant = new ItemEnchantment(enchantEntry, enchantSlot, DateTime.Now, duration);
            this.m_enchantments[(int) enchantSlot] = enchant;
            this.m_record.SetEnchant(enchantSlot, (int) enchant.Entry.Id, duration);
            this.SetEnchantId(enchantSlot, enchantEntry.Id);
            this.SetEnchantDuration(enchantSlot, duration);
            if (charges > 0)
                this.SetEnchantCharges(enchantSlot, charges - 1);
            Character owningCharacter = this.OwningCharacter;
            if (owningCharacter == null)
                return;
            EnchantMgr.ApplyEnchantToItem(this, enchant);
            if (enchant.Entry.GemTemplate != null)
                owningCharacter.Inventory.ModUniqueCount(enchant.Entry.GemTemplate, 1);
            this.OnOwnerReceivedNewEnchant(enchant);
            if (!applyBoni || !this.IsEquippedItem)
                return;
            this.SetEnchantEquipped(enchant);
        }

        /// <summary>
        /// Called when owner learns about new enchant:
        /// When enchant gets added and when receiving an enchanted item
        /// </summary>
        private void OnOwnerReceivedNewEnchant(ItemEnchantment enchant)
        {
            Character owner = this.OwningCharacter;
            ItemHandler.SendEnchantLog((IPacketReceivingEntity) owner, (Asda2ItemId) this.EntryId, enchant.Entry.Id);
            if (enchant.Duration == 0)
                return;
            int totalMilliseconds = (int) enchant.RemainingTime.TotalMilliseconds;
            owner.CallDelayed(totalMilliseconds, (Action<WorldObject>) (obj =>
            {
                if (this.IsDeleted || this.Owner != owner)
                    return;
                this.RemoveEnchant(enchant);
            }));
            ItemHandler.SendEnchantTimeUpdate((IPacketReceivingEntity) owner, this, enchant.Duration);
        }

        /// <summary>
        /// Removes the <see cref="T:WCell.RealmServer.Items.Enchanting.ItemEnchantment" /> from the given Slot.
        /// </summary>
        /// <param name="enchantSlot"></param>
        public void RemoveEnchant(EnchantSlot enchantSlot)
        {
            ItemEnchantment enchantment;
            if (this.m_enchantments == null || (enchantment = this.m_enchantments[(int) enchantSlot]) == null)
                Item.log.Error("Tried to remove Enchantment from unoccupied EnchantmentSlot {0} on Item {1}",
                    (object) enchantSlot, (object) this);
            else
                this.RemoveEnchant(enchantment);
        }

        public void RemoveEnchant(ItemEnchantment enchant)
        {
            this.m_enchantments[(int) enchant.Slot] = (ItemEnchantment) null;
            this.m_record.SetEnchant(enchant.Slot, 0, 0);
            Character owningCharacter = this.OwningCharacter;
            if (owningCharacter == null)
                return;
            EnchantMgr.RemoveEnchantFromItem(this, enchant);
            if (this.IsEquipped)
                this.SetEnchantUnequipped(enchant);
            if (enchant.Entry.GemTemplate == null)
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
            if (!this.CheckGems<T>(gems))
                return;
            this.EnsureEnchantments();
            bool flag = true;
            for (uint index = 0; (long) index < (long) gems.Length; ++index)
            {
                T gem = gems[index];
                if ((object) gem != null && gem.Template.GemProperties != null)
                {
                    if ((object) gem is Item && ((Item) (object) gem).IsDeleted)
                        return;
                    SocketInfo socketInfo = this.m_template.Sockets.Get<SocketInfo>(index);
                    if (socketInfo.Color != SocketColor.None)
                    {
                        this.ApplyEnchant(gem.Template.GemProperties.Enchantment, (EnchantSlot) (2 + (int) index), 0, 0,
                            true);
                        if ((object) gem is Item)
                            ((Item) (object) gem).Destroy();
                        flag = flag && gem.Template.GemProperties.Color.HasAnyFlag(socketInfo.Color);
                    }
                }
                else
                {
                    SocketInfo socketInfo = this.m_template.Sockets.Get<SocketInfo>(index);
                    flag = flag && (socketInfo.Color == SocketColor.None || this.m_enchantments[2U + index] != null &&
                                    this.m_enchantments[2U + index].Entry.GemTemplate.GemProperties.Color
                                        .HasAnyFlag(socketInfo.Color));
                }
            }

            if (flag)
            {
                if (this.GetEnchantment(EnchantSlot.Bonus) != null)
                    return;
                this.ApplyEnchant(this.m_template.SocketBonusEnchant, EnchantSlot.Bonus, 0, 0, true);
            }
            else
            {
                if (this.GetEnchantment(EnchantSlot.Bonus) == null)
                    return;
                this.RemoveEnchant(EnchantSlot.Bonus);
            }
        }

        /// <summary>
        /// Applies a set of random enchants in the prop slots between from and to
        /// </summary>
        public bool ApplyRandomEnchants(List<ItemRandomEnchantEntry> entries, EnchantSlot from, EnchantSlot to)
        {
            EnchantSlot enchantSlot = from;
            if (this.m_enchantments != null)
            {
                do
                    ;
                while (this.m_enchantments[(int) enchantSlot] != null &&
                       this.m_enchantments.Length > (int) ++enchantSlot);
                if (enchantSlot > to)
                    return false;
            }

            bool flag = false;
            foreach (ItemRandomEnchantEntry entry in entries)
            {
                if ((double) Utility.Random(0.0f, 100f) < (double) entry.ChancePercent)
                {
                    ItemEnchantmentEntry enchantmentEntry = EnchantMgr.GetEnchantmentEntry(entry.EnchantId);
                    if (enchantmentEntry != null)
                    {
                        this.ApplyEnchant(enchantmentEntry, enchantSlot, 0, 0, true);
                        flag = true;
                        do
                            ;
                        while (this.m_enchantments[(int) enchantSlot] != null && ++enchantSlot <= to);
                        if (enchantSlot > to)
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
            if (!this.m_template.HasSockets || this.m_template.SocketBonusEnchant == null)
                return;
            bool flag = true;
            for (uint index = 0; index < 3U; ++index)
            {
                SocketInfo socketInfo = this.m_template.Sockets.Get<SocketInfo>(index);
                flag = flag && (socketInfo.Color == SocketColor.None || this.m_enchantments[2U + index] != null &&
                                (this.m_enchantments[2U + index].Entry.GemTemplate.GemProperties.Color &
                                 socketInfo.Color) != SocketColor.None);
            }

            if (flag)
            {
                if (this.GetEnchantment(EnchantSlot.Bonus) != null)
                    return;
                this.ApplyEnchant(this.m_template.SocketBonusEnchant, EnchantSlot.Bonus, 0, 0, false);
            }
            else
            {
                if (this.GetEnchantment(EnchantSlot.Bonus) == null)
                    return;
                this.RemoveEnchant(EnchantSlot.Bonus);
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
            for (uint index = 0; (long) index < (long) gems.Length; ++index)
            {
                T gem = gems[index];
                if ((object) gem != null)
                {
                    SocketInfo socketInfo = this.m_template.Sockets.Get<SocketInfo>(index);
                    if (socketInfo.Color != SocketColor.None)
                    {
                        if (socketInfo.Color == SocketColor.Meta !=
                            (gem.Template.GemProperties.Color == SocketColor.Meta))
                            return false;
                        if (this.IsEquipped && !this.m_owner.Inventory.CheckEquippedGems(gem.Template))
                        {
                            ItemHandler.SendInventoryError((IPacketReceiver) this.m_owner, this, (Item) null,
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
            if (enchant.Slot == EnchantSlot.Permanent)
                this.Owner.SetUInt16Low((UpdateFieldId) ((PlayerFields) (284 + this.Slot * 2)),
                    (ushort) enchant.Entry.Id);
            else if (enchant.Slot == EnchantSlot.Temporary)
                this.Owner.SetUInt16High((UpdateFieldId) ((PlayerFields) (284 + this.Slot * 2)),
                    (ushort) enchant.Entry.Id);
            for (int index = 0; index < enchant.Entry.Effects.Length; ++index)
                EnchantMgr.ApplyEquippedEffect(this, enchant.Entry.Effects[index]);
        }

        private void SetEnchantUnequipped(ItemEnchantment enchant)
        {
            if (enchant.Slot == EnchantSlot.Permanent)
                this.Owner.SetUInt16Low((UpdateFieldId) ((PlayerFields) (284 + this.Slot * 2)), (ushort) 0);
            else if (enchant.Slot == EnchantSlot.Temporary)
                this.Owner.SetUInt16High((UpdateFieldId) ((PlayerFields) (284 + this.Slot * 2)), (ushort) 0);
            for (int index = 0; index < enchant.Entry.Effects.Length; ++index)
                EnchantMgr.RemoveEffect(this, enchant.Entry.Effects[index]);
        }

        /// <summary>Tries to equip this Item</summary>
        public InventoryError Equip()
        {
            return this.m_owner.Inventory.TryEquip(this.m_container, this.Slot);
        }

        public bool Unequip()
        {
            PlayerInventory inventory = this.m_owner.Inventory;
            SimpleSlotId freeSlot = inventory.FindFreeSlot((IMountableItem) this, this.Amount);
            if (freeSlot.Slot == (int) byte.MaxValue)
                return false;
            inventory.SwapUnchecked(this.m_container, this.Slot, freeSlot.Container, freeSlot.Slot);
            return true;
        }

        public InventoryError CheckEquip(Character user)
        {
            return this.m_template.CheckEquip(user);
        }

        internal void OnEquipDecision()
        {
            this.OnEquip();
        }

        internal void OnUnequipDecision(InventorySlot slot)
        {
            if (this.m_template.IsWeapon && slot == InventorySlot.AvLeftHead)
                this.m_owner.MainWeapon = (IAsda2Weapon) null;
            else
                this.OnUnEquip(slot);
        }

        /// <summary>
        /// Called when this Item gets equipped.
        /// Requires map context.
        /// </summary>
        public void OnEquip()
        {
            if (this.IsApplied)
                return;
            this.IsApplied = true;
            InventorySlot slot = (InventorySlot) this.Slot;
            Character owningCharacter = this.OwningCharacter;
            if (slot < InventorySlot.Bag1)
            {
                int num = this.m_template.IsAmmo ? 1 : 0;
            }

            this.m_template.ApplyStatMods(owningCharacter);
            if (this.m_template.BondType == ItemBondType.OnEquip)
                this.Flags |= ItemFlags.Soulbound;
            if (owningCharacter.IsUsingSpell)
                owningCharacter.SpellCast.Cancel(SpellFailedReason.Interrupted);
            for (int index = 0; index < this.m_template.Resistances.Length; ++index)
            {
                int resistance = this.m_template.Resistances[index];
                if (resistance > 0)
                    owningCharacter.ModBaseResistance((DamageSchool) index, resistance);
            }

            if (slot == InventorySlot.Invalid)
                owningCharacter.UpdateRangedDamage();
            else if (this.m_template.InventorySlotType == InventorySlotType.Shield)
                owningCharacter.UpdateBlockChance();
            if (this.m_template.EquipSpells != null)
                owningCharacter.SpellCast.TriggerAll((WorldObject) owningCharacter, this.m_template.EquipSpells);
            if (this.m_template.Set != null)
            {
                Spell[] spellArray =
                    this.m_template.Set.Boni.Get<Spell[]>(owningCharacter.Inventory.GetSetCount(this.m_template.Set) -
                                                          1U);
                if (spellArray != null)
                    owningCharacter.SpellCast.TriggerAll((WorldObject) owningCharacter, spellArray);
            }

            this.m_owner.PlayerAuras.OnEquip(this);
            if (this.m_owner.Inventory.m_ItemEquipmentEventHandlers != null)
            {
                foreach (IItemEquipmentEventHandler equipmentEventHandler in this.m_owner.Inventory
                    .m_ItemEquipmentEventHandlers)
                    equipmentEventHandler.OnEquip(this);
            }

            this.m_template.NotifyEquip(this);
        }

        /// <summary>
        /// Called when this Item gets unequipped.
        /// Requires map context.
        /// </summary>
        public void OnUnEquip(InventorySlot slot)
        {
            if (!this.IsApplied)
                return;
            this.IsApplied = false;
            if (!this.m_template.IsAmmo)
                this.m_owner.SetVisibleItem(slot, (Asda2Item) null);
            this.m_template.RemoveStatMods(this.m_owner);
            if (this.m_template.EquipSpells != null)
            {
                foreach (Spell equipSpell in this.m_template.EquipSpells)
                {
                    if (equipSpell.IsAura)
                        this.m_owner.Auras.Remove(equipSpell);
                }
            }

            for (int index = 0; index < this.m_template.Resistances.Length; ++index)
            {
                int resistance = this.m_template.Resistances[index];
                if (resistance > 0)
                    this.m_owner.ModBaseResistance((DamageSchool) index, -resistance);
            }

            if (slot == InventorySlot.Invalid)
                this.m_owner.UpdateRangedDamage();
            else if (this.m_template.InventorySlotType == InventorySlotType.Shield)
                this.m_owner.UpdateBlockChance();
            if (this.m_template.Set != null)
            {
                Spell[] spellArray =
                    this.m_template.Set.Boni.Get<Spell[]>(this.m_owner.Inventory.GetSetCount(this.m_template.Set) - 1U);
                if (spellArray != null)
                {
                    foreach (Spell index in spellArray)
                    {
                        Aura aura = this.m_owner.Auras[index, true];
                        if (aura != null)
                            aura.Remove(false);
                    }
                }
            }

            if (this.m_hitProc != null)
            {
                this.m_owner.RemoveProcHandler(this.m_hitProc);
                this.m_hitProc = (IProcHandler) null;
            }

            this.m_owner.PlayerAuras.OnBeforeUnEquip(this);
            if (this.m_owner.Inventory.m_ItemEquipmentEventHandlers != null)
            {
                foreach (IItemEquipmentEventHandler equipmentEventHandler in this.m_owner.Inventory
                    .m_ItemEquipmentEventHandlers)
                    equipmentEventHandler.OnBeforeUnEquip(this);
            }

            this.m_template.NotifyUnequip(this);
        }

        /// <summary>
        /// Called whenever an item is used.
        /// Make sure to only call on Items whose Template has a UseSpell.
        /// </summary>
        internal void OnUse()
        {
            if (this.m_template.BondType == ItemBondType.OnUse)
                this.Flags |= ItemFlags.Soulbound;
            if (this.m_template.UseSpell != null && this.m_template.UseSpell.HasCharges)
                this.SpellCharges = this.SpellCharges < 0 ? this.SpellCharges + 1 : this.SpellCharges - 1;
            this.m_template.NotifyUsed(this);
        }

        /// <summary>
        /// Destroys the Item without further checks.
        /// Also destroys all contained Items if this is a Container.
        /// </summary>
        public void Destroy()
        {
            if (this.m_container != null && this.m_container.IsValidSlot(this.Slot))
                this.m_container.Destroy(this.Slot);
            else
                this.DoDestroy();
        }

        /// <summary>Called by the container to</summary>
        protected internal virtual void DoDestroy()
        {
            ItemRecord record = this.m_record;
            this.m_owner.Inventory.OnAmountChanged(this, -this.Amount);
            if (record == null)
                return;
            record.OwnerId = 0;
            record.DeleteLater();
            this.m_record = (ItemRecord) null;
            this.Dispose();
        }

        /// <summary>
        /// Removes this Item from its old Container (if it was added to any).
        /// After calling this method,
        /// make sure to either Dispose the item after removing (in this case you can also simply use <see cref="M:WCell.RealmServer.Entities.Item.Destroy" />
        /// or re-add it somewhere else.
        /// </summary>
        public void Remove(bool ownerChange)
        {
            if (this.m_container == null)
                return;
            this.m_container.Remove(this, ownerChange);
        }

        public QuestHolderInfo QuestHolderInfo
        {
            get { return this.m_template.QuestHolderInfo; }
        }

        public bool CanGiveQuestTo(Character chr)
        {
            return this.m_owner == chr;
        }

        public void OnQuestGiverStatusQuery(Character chr)
        {
        }

        public override void Dispose(bool disposing)
        {
            this.m_owner = (Character) null;
            this.m_isInWorld = false;
            this.IsDeleted = true;
        }

        public override string ToString()
        {
            return string.Format("{0}{1} in Slot {4} (Templ: {2}, Id: {3})",
                this.Amount != 1 ? (object) (this.Amount.ToString() + "x ") : (object) "",
                (object) this.Template.DefaultName, (object) this.m_template.Id, (object) this.EntityId,
                (object) this.Slot);
        }

        public bool IsInContext
        {
            get
            {
                Unit owner = this.Owner;
                if (owner != null)
                {
                    IContextHandler contextHandler = owner.ContextHandler;
                    if (contextHandler != null)
                        return contextHandler.IsInContext;
                }

                return false;
            }
        }

        public void AddMessage(IMessage message)
        {
            Unit owner = this.Owner;
            if (owner == null)
                return;
            owner.AddMessage(message);
        }

        public void AddMessage(Action action)
        {
            Unit owner = this.Owner;
            if (owner == null)
                return;
            owner.AddMessage(action);
        }

        public bool ExecuteInContext(Action action)
        {
            Unit owner = this.Owner;
            if (owner != null)
                return owner.ExecuteInContext(action);
            return false;
        }

        public void EnsureContext()
        {
            Unit owner = this.Owner;
            if (owner == null)
                return;
            owner.EnsureContext();
        }

        public Character OwningCharacter
        {
            get { return this.m_owner; }
            internal set
            {
                this.m_owner = value;
                if (this.m_owner != null)
                {
                    this.m_isInWorld = this.m_unknown = true;
                    this.SetEntityId((UpdateFieldId) ItemFields.OWNER, value.EntityId);
                    this.m_record.OwnerId = (int) value.EntityId.Low;
                }
                else
                {
                    this.SetEntityId((UpdateFieldId) ItemFields.OWNER, EntityId.Zero);
                    this.m_record.OwnerId = 0;
                }
            }
        }

        /// <summary>
        /// The Inventory of the Container that contains this Item
        /// </summary>
        public BaseInventory Container
        {
            get { return this.m_container; }
            internal set
            {
                if (this.m_container == value)
                    return;
                if (value != null)
                {
                    IContainer container = value.Container;
                    this.SetEntityId((UpdateFieldId) ItemFields.CONTAINED, container.EntityId);
                    this.m_record.ContainerSlot = container.BaseInventory.Slot;
                }
                else
                {
                    this.SetEntityId((UpdateFieldId) ItemFields.CONTAINED, EntityId.Zero);
                    this.m_record.ContainerSlot = (byte) 0;
                }

                this.m_container = value;
            }
        }

        /// <summary>The life-time of this Item in seconds</summary>
        public EntityId Creator
        {
            get { return new EntityId((ulong) this.m_record.CreatorEntityId); }
            set
            {
                this.SetEntityId((UpdateFieldId) ItemFields.CREATOR, value);
                this.m_record.CreatorEntityId = (long) value.Full;
            }
        }

        public EntityId GiftCreator
        {
            get { return new EntityId((ulong) this.m_record.GiftCreatorEntityId); }
            set
            {
                this.SetEntityId((UpdateFieldId) ItemFields.GIFTCREATOR, value);
                this.m_record.GiftCreatorEntityId = (long) value.Full;
            }
        }

        /// <summary>
        /// The Slot of this Item within its <see cref="P:WCell.RealmServer.Entities.Item.Container">Container</see>.
        /// </summary>
        public int Slot
        {
            get { return this.m_record.Slot; }
            internal set { this.m_record.Slot = value; }
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
            if (value == 0)
                return 0;
            if (this.m_owner != null)
            {
                if (value > 0 && this.m_template.UniqueCount > 0)
                {
                    int uniqueCount = this.m_owner.Inventory.GetUniqueCount(this.m_template.ItemId);
                    if (value > uniqueCount)
                        value = uniqueCount;
                }

                this.m_owner.Inventory.OnAmountChanged(this, value);
            }

            this.m_record.Amount += value;
            this.SetInt32((UpdateFieldId) ItemFields.STACK_COUNT, this.m_record.Amount);
            return value;
        }

        /// <summary>
        /// Current amount of items in this stack.
        /// Setting the Amount to 0 will destroy the Item.
        /// Keep in mind that this is uint and thus can never become smaller than 0!
        /// </summary>
        public int Amount
        {
            get { return this.m_record.Amount; }
            set
            {
                if (value <= 0)
                {
                    this.Destroy();
                }
                else
                {
                    int diff = value - this.m_record.Amount;
                    if (diff == 0)
                        return;
                    if (this.m_owner != null)
                        this.m_owner.Inventory.OnAmountChanged(this, diff);
                    this.SetInt32((UpdateFieldId) ItemFields.STACK_COUNT, value);
                    this.m_record.Amount = value;
                }
            }
        }

        public uint Duration
        {
            get { return (uint) this.m_record.Duration; }
            set
            {
                this.SetUInt32((UpdateFieldId) ItemFields.DURATION, value);
                this.m_record.Duration = (int) value;
            }
        }

        /// <summary>
        /// Charges of the <c>UseSpell</c> of this Item.
        /// </summary>
        public int SpellCharges
        {
            get { return (int) this.m_record.Charges; }
            set
            {
                if (value == 0 && this.m_record.Charges < (short) 0)
                {
                    this.Destroy();
                }
                else
                {
                    this.m_record.Charges = (short) value;
                    if (this.m_template.UseSpell == null)
                        return;
                    this.SetSpellCharges(this.m_template.UseSpell.Index, value);
                }
            }
        }

        public uint GetSpellCharges(uint index)
        {
            return this.GetUInt32((ItemFields) (16 + (int) index));
        }

        public void ModSpellCharges(uint index, int delta)
        {
            this.SetUInt32(16 + (int) index, (uint) ((ulong) this.GetSpellCharges(index) + (ulong) delta));
        }

        public void SetSpellCharges(uint index, int value)
        {
            this.SetUInt32(16 + (int) index, (uint) Math.Abs(value));
        }

        public ItemFlags Flags
        {
            get { return this.m_record.Flags; }
            set
            {
                this.SetUInt32((UpdateFieldId) ItemFields.FLAGS, (uint) value);
                this.m_record.Flags = value;
            }
        }

        public bool IsAuctioned
        {
            get { return this.m_record.IsAuctioned; }
            set { this.m_record.IsAuctioned = true; }
        }

        public bool IsSoulbound
        {
            get { return this.Flags.HasFlag((Enum) ItemFlags.Soulbound); }
        }

        public bool IsGiftWrapped
        {
            get { return this.Flags.HasFlag((Enum) ItemFlags.GiftWrapped); }
        }

        public bool IsConjured
        {
            get { return this.Flags.HasFlag((Enum) ItemFlags.Conjured); }
        }

        public uint PropertySeed
        {
            get { return this.GetUInt32(ItemFields.PROPERTY_SEED); }
            set
            {
                this.SetUInt32((UpdateFieldId) ItemFields.PROPERTY_SEED, value);
                this.m_record.RandomSuffix = (int) value;
            }
        }

        public uint RandomPropertiesId
        {
            get { return (uint) this.m_record.RandomProperty; }
            set
            {
                this.SetUInt32((UpdateFieldId) ItemFields.RANDOM_PROPERTIES_ID, value);
                this.m_record.RandomProperty = (int) value;
            }
        }

        public int Durability
        {
            get { return this.m_record.Durability; }
            set
            {
                this.SetInt32((UpdateFieldId) ItemFields.DURABILITY, value);
                this.m_record.Durability = value;
            }
        }

        public int MaxDurability
        {
            get { return this.GetInt32(ItemFields.MAXDURABILITY); }
            protected set { this.SetInt32((UpdateFieldId) ItemFields.MAXDURABILITY, value); }
        }

        public void RepairDurability()
        {
            this.Durability = this.MaxDurability;
        }

        public uint TextId
        {
            get { return this.m_record.ItemTextId; }
            internal set { this.m_record.ItemTextId = value; }
        }

        public string ItemText
        {
            get { return this.m_record.ItemText; }
            internal set { this.m_record.ItemText = value; }
        }

        public DamageInfo[] Damages
        {
            get { return this.m_template.Damages; }
        }

        public int BonusDamage { get; set; }

        public SkillId Skill
        {
            get { return this.m_template.ItemProfession; }
        }

        public bool IsRanged
        {
            get { return (double) this.m_template.RangeModifier > 0.0; }
        }

        public bool IsMelee
        {
            get { return (double) this.m_template.RangeModifier == 0.0; }
        }

        /// <summary>
        /// The minimum Range of this weapon
        /// TODO: temporary values
        /// </summary>
        public float MinRange
        {
            get
            {
                if (this.IsMelee)
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
                if (this.IsMelee)
                    return Unit.DefaultMeleeAttackRange;
                return Unit.DefaultRangedAttackRange;
            }
        }

        /// <summary>The time in milliseconds between 2 attacks</summary>
        public int AttackTime
        {
            get { return this.m_template.AttackTime; }
        }

        public ItemRecord Record
        {
            get { return this.m_record; }
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
using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants.Items;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items.Enchanting;
using WCell.RealmServer.Network;
using WCell.Util.Threading;

namespace WCell.RealmServer.Items
{
    public class PlayerInventory : BaseInventory
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>Durability loss on all Items upon Death in percent</summary>
        public static int DeathDurabilityLossPct = 10;

        /// <summary>
        /// Durability loss of *all* Items when reviving at the SpiritHealer
        /// </summary>
        public static int SHResDurabilityLossPct = 25;

        public readonly Dictionary<Asda2ItemId, int> UniqueCounts = new Dictionary<Asda2ItemId, int>(3);
        public const int MaxBankBagCount = 6;
        internal Character m_owner;
        internal Item m_ammo;
        internal WorldObject m_currentBanker;
        private int m_totalCount;

        /// <summary>
        /// Every PartialInventory is also ensured to be IISlotHandler
        /// </summary>
        protected internal PartialInventory[] m_partialInventories;

        public static bool AutoEquipNewItems;
        internal List<IItemEquipmentEventHandler> m_ItemEquipmentEventHandlers;

        /// <summary>
        /// 
        /// </summary>
        public PlayerInventory(Character character)
            : base((IContainer) character, (UpdateFieldId) PlayerFields.INV_SLOT_HEAD, 118)
        {
            this.m_owner = character;
            this.m_partialInventories = new PartialInventory[7];
            this.m_partialInventories[0] = (PartialInventory) new EquipmentInventory(this);
            this.m_partialInventories[1] = (PartialInventory) new BackPackInventory(this);
            this.m_partialInventories[2] = (PartialInventory) new EquippedContainerInventory(this);
            this.m_partialInventories[3] = (PartialInventory) new BankInventory(this);
            this.m_partialInventories[4] = (PartialInventory) new BankBagInventory(this);
            this.m_partialInventories[5] = (PartialInventory) new BuyBackInventory(this);
            this.m_partialInventories[6] = (PartialInventory) new KeyRingInventory(this);
        }

        /// <summary>The amount of items, currently in this inventory.</summary>
        public int TotalCount
        {
            get { return this.m_totalCount; }
        }

        public override PlayerInventory OwnerInventory
        {
            get { return this; }
        }

        /// <summary>Gets the item at the given InventorySlot (or null)</summary>
        public Item this[InventorySlot slot]
        {
            get { return this.m_Items[(int) slot]; }
            set { this[(int) slot] = value; }
        }

        /// <summary>Gets the item at the given InventorySlot (or null)</summary>
        public Item this[EquipmentSlot slot]
        {
            get { return this.m_Items[(int) slot]; }
            set { this[(int) slot] = value; }
        }

        /// <summary>
        /// whether the client currently is allowed to/tries to access his/her bank (through a banker usually)
        /// </summary>
        public bool IsBankOpen
        {
            get
            {
                if (this.m_currentBanker != null && !this.m_owner.IsInRadius(this.m_currentBanker, 5f))
                    this.m_currentBanker = (WorldObject) null;
                return this.m_currentBanker != null;
            }
        }

        /// <summary>
        /// The banker at which we currently opened the BankBox (if any)
        /// </summary>
        public WorldObject CurrentBanker
        {
            get { return this.m_currentBanker; }
            set { this.m_currentBanker = value; }
        }

        /// <summary>The currently used Ammo</summary>
        public Item Ammo
        {
            get { return this.m_ammo; }
            set
            {
                if (value == this.m_ammo)
                    return;
                if (this.m_ammo != null)
                {
                    this.m_ammo.OnUnEquip(InventorySlot.Invalid);
                    this.m_owner.SetUInt32((UpdateFieldId) PlayerFields.AMMO_ID, 0U);
                }

                if (value != null)
                {
                    this.m_owner.SetUInt32((UpdateFieldId) PlayerFields.AMMO_ID, value.Template.Id);
                    value.OnEquip();
                }

                this.m_ammo = value;
            }
        }

        /// <summary>The id of the currently used Ammo</summary>
        public uint AmmoId
        {
            get { return this.m_owner.GetUInt32(PlayerFields.AMMO_ID); }
        }

        /// <summary>All equipped items</summary>
        public EquipmentInventory Equipment
        {
            get { return this.m_partialInventories[0] as EquipmentInventory; }
        }

        /// <summary>The 4 equippable srcCont slots, next to the BackPack</summary>
        public EquippedContainerInventory EquippedContainers
        {
            get { return this.m_partialInventories[2] as EquippedContainerInventory; }
        }

        /// <summary>The contents of the backpack</summary>
        public BackPackInventory BackPack
        {
            get
            {
                if (this.m_partialInventories[1] == null)
                    this.m_partialInventories[1] = (PartialInventory) new BackPackInventory(this);
                return this.m_partialInventories[1] as BackPackInventory;
            }
        }

        /// <summary>The contents of the bankbox (excluding bags)</summary>
        public BankInventory Bank
        {
            get { return this.m_partialInventories[3] as BankInventory; }
        }

        /// <summary>All available containers in the bank</summary>
        public BankBagInventory BankBags
        {
            get { return this.m_partialInventories[4] as BankBagInventory; }
        }

        /// <summary>
        /// Items that have been sold and can be re-purchased by the player
        /// </summary>
        public BuyBackInventory BuyBack
        {
            get { return this.m_partialInventories[5] as BuyBackInventory; }
        }

        /// <summary>The keyring</summary>
        public KeyRingInventory KeyRing
        {
            get { return this.m_partialInventories[6] as KeyRingInventory; }
        }

        public override InventoryError FullError
        {
            get { return InventoryError.INVENTORY_FULL; }
        }

        internal void ModUniqueCount(ItemTemplate templ, int delta)
        {
            if (templ.UniqueCount <= 0)
                return;
            int num;
            this.UniqueCounts.TryGetValue(templ.ItemId, out num);
            this.UniqueCounts[templ.ItemId] = num + delta;
        }

        internal void AddItemUniqueCount(Item item)
        {
            if (item.IsBuyback)
                return;
            this.ModUniqueCount(item.Template, item.Amount);
            if (item.Enchantments == null || !item.Template.HasSockets)
                return;
            for (int index = 0; index < 3; ++index)
            {
                ItemEnchantment enchantment = item.Enchantments[index + 2];
                if (enchantment != null)
                    this.ModUniqueCount(enchantment.Entry.GemTemplate, 1);
            }
        }

        internal void RemoveItemUniqueCount(Item item)
        {
            if (item.IsBuyback)
                return;
            this.ModUniqueCount(item.Template, -item.Amount);
            if (item.Enchantments == null || !item.Template.HasSockets)
                return;
            for (int index = 0; index < 3; ++index)
            {
                ItemEnchantment enchantment = item.Enchantments[index + 2];
                if (enchantment != null)
                    this.ModUniqueCount(enchantment.Entry.GemTemplate, -1);
            }
        }

        public int GetUniqueCount(Asda2ItemId id)
        {
            int num;
            this.UniqueCounts.TryGetValue(id, out num);
            return num;
        }

        /// <summary>
        /// Returns the IItemSlotHandler for the specified InventorySlot
        /// </summary>
        public override IItemSlotHandler GetHandler(int slot)
        {
            if (!this.IsValidSlot(slot))
                return (IItemSlotHandler) null;
            return this.m_partialInventories[(int) ItemMgr.PartialInventoryTypes[slot]] as IItemSlotHandler;
        }

        /// <summary>
        /// Returns the inventory of the corresponding cont (or null).
        /// Only works for bags in the equipment's or bank's cont slots (only these bags may contain items).
        /// </summary>
        public BaseInventory GetContainer(InventorySlot slot, bool inclBank)
        {
            if (slot == InventorySlot.Invalid || slot >= (InventorySlot) ItemMgr.ContainerSlotsWithBank.Length)
                return (BaseInventory) this;
            if (inclBank && ItemMgr.ContainerSlotsWithBank[(int) slot] ||
                !inclBank && ItemMgr.ContainerSlotsWithoutBank[(int) slot])
            {
                Container container = this[slot] as Container;
                if (container != null)
                    return container.BaseInventory;
            }

            return (BaseInventory) null;
        }

        /// <summary>
        /// Returns the Inventory of the Container with the given id or this Character.
        /// </summary>
        /// <returns>Never null.</returns>
        public BaseInventory GetContainer(EntityId containerId, bool inclBank)
        {
            if (containerId == EntityId.Zero || containerId == this.m_owner.EntityId)
                return (BaseInventory) this;
            EquippedContainerInventory equippedContainers = this.EquippedContainers;
            for (int index = 0; index < equippedContainers.Count; ++index)
            {
                Item obj = equippedContainers[index];
                if (obj.EntityId == containerId)
                    return ((Container) obj).BaseInventory;
            }

            if (inclBank)
            {
                for (int index = 0; index < this.BankBags.Count; ++index)
                {
                    Item bankBag = this.BankBags[index];
                    if (bankBag.EntityId == containerId)
                        return ((Container) bankBag).BaseInventory;
                }
            }

            return (BaseInventory) this;
        }

        /// <summary>
        /// Returns the inventory of the corresponding bank-container (or null)
        /// </summary>
        public BaseInventory GetBankContainer(InventorySlot slot)
        {
            if (slot == InventorySlot.Invalid || slot >= (InventorySlot) ItemMgr.ContainerSlotsWithBank.Length)
                return (BaseInventory) this;
            if (ItemMgr.ContainerBankSlots[(int) slot])
            {
                Container container = this[slot] as Container;
                if (container != null)
                    return container.BaseInventory;
            }

            return (BaseInventory) null;
        }

        /// <summary>
        /// Checks some basic parameters for whether the character may interact with (Equip or Use) items and
        /// sends an error if the character cannot interact. Also cancels the current spellcast (if any)
        /// </summary>
        public InventoryError CheckInteract()
        {
            if (this.m_owner.GodMode)
                return InventoryError.OK;
            Character owner = this.Owner;
            InventoryError error;
            if (!owner.IsAlive)
                error = InventoryError.YOU_ARE_DEAD;
            else if (owner.IsUnderInfluenceOf(SpellMechanic.Disarmed))
            {
                error = InventoryError.CANT_DO_WHILE_DISARMED;
            }
            else
            {
                if (owner.CanInteract)
                    return InventoryError.OK;
                error = InventoryError.CANT_DO_RIGHT_NOW;
            }

            ItemHandler.SendInventoryError((IPacketReceiver) owner.Client, (Item) null, (Item) null, error);
            return error;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The result (InventoryError.OK in case that it worked)</returns>
        public InventoryError Ensure(Asda2ItemId templId, int amount, bool equip)
        {
            ItemTemplate template = ItemMgr.GetTemplate(templId);
            if (template != null)
                return this.Ensure(template, amount);
            return InventoryError.ITEM_NOT_FOUND;
        }

        public InventoryError Ensure(ItemTemplate templ, int amount)
        {
            return this.Ensure(templ, amount, PlayerInventory.AutoEquipNewItems);
        }

        public InventoryError Ensure(ItemTemplate templ, int amount, bool equip)
        {
            if (templ.EquipmentSlots == null)
            {
                if (equip)
                    return InventoryError.ITEM_CANT_BE_EQUIPPED;
            }
            else
            {
                for (int index = 0; index < templ.EquipmentSlots.Length; ++index)
                {
                    Item obj = this[templ.EquipmentSlots[index]];
                    if (obj != null && (int) obj.Template.Id == (int) templ.Id)
                        return InventoryError.OK;
                }
            }

            int found = 0;
            if (!this.Iterate((Func<Item, bool>) (item =>
            {
                if (item.Template == templ)
                {
                    found += item.Amount;
                    if (equip && !item.IsEquipped)
                    {
                        int num = (int) this.TryEquip((BaseInventory) this, item.Slot);
                        return false;
                    }

                    if (found >= amount)
                        return false;
                }

                return true;
            })))
                return InventoryError.OK;
            amount -= found;
            if (!equip)
                return this.TryAdd(templ, ref amount, ItemReceptionType.Receive);
            InventorySlot equipSlot = this.GetEquipSlot(templ, true);
            if (equipSlot == InventorySlot.Invalid)
                return InventoryError.INVENTORY_FULL;
            return this.TryAdd(templ, equipSlot, ItemReceptionType.Receive);
        }

        /// <summary>Called whenever the Player receives a new Item</summary>
        internal void OnNewStack(Item item)
        {
            item.m_owner = this.m_owner;
            this.OnAddDontNotify(item);
        }

        internal void OnAddDontNotify(Item item)
        {
            if (!item.IsBuyback)
            {
                ++this.m_totalCount;
                this.AddItemUniqueCount(item);
                this.m_owner.QuestLog.OnItemAmountChanged(item, item.Amount);
                item.OnAdd();
            }

            IContextHandler contextHandler = this.m_owner.ContextHandler;
            if (contextHandler == null)
                return;
            contextHandler.AddMessage((Action) (() =>
            {
                if (this.m_owner == null)
                    return;
                this.m_owner.AddItemToUpdate(item);
            }));
        }

        /// <summary>
        /// Called when the given Item is removed for good.
        /// Don't use this method - but use item.Remove instead.
        /// </summary>
        internal void OnRemove(Item item)
        {
            if (item.IsBuyback)
                return;
            --this.m_totalCount;
            if (item == this.m_ammo)
                this.SetAmmo(this.m_ammo.Template.Id);
            this.m_owner.RemoveOwnedItem(item);
            this.m_owner.QuestLog.OnItemAmountChanged(item, -item.Amount);
            this.RemoveItemUniqueCount(item);
        }

        /// <summary>
        /// Called when the given Item's amount changes by the given difference
        /// </summary>
        public void OnAmountChanged(Item item, int diff)
        {
            if (item.IsBuyback)
                return;
            this.m_owner.QuestLog.OnItemAmountChanged(item, diff);
            this.ModUniqueCount(item.Template, diff);
        }

        public override int FindFreeSlot()
        {
            return this.BackPack.FindFreeSlot();
        }

        /// <summary>
        /// Gets a free slot in the backpack (use FindFreeSlot(IMountableItem, uint) to also look through equipped bags and optionally the bank)
        /// </summary>
        public override int FindFreeSlot(int offset, int end)
        {
            return this.BackPack.FindFreeSlot();
        }

        /// <summary>Finds a free slot after checking for uniqueness</summary>
        /// <param name="templ"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public SimpleSlotId FindFreeSlotCheck(ItemTemplate templ, int amount, out InventoryError err)
        {
            err = InventoryError.OK;
            int amount1 = amount;
            this.CheckUniqueness((IMountableItem) templ, ref amount1, ref err, true);
            if (amount1 != amount)
                return SimpleSlotId.Default;
            SimpleSlotId freeSlot = this.FindFreeSlot((IMountableItem) templ, amount);
            if (freeSlot.Slot == (int) byte.MaxValue)
                err = InventoryError.INVENTORY_FULL;
            return freeSlot;
        }

        /// <summary>
        /// Gets a free slot in a preferred equipped bag (eg Herb bag for Herbs) or backpack
        /// </summary>
        public override SimpleSlotId FindFreeSlot(IMountableItem mountItem, int amount)
        {
            return this.FindFreeSlot(mountItem, amount, PlayerInventory.AutoEquipNewItems);
        }

        public SimpleSlotId FindFreeSlot(Item item, bool tryEquip)
        {
            return this.FindFreeSlot((IMountableItem) item, item.Amount, tryEquip);
        }

        /// <summary>
        /// Gets a free slot in a preferred equipped bag (eg Herb bag for Herbs) or backpack.
        /// Looks for a suitable equipment slot first, if tryEquip is true
        /// </summary>
        public SimpleSlotId FindFreeSlot(IMountableItem mountItem, int amount, bool tryEquip)
        {
            ItemTemplate template = mountItem.Template;
            if (tryEquip && template.EquipmentSlots != null)
            {
                for (int index = 0; index < template.EquipmentSlots.Length; ++index)
                {
                    int equipmentSlot = (int) template.EquipmentSlots[index];
                    if (this[equipmentSlot] == null)
                    {
                        IItemSlotHandler handler = this.GetHandler(equipmentSlot);
                        InventoryError err = InventoryError.OK;
                        handler.CheckAdd(equipmentSlot, amount, (IMountableItem) template, ref err);
                        if (err == InventoryError.OK)
                            return new SimpleSlotId()
                            {
                                Container = (BaseInventory) this,
                                Slot = equipmentSlot
                            };
                        break;
                    }
                }
            }

            SimpleSlotId slotId = SimpleSlotId.Default;
            this.GetPreferredSlot(template, amount, ref slotId);
            if (slotId.Slot == (int) byte.MaxValue)
            {
                InventorySlot[] slotsWithoutBank = ItemMgr.StorageSlotsWithoutBank;
                bool[] containerSlotsWithBank = ItemMgr.ContainerSlotsWithBank;
                for (int index1 = 0; index1 < slotsWithoutBank.Length; ++index1)
                {
                    int index2 = (int) slotsWithoutBank[index1];
                    if (containerSlotsWithBank[index2])
                    {
                        Container container = this.m_Items[index2] as Container;
                        if (container != null && container.Template.MayAddToContainer(template))
                        {
                            BaseInventory baseInventory = container.BaseInventory;
                            Item[] items = baseInventory.Items;
                            for (int index3 = 0; index3 < items.Length; ++index3)
                            {
                                if (items[index3] == null)
                                {
                                    slotId.Container = baseInventory;
                                    slotId.Slot = index3;
                                    return slotId;
                                }
                            }
                        }
                    }
                    else if (this.m_Items[index2] == null)
                    {
                        slotId.Container = (BaseInventory) this;
                        slotId.Slot = index2;
                        return slotId;
                    }
                }

                slotId.Slot = (int) byte.MaxValue;
            }

            return slotId;
        }

        /// <summary>
        /// Sets slotId to the slot that the given templ would prefer (if it has any bag preference).
        /// </summary>
        /// <param name="templ"></param>
        /// <param name="slotId"></param>
        public void GetPreferredSlot(ItemTemplate templ, int amount, ref SimpleSlotId slotId)
        {
            if (templ.IsKey)
            {
                slotId.Container = (BaseInventory) this;
                slotId.Slot = this.KeyRing.FindFreeSlot();
            }
            else
            {
                if (templ.IsContainer || templ.BagFamily == ItemBagFamilyMask.None)
                    return;
                for (InventorySlot index = InventorySlot.Bag1; index <= InventorySlot.BagLast; ++index)
                {
                    Container container = this[index] as Container;
                    if (container != null && container.Template.BagFamily != ItemBagFamilyMask.None)
                    {
                        BaseInventory baseInventory = container.BaseInventory;
                        if (container.Template.MayAddToContainer(templ))
                        {
                            slotId.Slot = baseInventory.FindFreeSlot();
                            if (slotId.Slot != (int) byte.MaxValue)
                            {
                                slotId.Container = baseInventory;
                                break;
                            }
                        }
                    }
                }
            }
        }

        public override bool Distribute(ItemTemplate template, ref int amount)
        {
            if (this.m_ammo != null && this.m_ammo.Template == template)
            {
                int num = template.MaxAmount - this.m_ammo.Amount;
                if (num > 0)
                {
                    if (amount <= num)
                    {
                        this.m_ammo.Amount += amount;
                        return true;
                    }

                    this.m_ammo.Amount += num;
                    amount -= num;
                }
            }

            return base.Distribute(template, ref amount);
        }

        /// <summary>Gets a free slot in the bank or one of the bankbags</summary>
        public SimpleSlotId FindFreeBankSlot(IMountableItem item, int amount)
        {
            SimpleSlotId simpleSlotId = new SimpleSlotId();
            for (int index = 0; index < ItemMgr.BankSlots.Length; ++index)
            {
                int bankSlot = (int) ItemMgr.BankSlots[index];
                if (this.m_Items[bankSlot] == null)
                {
                    simpleSlotId.Container = (BaseInventory) this;
                    simpleSlotId.Slot = bankSlot;
                    return simpleSlotId;
                }
            }

            for (int index1 = 0; index1 < ItemMgr.BankBagSlots.Length; ++index1)
            {
                Container container = this.m_Items[(int) ItemMgr.BankBagSlots[index1]] as Container;
                if (container != null)
                {
                    BaseInventory baseInventory = container.BaseInventory;
                    if (baseInventory.CheckAdd(0, item, amount) == InventoryError.OK)
                    {
                        Item[] items = baseInventory.Items;
                        for (int index2 = 0; index2 < items.Length; ++index2)
                        {
                            if (items[index2] == null)
                            {
                                simpleSlotId.Container = baseInventory;
                                simpleSlotId.Slot = index2;
                                return simpleSlotId;
                            }
                        }
                    }
                }
            }

            simpleSlotId.Slot = (int) byte.MaxValue;
            return simpleSlotId;
        }

        /// <summary>
        /// Gets 0 to max free slots in the backpack or one of the equipped bags and optionally the bank (+ its bags)
        /// </summary>
        /// <param name="inclBank">whether to also look in the bank for free slots</param>
        /// <param name="max">The max of free inventory slots to be returned</param>
        public IList<SimpleSlotId> FindFreeSlots(bool inclBank, int max)
        {
            List<SimpleSlotId> simpleSlotIdList = new List<SimpleSlotId>();
            InventorySlot[] inventorySlotArray = inclBank ? ItemMgr.InvSlotsWithBank : ItemMgr.StorageSlotsWithoutBank;
            bool[] containerSlotsWithBank = ItemMgr.ContainerSlotsWithBank;
            for (int index1 = 0; index1 < inventorySlotArray.Length; ++index1)
            {
                int index2 = (int) inventorySlotArray[index1];
                if (containerSlotsWithBank[index2])
                {
                    Container container = this.m_Items[index2] as Container;
                    if (container != null)
                    {
                        BaseInventory baseInventory = container.BaseInventory;
                        Item[] items = baseInventory.Items;
                        for (int index3 = 0; index3 < items.Length; ++index3)
                        {
                            if (items[index3] == null)
                            {
                                SimpleSlotId simpleSlotId = new SimpleSlotId()
                                {
                                    Container = baseInventory,
                                    Slot = index3
                                };
                                simpleSlotIdList.Add(simpleSlotId);
                                if (simpleSlotIdList.Count == max)
                                    return (IList<SimpleSlotId>) simpleSlotIdList;
                            }
                        }
                    }
                }
                else if (this.m_Items[index2] == null)
                {
                    SimpleSlotId simpleSlotId = new SimpleSlotId()
                    {
                        Container = (BaseInventory) this,
                        Slot = index2
                    };
                    simpleSlotIdList.Add(simpleSlotId);
                    if (simpleSlotIdList.Count == max)
                        return (IList<SimpleSlotId>) simpleSlotIdList;
                }
            }

            return (IList<SimpleSlotId>) simpleSlotIdList;
        }

        /// <summary>
        /// Iterates over all existing items that this Character carries in backpack + bags (if specified, also Bank + BankBags)
        /// </summary>
        /// <param name="validator">Returns whether to continue iteration</param>
        /// <returns>whether iteration was not cancelled (usually indicating we didn't find what we were looking for)</returns>
        public override bool Iterate(Func<Item, bool> validator)
        {
            return this.Iterate(false, validator);
        }

        /// <summary>
        /// Iterates over all existing items that this Character carries in backpack + bags (if specified, also Bank + BankBags)
        /// </summary>
        /// <param name="inclBank">whether to also look through the bank</param>
        /// <param name="validator">Returns whether to continue iteration</param>
        /// <returns>whether iteration was not cancelled (usually indicating we didn't find what we were looking for)</returns>
        public bool Iterate(bool inclBank, Func<Item, bool> validator)
        {
            return this.Iterate(inclBank ? ItemMgr.InvSlotsWithBank : ItemMgr.InvSlots, validator);
        }

        /// <summary>Iterates over the Backpack and Bags</summary>
        /// <param name="validator"></param>
        /// <returns></returns>
        public bool IterateStorage(Func<Item, bool> validator)
        {
            return this.Iterate(ItemMgr.StorageSlotsWithoutBank, validator);
        }

        /// <summary>Iterates over all equipped items</summary>
        /// <param name="validator"></param>
        /// <returns></returns>
        public bool IterateEquipment(Func<Item, bool> validator)
        {
            return this.Iterate(ItemMgr.EquipmentSlots, validator);
        }

        public bool Iterate(InventorySlot[] slots, Func<Item, bool> validator)
        {
            this.Owner.EnsureContext();
            bool[] containerSlotsWithBank = ItemMgr.ContainerSlotsWithBank;
            for (int index = 0; index < slots.Length; ++index)
            {
                int slot = (int) slots[index];
                Item obj1 = this.m_Items[slot];
                if (containerSlotsWithBank[slot])
                {
                    Container container = obj1 as Container;
                    if (container != null)
                    {
                        foreach (Item obj2 in container.BaseInventory.Items)
                        {
                            if (obj2 != null && !validator(obj2))
                                return false;
                        }
                    }
                }
                else if (obj1 != null && !validator(obj1))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// whether the character has at least the given amount of Items with the given templateId
        /// </summary>
        public bool Contains(uint templateId, int amount, bool inclBank)
        {
            int found = 0;
            this.Iterate(inclBank, (Func<Item, bool>) (item =>
            {
                if ((int) item.Template.Id != (int) templateId)
                    return true;
                found += item.Amount;
                return found < amount;
            }));
            return found >= amount;
        }

        /// <summary>
        /// whether the character has  at least the given amount of Items with the given templateId
        /// </summary>
        public bool Contains(Asda2ItemId templateId, int amount, bool inclBank)
        {
            int found = 0;
            this.Iterate(inclBank, (Func<Item, bool>) (item =>
            {
                if (item.Template.ItemId != templateId)
                    return true;
                found += item.Amount;
                return found < amount;
            }));
            return found >= amount;
        }

        /// <summary>
        /// whether the character has the Item with the given ItemId
        /// </summary>
        public bool Contains(uint itemId)
        {
            return this.GetItemByItemId(itemId) != null;
        }

        /// <summary>
        /// whether the character has the Item with the given ItemId
        /// </summary>
        public bool Contains(Asda2ItemId itemId)
        {
            return this.GetItemByItemId(itemId) != null;
        }

        /// <summary>
        /// whether the character has the Item with the given unique id
        /// </summary>
        public bool ContainsLowId(uint lowId)
        {
            return this.GetItemByLowId(lowId) != null;
        }

        /// <summary>
        /// whether the character has all Items with the given unique id
        /// </summary>
        public bool ContainsAll(uint[] entryIds)
        {
            for (int index = 0; index < entryIds.Length; ++index)
            {
                uint id = entryIds[index];
                if (id != 0U && this.Iterate(true, (Func<Item, bool>) (item => (int) item.EntryId != (int) id)))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// whether the character has all Items with the given unique id
        /// </summary>
        public bool ContainsAll(Asda2ItemId[] itemIds)
        {
            for (int index = 0; index < itemIds.Length; ++index)
            {
                Asda2ItemId id = itemIds[index];
                if (id != (Asda2ItemId) 0 &&
                    this.Iterate(true, (Func<Item, bool>) (item => (Asda2ItemId) item.EntryId != id)))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Consumes the given amount of items with the given templateId.
        /// Also looks through the bank when inclBank is set.
        /// </summary>
        /// <param name="inclBank">whether to also search in the bank and its bags (if not enough was found in inventory and bags)</param>
        /// <param name="templateId"></param>
        /// <param name="amount">If 0, consumes all Items that were found</param>
        /// <param name="force">Whether only to remove if there are at least the given amount of items</param>
        /// <returns>whether the required amount of items was found (and thus consumed).</returns>
        public bool Consume(Asda2ItemId templateId, bool inclBank = false, int amount = 1, bool force = true)
        {
            return this.Consume((uint) templateId, inclBank, amount, force);
        }

        /// <summary>
        /// Consumes the given amount of items with the given templateId.
        /// Also looks through the bank when inclBank is set.
        /// </summary>
        /// <param name="inclBank">whether to also search in the bank and its bags (if not enough was found in inventory and bags)</param>
        /// <param name="templateId"></param>
        /// <param name="amount">If 0, consumes all Items that were found</param>
        /// <param name="force">Whether only to remove if there are at least the given amount of items</param>
        /// <returns>whether the required amount of items was found (and thus consumed).</returns>
        public bool Consume(uint templateId, bool inclBank = false, int amount = 1, bool force = true)
        {
            List<SimpleSlotId> simpleSlotIdList = new List<SimpleSlotId>();
            int num1 = this.Find(inclBank, amount, (IList<SimpleSlotId>) simpleSlotIdList,
                (Func<Item, bool>) (item => (int) item.Template.Id == (int) templateId));
            if (!force && num1 < amount)
                return false;
            int count = simpleSlotIdList.Count;
            int num2 = num1 == amount ? 0 : 1;
            if (count > num2)
            {
                for (int index = count - 1; index >= num2; --index)
                    simpleSlotIdList[index].Container.Destroy(simpleSlotIdList[index].Slot);
            }

            if (num1 > amount && count > 0)
                simpleSlotIdList[0].Item.Amount = num1 - amount;
            return true;
        }

        /// <summary>
        /// Finds and consumes all of the given items.
        /// Does not consume anything and returns false if not all items were found.
        /// </summary>
        public bool Consume(ItemStackDescription[] items, bool inclBank)
        {
            List<SimpleSlotId>[] simpleSlotIdListArray = new List<SimpleSlotId>[items.Length];
            int[] numArray = new int[items.Length];
            for (int index = 0; index < items.Length; ++index)
            {
                ItemStackDescription template = items[index];
                List<SimpleSlotId> simpleSlotIdList;
                simpleSlotIdListArray[index] = simpleSlotIdList = new List<SimpleSlotId>(3);
                numArray[index] = this.Find(inclBank, template.Amount, (IList<SimpleSlotId>) simpleSlotIdList,
                    (Func<Item, bool>) (item => item.Template.ItemId == template.ItemId));
                if (numArray[index] < template.Amount)
                    return false;
            }

            for (int index1 = 0; index1 < items.Length; ++index1)
            {
                ItemStackDescription stackDescription = items[index1];
                List<SimpleSlotId> simpleSlotIdList = simpleSlotIdListArray[index1];
                int num1 = numArray[index1];
                int count = simpleSlotIdList.Count;
                int num2 = num1 == stackDescription.Amount ? 0 : 1;
                if (count > num2)
                {
                    for (int index2 = count - 1; index2 >= num2; --index2)
                        simpleSlotIdList[index2].Container.Destroy(simpleSlotIdList[index2].Slot);
                }

                if (num1 > stackDescription.Amount)
                    simpleSlotIdList[0].Item.Amount = num1 - stackDescription.Amount;
            }

            return true;
        }

        /// <summary>
        /// Finds up to max items that are validated by the given validator and puts them in the given list.
        /// </summary>
        /// <param name="inclBank">whether to also search in the bank and its bags (if not enough was found in inventory and bags)</param>
        /// <param name="max">Maximum amount of items to be looked through</param>
        /// <param name="list">List of slot-identifiers for the items to be added to</param>
        /// <returns>The amount of found items</returns>
        public int Find(bool inclBank, int max, IList<SimpleSlotId> list, Func<Item, bool> validator)
        {
            int found = 0;
            this.Iterate(inclBank, (Func<Item, bool>) (item =>
            {
                if (validator(item))
                {
                    found += item.Amount;
                    list.Add(new SimpleSlotId()
                    {
                        Container = item.Container,
                        Slot = item.Slot
                    });
                    if (found >= max)
                        return false;
                }

                return true;
            }));
            return found;
        }

        /// <summary>
        /// Returns the total amount of Items within this Inventory of the given ItemId
        /// </summary>
        public int GetAmount(Asda2ItemId id)
        {
            int amount = 0;
            this.Iterate((Func<Item, bool>) (nexItem =>
            {
                if (nexItem.Template.ItemId == id)
                    amount += nexItem.Amount;
                return true;
            }));
            return amount;
        }

        /// <summary>Moves an item from one slot to another</summary>
        public InventoryError TrySwap(InventorySlot srcBagSlot, int srcSlot, InventorySlot destBagSlot, int destSlot)
        {
            BaseInventory container1 = this.GetContainer(srcBagSlot, this.IsBankOpen);
            BaseInventory container2 = this.GetContainer(destBagSlot, this.IsBankOpen);
            InventoryError error;
            if (container1 == null || container2 == null)
            {
                error = InventoryError.ITEMS_CANT_BE_SWAPPED;
                ItemHandler.SendInventoryError((IPacketReceiver) this.Owner.Client, (Item) null, (Item) null, error);
            }
            else
                error = this.TrySwap(container1, srcSlot, container2, destSlot);

            return error;
        }

        /// <summary>
        /// Moves an item from one slot to another.
        /// Core method for moving items around.
        /// </summary>
        public InventoryError TrySwap(BaseInventory srcCont, int srcSlot, BaseInventory destCont, int destSlot)
        {
            InventoryError err = InventoryError.OK;
            Item obj1 = (Item) null;
            Item otherItem = (Item) null;
            Item obj2 = (Item) null;
            if (!srcCont.IsValidSlot(srcSlot))
                err = InventoryError.ITEM_NOT_FOUND;
            else if (!destCont.IsValidSlot(destSlot))
            {
                err = InventoryError.ITEM_NOT_FOUND2;
            }
            else
            {
                obj1 = srcCont[srcSlot];
                if (obj1 == null)
                    err = InventoryError.SLOT_IS_EMPTY;
                else if (!obj1.CanBeUsed)
                    err = InventoryError.CANT_DO_RIGHT_NOW;
                else if (!this.m_owner.CanInteract)
                    err = InventoryError.CANT_DO_RIGHT_NOW;
                else if (obj1.IsEquippedContainer && !((Container) obj1).BaseInventory.IsEmpty &&
                         (!ItemMgr.IsContainerEquipmentSlot(destSlot) && !this.Owner.GodMode))
                {
                    err = InventoryError.CAN_ONLY_DO_WITH_EMPTY_BAGS;
                }
                else
                {
                    if (destCont == this)
                    {
                        switch (destSlot)
                        {
                            case 15:
                                if (obj1.Template.IsTwoHandWeapon)
                                {
                                    obj2 = this[EquipmentSlot.OffHand];
                                    break;
                                }

                                break;
                            case 16:
                                obj2 = this[EquipmentSlot.MainHand];
                                if (obj2 != null && !obj2.Template.IsTwoHandWeapon)
                                {
                                    obj2 = (Item) null;
                                    break;
                                }

                                break;
                        }
                    }

                    otherItem = destCont[destSlot];
                    if (otherItem != null)
                    {
                        if (!otherItem.CanBeUsed)
                        {
                            err = InventoryError.CANT_DO_RIGHT_NOW;
                        }
                        else
                        {
                            if (obj2 != null)
                            {
                                if (!obj2.Unequip())
                                {
                                    InventoryError error = InventoryError.INVENTORY_FULL;
                                    ItemHandler.SendInventoryError((IPacketReceiver) this.Owner.Client, obj1, otherItem,
                                        error);
                                    return error;
                                }

                                obj2 = (Item) null;
                            }

                            if (otherItem.IsEquippedContainer)
                            {
                                BaseInventory baseInventory = ((Container) otherItem).BaseInventory;
                                err = baseInventory.CheckAdd(0, (IMountableItem) otherItem, otherItem.Amount);
                                if (err != InventoryError.OK)
                                {
                                    ItemHandler.SendInventoryError((IPacketReceiver) this.Owner.Client, obj1, otherItem,
                                        err);
                                    return err;
                                }

                                int freeSlot = baseInventory.FindFreeSlot();
                                if (freeSlot == (int) byte.MaxValue)
                                    return InventoryError.OK;
                                baseInventory.AddUnchecked(freeSlot, otherItem, false);
                            }
                            else
                            {
                                IItemSlotHandler handler = destCont.GetHandler(destSlot);
                                int amount = obj1.Amount;
                                handler.CheckAdd(destSlot, amount, (IMountableItem) obj1, ref err);
                                if (err == InventoryError.OK && !obj1.CanStackWith(otherItem))
                                    srcCont.GetHandler(srcSlot).CheckAdd(srcSlot, otherItem.Amount,
                                        (IMountableItem) otherItem, ref err);
                            }
                        }
                    }
                    else
                    {
                        int slot = 0;
                        if (obj2 != null)
                        {
                            srcCont.GetHandler(srcSlot).CheckAdd(srcSlot, obj2.Amount, (IMountableItem) obj2, ref err);
                            if (err != InventoryError.OK)
                            {
                                ItemHandler.SendInventoryError((IPacketReceiver) this.Owner.Client, obj1, otherItem,
                                    err);
                                return err;
                            }

                            slot = obj2.Slot;
                            obj2.Remove(false);
                        }

                        destCont.GetHandler(destSlot).CheckAdd(destSlot, obj1.Amount, (IMountableItem) obj1, ref err);
                        if (err != InventoryError.OK && obj2 != null)
                            this.AddUnchecked(slot, obj2, false);
                    }
                }
            }

            if (err != InventoryError.OK)
            {
                ItemHandler.SendInventoryError((IPacketReceiver) this.Owner.Client, obj1, otherItem, err);
            }
            else
            {
                this.SwapUnchecked(srcCont, srcSlot, destCont, destSlot);
                if (obj2 != null)
                    srcCont.AddUnchecked(srcSlot, obj2, false);
            }

            return err;
        }

        /// <summary>Tries to move an Item from one container into another</summary>
        public InventoryError TryMove(InventorySlot srcBagSlot, int srcSlot, InventorySlot destBagSlot)
        {
            BaseInventory container1 = this.GetContainer(srcBagSlot, this.IsBankOpen);
            BaseInventory container2 = this.GetContainer(destBagSlot, this.IsBankOpen);
            InventoryError error;
            if (container1 == null || container2 == null)
            {
                error = InventoryError.ITEMS_CANT_BE_SWAPPED;
                ItemHandler.SendInventoryError((IPacketReceiver) this.Owner.Client, (Item) null, (Item) null, error);
            }
            else
                error = this.TryMove(container1, srcSlot, container2);

            return error;
        }

        /// <summary>
        /// Tries to auto-equip an item from the given slot (from within the corresponding cont)
        /// </summary>
        public InventoryError TryMove(BaseInventory srcCont, int srcSlot, BaseInventory destCont)
        {
            InventoryError error = InventoryError.ITEM_NOT_FOUND;
            Item obj = (Item) null;
            if (srcCont.IsValidSlot(srcSlot))
            {
                obj = srcCont[srcSlot];
                if (obj != null)
                {
                    int freeSlot = destCont.FindFreeSlot();
                    if (freeSlot != (int) byte.MaxValue)
                        return this.TrySwap(srcCont, srcSlot, destCont, freeSlot);
                    error = destCont.FullError;
                }
                else
                    error = InventoryError.ITEM_NOT_FOUND;
            }

            ItemHandler.SendInventoryError((IPacketReceiver) this.Owner.Client, obj, (Item) null, error);
            return error;
        }

        /// <summary>
        /// Tries to auti-equip an item from the given slot (from within the corresponding cont)
        /// </summary>
        public InventoryError TryEquip(InventorySlot contSlot, int slot)
        {
            InventoryError error = InventoryError.ITEM_NOT_FOUND;
            BaseInventory container = this.GetContainer(contSlot, this.IsBankOpen);
            if (container != null)
                error = this.TryEquip(container, slot);
            else
                ItemHandler.SendInventoryError((IPacketReceiver) this.m_owner.Client, (Item) null, (Item) null, error);
            return error;
        }

        /// <summary>
        /// Tries to auti-equip an item from the given slot (from within the corresponding cont)
        /// </summary>
        public InventoryError TryEquip(BaseInventory cont, int slot)
        {
            Item obj = (Item) null;
            InventoryError error;
            if (cont.IsValidSlot(slot))
            {
                obj = cont[slot];
                if (obj != null)
                {
                    if (!obj.IsEquipped)
                    {
                        if (obj.Template.EquipmentSlots == null)
                        {
                            error = InventoryError.ITEM_CANT_BE_EQUIPPED;
                        }
                        else
                        {
                            InventorySlot equipSlot = this.GetEquipSlot(obj.Template, false);
                            if (equipSlot != InventorySlot.Invalid)
                                return this.TrySwap(cont, slot, (BaseInventory) this, (int) equipSlot);
                            error = InventoryError.INVENTORY_FULL;
                        }
                    }
                    else
                        error = InventoryError.OK;
                }
                else
                    error = InventoryError.ITEM_NOT_FOUND;
            }
            else
                error = InventoryError.ITEM_NOT_FOUND;

            ItemHandler.SendInventoryError((IPacketReceiver) this.Owner.Client, obj, (Item) null, error);
            return error;
        }

        public InventoryError Split(InventorySlot srcBagSlot, int srcSlot, InventorySlot destBagSlot, int destSlot,
            int amount)
        {
            bool isBankOpen = this.IsBankOpen;
            BaseInventory container1 = this.GetContainer(srcBagSlot, isBankOpen);
            BaseInventory container2 = this.GetContainer(destBagSlot, isBankOpen);
            InventoryError error;
            if (container1 == null || container2 == null)
            {
                error = InventoryError.ITEM_NOT_FOUND;
                ItemHandler.SendInventoryError((IPacketReceiver) this.Owner.Client, (Item) null, (Item) null, error);
            }
            else
                error = this.Split(container1, srcSlot, container2, destSlot, amount);

            return error;
        }

        public InventoryError Split(BaseInventory srcCont, int srcSlot, BaseInventory destCont, int destSlot,
            int amount)
        {
            InventoryError err = InventoryError.OK;
            Item obj = (Item) null;
            Item otherItem = (Item) null;
            if (!this.m_owner.CanInteract)
                err = InventoryError.ITEMS_CANT_BE_SWAPPED;
            else if (!srcCont.IsValidSlot(srcSlot))
                err = InventoryError.ITEM_NOT_FOUND;
            else if (!destCont.IsValidSlot(destSlot))
            {
                err = InventoryError.ITEM_NOT_FOUND2;
            }
            else
            {
                obj = srcCont[srcSlot];
                if (obj == null)
                {
                    err = InventoryError.ITEM_NOT_FOUND;
                }
                else
                {
                    ItemTemplate template = obj.Template;
                    if (!obj.CanBeUsed)
                        err = InventoryError.CANT_DO_RIGHT_NOW;
                    if (!template.IsStackable)
                        err = InventoryError.COULDNT_SPLIT_ITEMS;
                    else if (amount > obj.Amount)
                        err = InventoryError.TRIED_TO_SPLIT_MORE_THAN_COUNT;
                    else if (amount > 0)
                    {
                        otherItem = destCont[destSlot];
                        if (otherItem == null)
                        {
                            destCont.GetHandler(destSlot).CheckAdd(destSlot, amount, (IMountableItem) obj, ref err);
                            if (err == InventoryError.OK)
                                destCont[destSlot] = obj.Split(amount);
                        }
                        else if (otherItem.IsContainer && destCont == this)
                            err = ((Container) otherItem).BaseInventory.TryAddAmount(obj, amount, false,
                                ItemReceptionType.Receive);
                        else if (!obj.CanStackWith(otherItem))
                        {
                            err = InventoryError.COULDNT_SPLIT_ITEMS;
                        }
                        else
                        {
                            amount = Math.Min(amount, template.MaxAmount - otherItem.Amount);
                            obj.Amount -= amount;
                            otherItem.Amount += amount;
                        }
                    }
                }
            }

            if (err != InventoryError.OK)
                ItemHandler.SendInventoryError((IPacketReceiver) this.Owner.Client, obj, otherItem, err);
            return err;
        }

        /// <summary>
        /// Moves the item from the given slot in the bank to the first free slot in the inventory or one of the equipped bags
        /// </summary>
        public InventoryError Withdraw(InventorySlot bagSlot, int slot)
        {
            Item bankItem = this.GetBankItem(bagSlot, slot);
            InventoryError error;
            if (bankItem == null)
            {
                error = InventoryError.ITEM_NOT_FOUND;
            }
            else
            {
                SimpleSlotId freeSlot = this.FindFreeSlot((IMountableItem) bankItem, bankItem.Amount);
                if (freeSlot.Slot == (int) byte.MaxValue)
                {
                    error = InventoryError.INVENTORY_FULL;
                    ItemHandler.SendInventoryError((IPacketReceiver) this.m_owner.Client, bankItem, (Item) null, error);
                }
                else
                {
                    error = InventoryError.OK;
                    this.SwapUnchecked(bankItem.Container, slot, freeSlot.Container, freeSlot.Slot);
                }
            }

            return error;
        }

        /// <summary>
        /// Moves the item from the given slot to the given slot in the Bank or one of its bags
        /// </summary>
        public InventoryError Deposit(InventorySlot bagSlot, int slot)
        {
            Item obj = this.GetItem(bagSlot, slot, false);
            InventoryError error;
            if (obj == null)
            {
                error = InventoryError.ITEM_NOT_FOUND;
            }
            else
            {
                SimpleSlotId freeBankSlot = this.FindFreeBankSlot((IMountableItem) obj, obj.Amount);
                error = freeBankSlot.Slot != (int) byte.MaxValue
                    ? this.TrySwap(obj.Container, slot, freeBankSlot.Container, freeBankSlot.Slot)
                    : InventoryError.BANK_FULL;
            }

            if (error != InventoryError.OK)
                ItemHandler.SendInventoryError((IPacketReceiver) this.m_owner.Client, obj, (Item) null, error);
            return error;
        }

        /// <summary>
        /// Returns the first stack of Items with ItemId encountered in the Character's backpack
        /// and bags and (if open) in the Bank and Bankbags.
        /// </summary>
        /// <param name="id">ItemId of the Item to find.</param>
        /// <returns>The first stack encountered or null.</returns>
        public Item GetItemByItemId(Asda2ItemId id)
        {
            return this.GetItemByItemId((uint) id, this.IsBankOpen);
        }

        /// <summary>
        /// Returns the first stack of Items with ItemId encountered in the Character's backpack
        /// and bags and (if open) in the Bank and Bankbags.
        /// </summary>
        /// <param name="id">ItemId of the Item to find.</param>
        /// <returns>The first stack encountered or null.</returns>
        public Item GetItemByItemId(uint id)
        {
            return this.GetItemByItemId(id, this.IsBankOpen);
        }

        /// <summary>
        /// Returns the first stack of Items with ItemId encountered in the Character's backpack
        /// and bags and (optionally) in the Bank and Bankbags.
        /// </summary>
        /// <param name="id">ItemId of the Item to find.</param>
        /// <param name="includeBank">Whether to search the Bank.</param>
        /// <returns>The first stack encountered or null.</returns>
        public Item GetItemByItemId(Asda2ItemId id, bool includeBank)
        {
            return this.GetItemByItemId((uint) id, includeBank);
        }

        /// <summary>
        /// Returns the first stack of Items with ItemId encountered in the Character's backpack
        /// and bags and (optionally) in the Bank and Bankbags.
        /// </summary>
        /// <param name="templateId">ItemId of the Item to find.</param>
        /// <param name="includeBank">Whether to search the Bank.</param>
        /// <returns>The first stack encountered or null.</returns>
        public Item GetItemByItemId(uint templateId, bool includeBank)
        {
            Item foundItem = (Item) null;
            this.Iterate(includeBank, (Func<Item, bool>) (item =>
            {
                if ((int) item.Template.Id != (int) templateId)
                    return true;
                foundItem = item;
                return false;
            }));
            return foundItem;
        }

        /// <summary>
        /// Searches through a Character's Backpack and Bags and (if open) Bank and Bank Bags
        /// for Items with the given Id and adds up their amounts.
        /// </summary>
        /// <param name="id">ItemId of the Item to find.</param>
        /// <returns>The sum of the amounts of the Items encountered.</returns>
        public int GetItemAmountByItemId(Asda2ItemId id)
        {
            return this.GetItemAmountByItemId((uint) id, this.IsBankOpen);
        }

        /// <summary>
        /// Searches through a Character's Backpack and Bags and (if open) Bank and Bank Bags
        /// for Items with the given Id and adds up their amounts.
        /// </summary>
        /// <param name="id">ItemId of the Item to find.</param>
        /// <returns>The sum of the amounts of the Items encountered.</returns>
        public int GetItemAmountByItemId(uint id)
        {
            return this.GetItemAmountByItemId(id, this.IsBankOpen);
        }

        /// <summary>
        /// Searches through a Character's Backpack and Bags and (if open) Bank and Bank Bags
        /// for Items with the given Id and adds up their amounts.
        /// </summary>
        /// <param name="id">ItemId of the Item to find.</param>
        /// <param name="includeBank">Whether to search the Bank and Bank Bags.</param>
        /// <returns>The sum of the amounts of the Items encountered.</returns>
        public int GetItemAmountByItemId(Asda2ItemId id, bool includeBank)
        {
            return this.GetItemAmountByItemId((uint) id, includeBank);
        }

        /// <summary>
        /// Searches through a Character's Backpack and Bags and (if open) Bank and Bank Bags
        /// for Items with the given Id and adds up their amounts.
        /// </summary>
        /// <param name="templateId">ItemId of the Item to find.</param>
        /// <param name="includeBank">Whether to search the Bank and Bank Bags.</param>
        /// <returns>The sum of the amounts of the Items encountered.</returns>
        public int GetItemAmountByItemId(uint templateId, bool includeBank)
        {
            int total = 0;
            this.Iterate(includeBank, (Func<Item, bool>) (item =>
            {
                if ((int) item.Template.Id == (int) templateId)
                    total += item.Amount;
                return true;
            }));
            return total;
        }

        /// <summary>
        /// Returns the Item with the given EntityId if it exists in the player's inventory (or bank, if it is open).
        /// </summary>
        public Item GetItem(EntityId id)
        {
            return this.GetItem(id, this.IsBankOpen);
        }

        public Item GetItem(EntityId id, bool includeBank)
        {
            Item item = (Item) null;
            this.Iterate(includeBank, (Func<Item, bool>) (nextItem =>
            {
                if (!(nextItem.EntityId == id))
                    return true;
                item = nextItem;
                return false;
            }));
            return item;
        }

        public Item GetItemByLowId(uint entityLowId)
        {
            return this.GetItemByLowId(entityLowId, false);
        }

        public Item GetItemByLowId(uint entityLowId, bool includeBank)
        {
            Item item = (Item) null;
            this.Iterate(includeBank, (Func<Item, bool>) (nextItem =>
            {
                if ((int) nextItem.EntityId.Low != (int) entityLowId)
                    return true;
                item = nextItem;
                return false;
            }));
            return item;
        }

        /// <summary>
        /// Attempts to retreive an item in the character's available inventory.
        /// </summary>
        /// <param name="contSlot">The container's slot (Bag1 ... BagLast, BankBag1 ... BankBagLast),
        /// use InventorySlot.Invalid for the Backpack slots.</param>
        /// <param name="slot">The slot within the container where the item resides.</param>
        /// <param name="inclBank">Whether to check the bank as well.</param>
        /// <returns>The item or null.</returns>
        public Item GetItem(InventorySlot contSlot, int slot, bool inclBank)
        {
            BaseInventory baseInventory = this.GetContainer(contSlot, inclBank) ?? (BaseInventory) this;
            if (!baseInventory.IsValidSlot(slot))
                return (Item) null;
            return baseInventory[slot];
        }

        /// <summary>Returns an item from the bank in the given slot</summary>
        public Item GetBankItem(InventorySlot contSlot, int slot)
        {
            BaseInventory bankContainer = this.GetBankContainer(contSlot);
            if (bankContainer != null && bankContainer.IsValidSlot(slot))
                return bankContainer[slot];
            return (Item) null;
        }

        /// <summary>
        /// Enumerates all of the Character's owned Items (optionally with or without BuyBack)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Item> GetAllItems(bool includeBuyBack)
        {
            foreach (InventorySlot index in includeBuyBack ? ItemMgr.AllSlots : ItemMgr.OwnedSlots)
            {
                Item item = this[index];
                if (item != null)
                {
                    yield return item;
                    if (item is Container && ItemMgr.ContainerSlotsWithBank[item.Slot])
                    {
                        IEnumerator<Item> enumerator = ((Container) item).BaseInventory.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            Item contItem = enumerator.Current;
                            yield return contItem;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes Items from the Backpack, Bags and (if indicated) from the Bank and BankBags.
        /// </summary>
        /// <param name="id">The ItemId of the Items to remove.</param>
        /// <param name="amount">The number of Items to remove.</param>
        /// <param name="includeBank">Whether to include the Bank and BankBags.</param>
        /// <returns>Whether amount of items was removed.</returns>
        public bool RemoveByItemId(Asda2ItemId id, int amount, bool includeBank)
        {
            if (this.GetItemAmountByItemId(id, includeBank) < amount)
                return false;
            this.Iterate(includeBank, (Func<Item, bool>) (item =>
            {
                if (item.Template.ItemId != id)
                    return true;
                if (item.Amount >= amount)
                {
                    item.Amount -= amount;
                    amount = 0;
                    return false;
                }

                amount -= item.Amount;
                item.Amount = 0;
                return true;
            }));
            return amount <= 0;
        }

        /// <summary>
        /// </summary>
        /// <returns>Whether a match was found</returns>
        public bool Iterate(InventorySlotTypeMask slots, Func<Item, bool> callback)
        {
            for (InventorySlotType inventorySlotType = InventorySlotType.Head;
                inventorySlotType < InventorySlotType.End;
                ++inventorySlotType)
            {
                if (slots.HasAnyFlag(inventorySlotType))
                {
                    foreach (EquipmentSlot equipmentSlot in ItemMgr.GetEquipmentSlots(inventorySlotType))
                    {
                        Item obj = this[equipmentSlot];
                        if (obj != null && !callback(obj))
                            return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the given slot is occupied and -if so- puts the item from that slot into a free
        /// storage slot (within the backpack or any equipped bags).
        /// </summary>
        /// <returns>whether the given slot is now empty</returns>
        public bool EnsureEmpty(EquipmentSlot slot)
        {
            Item obj = this[slot];
            if (obj == null)
                return true;
            SimpleSlotId freeSlot = this.FindFreeSlot((IMountableItem) obj, obj.Amount);
            if (freeSlot.Slot == (int) byte.MaxValue)
                return false;
            this.SwapUnchecked((BaseInventory) this, (int) slot, freeSlot.Container, freeSlot.Slot);
            return true;
        }

        /// <summary>
        /// Tries to unequip the item at the given InventorySlot into a free available slot
        /// </summary>
        public bool Unequip(InventorySlot slot)
        {
            return this.Unequip((BaseInventory) this, (int) slot);
        }

        /// <summary>
        /// Tries to unequip the item at the given slot and put it into the first available slot in the backpack or an equipped cont.
        /// </summary>
        public bool Unequip(BaseInventory container, int slot)
        {
            Item obj = container[slot];
            if (obj == null)
                return false;
            return obj.Unequip();
        }

        /// <summary>
        /// Returns a free EquipmentSlot (or the first allowed slot).
        /// If force is set and there is no free slot and the backpack still has space, it will unequip the
        /// item in the first possible slot to the backpack or an equipped bag.
        /// </summary>
        public InventorySlot GetEquipSlot(ItemTemplate templ, bool force)
        {
            EquipmentSlot[] equipmentSlots = templ.EquipmentSlots;
            if (templ.IsMeleeWeapon)
            {
                Item obj = this[EquipmentSlot.MainHand];
                if (obj != null && obj.Template.IsTwoHandWeapon)
                    return InventorySlot.AvLeftHead;
            }

            for (int index1 = 0; index1 < equipmentSlots.Length; ++index1)
            {
                EquipmentSlot index2 = equipmentSlots[index1];
                if ((index2 != EquipmentSlot.OffHand || this.m_owner.Skills.CanDualWield) && this[index2] == null)
                    return (InventorySlot) index2;
            }

            EquipmentSlot slot = equipmentSlots[0];
            if (force && !this.EnsureEmpty(slot))
                return InventorySlot.Invalid;
            return (InventorySlot) slot;
        }

        /// <summary>Sets the ammo to the given template-id.</summary>
        /// <returns>whether the ammo with the given id was found</returns>
        public bool SetAmmo(uint templId)
        {
            if (templId == 0U)
            {
                this.Ammo = (Item) null;
                return true;
            }

            bool found = false;
            this.Iterate(this.IsBankOpen, (Func<Item, bool>) (item =>
            {
                if ((int) item.Template.Id != (int) templId || item.Amount <= 0 || item == this.m_ammo)
                    return true;
                InventoryError error = item.Template.CheckEquip(this.Owner);
                if (error == InventoryError.OK)
                {
                    if (!item.CanBeUsed)
                        error = InventoryError.CANT_DO_RIGHT_NOW;
                    else if (item.Template.IsAmmo)
                    {
                        this.Ammo = item;
                        found = true;
                    }
                    else
                        error = InventoryError.ONLY_AMMO_CAN_GO_HERE;
                }

                if (error != InventoryError.OK)
                    ItemHandler.SendInventoryError((IPacketReceiver) this.Owner.Client, item, (Item) null, error);
                return false;
            }));
            return found;
        }

        /// <summary>
        /// Tries to consume 1 piece of ammo and returns whether there was any left to be consumed
        /// </summary>
        public bool ConsumeAmmo()
        {
            if (this.m_ammo == null)
                return false;
            --this.m_ammo.Amount;
            return true;
        }

        /// <summary>
        /// Returns the amount of Items of the given Set that the owner currently has equipped.
        /// </summary>
        /// <param name="set"></param>
        /// <returns></returns>
        public uint GetSetCount(ItemSet set)
        {
            uint num = 0;
            foreach (ItemTemplate template in set.Templates)
            {
                foreach (EquipmentSlot equipmentSlot in template.EquipmentSlots)
                {
                    Item obj = this.Equipment[equipmentSlot];
                    if (obj != null && obj.Template.Set == set)
                        ++num;
                }
            }

            return num;
        }

        public IList<EquipmentSet> EquipmentSets
        {
            get
            {
                if (this.Owner != null)
                    return this.Owner.Record.EquipmentSets;
                return EquipmentSet.EmptyList;
            }
        }

        public void SetEquipmentSet(EntityId setEntityId, int setId, string name, string icon, IList<EntityId> itemList)
        {
        }

        public void UseEquipmentSet(EquipmentSwapHolder[] swaps)
        {
            if (swaps == null)
                return;
            for (int destSlot = 0; destSlot < 19; ++destSlot)
            {
                EquipmentSwapHolder swap = swaps[destSlot];
                if (!(swap.ItemGuid == EntityId.Zero))
                {
                    Item obj1 = this.GetItem(swap.ItemGuid);
                    Item obj2 = this.GetItem(swap.SrcContainer, swap.SrcSlot, this.IsBankOpen);
                    if (obj1 != null && obj1 != obj2)
                    {
                        InventoryError error = this.TrySwap(swap.SrcContainer, swap.SrcSlot, InventorySlot.Invalid,
                            destSlot);
                        if (error != InventoryError.OK)
                        {
                            ItemHandler.SendInventoryError((IPacketReceiver) this.Owner.Client, error);
                            break;
                        }

                        ItemHandler.SendUseEquipmentSetResult((IPacketReceiver) this.Owner.Client,
                            UseEquipmentSetError.Success);
                    }
                }
            }
        }

        public void SendEquipmentSetList()
        {
            ItemHandler.SendEquipmentSetList((IPacketReceiver) this.Owner, this.EquipmentSets);
        }

        public void DeleteEquipmentSet(EntityId setGuid)
        {
            foreach (EquipmentSet equipmentSet in (IEnumerable<EquipmentSet>) this.EquipmentSets)
            {
                if (!(equipmentSet.SetGuid != setGuid))
                {
                    equipmentSet.Items = (IList<EquipmentSetItemMapping>) null;
                    this.EquipmentSets.Remove(equipmentSet);
                    break;
                }
            }
        }

        private EquipmentSet GetEquipmentSet(EntityId setEntityId)
        {
            foreach (EquipmentSet equipmentSet in (IEnumerable<EquipmentSet>) this.EquipmentSets)
            {
                if (!(equipmentSet.SetGuid != setEntityId))
                    return equipmentSet;
            }

            return (EquipmentSet) null;
        }

        /// <summary>Adds a handler to be notified upon equipment changes</summary>
        public void AddEquipmentHandler(IItemEquipmentEventHandler handler)
        {
            if (this.m_ItemEquipmentEventHandlers == null)
                this.m_ItemEquipmentEventHandlers = new List<IItemEquipmentEventHandler>(3);
            this.m_ItemEquipmentEventHandlers.Add(handler);
        }

        /// <summary>Removes the given handler</summary>
        public void RemoveEquipmentHandler(IItemEquipmentEventHandler handler)
        {
            if (this.m_ItemEquipmentEventHandlers == null)
                return;
            this.m_ItemEquipmentEventHandlers.Remove(handler);
        }

        /// <summary>
        /// Checks for whether the given amount of that Item can still be added
        /// (due to max unique count).
        /// </summary>
        /// <param name="mountItem"></param>
        /// <returns></returns>
        internal InventoryError CheckEquipCount(IMountableItem mountItem)
        {
            ItemTemplate template = mountItem.Template;
            if (template.Flags.HasFlag((Enum) ItemFlags.UniqueEquipped))
            {
                foreach (EquipmentSlot equipmentSlot in template.EquipmentSlots)
                {
                    Item obj = this[equipmentSlot];
                    if (obj != null && (int) obj.Template.Id == (int) template.Id)
                        return InventoryError.ITEM_UNIQUE_EQUIPABLE;
                }
            }

            if (mountItem.Enchantments != null)
            {
                for (EnchantSlot enchantSlot = EnchantSlot.Socket1; enchantSlot < EnchantSlot.Bonus; ++enchantSlot)
                {
                    ItemEnchantment enchantment = mountItem.Enchantments[(uint) enchantSlot];
                    if (enchantment != null && !this.CheckEquippedGems(enchantment.Entry.GemTemplate))
                        return InventoryError.ITEM_UNIQUE_EQUIPPABLE_SOCKETED;
                }
            }

            return InventoryError.OK;
        }

        internal bool CheckEquippedGems(ItemTemplate gemTempl)
        {
            if (gemTempl != null && gemTempl.Flags.HasFlag((Enum) ItemFlags.UniqueEquipped))
            {
                for (EquipmentSlot index = EquipmentSlot.Head; index < EquipmentSlot.Bag1; ++index)
                {
                    Item obj = this[index];
                    if (obj != null && obj.HasGem(gemTempl.ItemId))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Removes the given percentage of durability from all Items of this Inventory.
        /// </summary>
        public void ApplyDurabilityLoss(int durLossPct)
        {
            this.IterateEquipment((Func<Item, bool>) (item =>
            {
                if (item.MaxDurability > 0)
                    item.Durability = Math.Max(0, item.Durability - (item.Durability * durLossPct + 50) / 100);
                return true;
            }));
            ItemHandler.SendDurabilityDamageDeath((IPacketReceiver) this.Owner);
        }

        /// <summary>Unequips all Items (if there is enough space left)</summary>
        public void Strip()
        {
            foreach (Item obj in (PartialInventory) this.Equipment)
                obj.Unequip();
        }

        /// <summary>Destroys all Items</summary>
        public void Purge()
        {
            foreach (Item allItem in this.GetAllItems(false))
                allItem.Destroy();
        }

        /// <summary>
        /// Check for whether there is at least one Item of each Category in this Inventory.
        /// </summary>
        /// <param name="cats"></param>
        /// <returns></returns>
        public bool CheckTotemCategories(ToolCategory[] cats)
        {
            for (int index1 = 0; index1 < cats.Length; ++index1)
            {
                ToolCategory cat = cats[index1];
                EquipmentSlot[] toolCategorySlots = ItemMgr.GetToolCategorySlots(cat);
                bool flag = false;
                if (toolCategorySlots != null)
                {
                    for (int index2 = 0; index2 < toolCategorySlots.Length; ++index2)
                    {
                        Item obj = this[toolCategorySlots[index2]];
                        if (obj != null && obj.Template.ToolCategory == cat)
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                else
                    flag = this[EquipmentSlot.MainHand] != null &&
                           this[EquipmentSlot.MainHand].Template.ToolCategory == cat ||
                           !this.Iterate((Func<Item, bool>) (item => item.Template.ToolCategory != cat));

                if (!flag)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Ensures that there is at least one Item that
        /// works as HearthStone in this inventory (if missing, tries to add one).
        /// </summary>
        /// <returns>False only if Hearthstone is missing and could not be added, otherwise true.</returns>
        public bool EnsureHearthStone()
        {
            return !this.Iterate((Item item) => !item.Template.IsHearthStone) || true;
        }

        public void AddDefaultItems()
        {
            List<ItemStack> initialItems = this.m_owner.Archetype.GetInitialItems(this.m_owner.Gender);
            if (initialItems == null)
                return;
            for (int index = 0; index < initialItems.Count; ++index)
            {
                ItemStack itemStack = initialItems[index];
                Item obj = Item.CreateItem(itemStack.Template, this.m_owner, itemStack.Amount);
                SimpleSlotId freeSlot = this.FindFreeSlot((IMountableItem) obj, 1, true);
                if (freeSlot.Slot == (int) byte.MaxValue)
                {
                    PlayerInventory.log.Warn("{0} could not equip initial Item \"{1}\": No available slot.",
                        (object) this.m_owner, (object) obj);
                }
                else
                {
                    freeSlot.Container[freeSlot.Slot] = obj;
                    this.OnAddDontNotify(obj);
                    if (obj.Template.IsAmmo)
                        this.Ammo = obj;
                }
            }
        }

        /// <summary>Adds the Items of this Character</summary>
        internal void AddOwnedItems()
        {
        }

        /// <summary>
        /// Saves all items and adds their records to the given list.
        /// </summary>
        /// <param name="records"></param>
        public void SaveAll(List<ItemRecord> records)
        {
            foreach (Item allItem in this.GetAllItems(false))
            {
                allItem.Save();
                records.Add(allItem.Record);
            }
        }

        public override IEnumerator<Item> GetEnumerator()
        {
            foreach (Item obj in this.m_Items)
            {
                if (obj != null)
                {
                    yield return obj;
                    if (obj.IsContainer)
                    {
                        BaseInventory cont = ((Container) obj).BaseInventory;
                        IEnumerator<Item> enumerator = cont.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            Item bagItem = enumerator.Current;
                            yield return bagItem;
                        }
                    }
                }
            }
        }
    }
}
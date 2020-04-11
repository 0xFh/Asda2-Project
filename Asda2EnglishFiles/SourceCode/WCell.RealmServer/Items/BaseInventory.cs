using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using WCell.Constants.Items;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items.Enchanting;
using WCell.RealmServer.Network;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.Threading;

namespace WCell.RealmServer.Items
{
    public abstract class BaseInventory : IInventory, IList<Item>, ICollection<Item>, IEnumerable<Item>, IEnumerable
    {
        public const int INVALID_SLOT = 255;
        protected int m_baseField;
        protected Item[] m_Items;
        protected int m_count;

        /// <summary>The srcCont or Player that this inventory belongs to.</summary>
        protected internal IContainer m_container;

        protected BaseInventory(IContainer owner, UpdateFieldId baseField, int invSize)
            : this(owner, baseField, new Item[invSize])
        {
        }

        /// <summary>Inventory for shared item-array</summary>
        protected BaseInventory(IContainer owner, UpdateFieldId baseField, Item[] items)
        {
            this.m_container = owner;
            this.m_baseField = baseField.RawId;
            this.m_Items = items;
            this.m_count = 0;
        }

        /// <summary>The owning player of this inventory</summary>
        public Character Owner
        {
            get
            {
                if (this.m_container is Item)
                    return ((Item) this.m_container).OwningCharacter;
                return this.m_container as Character;
            }
        }

        /// <summary>
        /// The containing ObjectBase (either Container or Character)
        /// </summary>
        public IContainer Container
        {
            get { return this.m_container; }
        }

        /// <summary>The slot of where this Container is located</summary>
        public byte Slot
        {
            get
            {
                if (this.m_container is WCell.RealmServer.Entities.Container)
                    return (byte) ((Item) this.m_container).Slot;
                return byte.MaxValue;
            }
        }

        public abstract InventoryError FullError { get; }

        /// <summary>
        /// The underlying arrays of items of this inventory (don't modify from outside)
        /// </summary>
        public Item[] Items
        {
            get { return this.m_Items; }
        }

        public int IndexOf(Item item)
        {
            return item.Slot;
        }

        public void Insert(int index, Item item)
        {
            throw new InvalidOperationException();
        }

        public void RemoveAt(int index)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Sets or Gets the Item at the given slot (make sure that slot is valid and unoccupied).
        /// </summary>
        /// <remarks>Cannot set to null - Use <see cref="M:WCell.RealmServer.Items.BaseInventory.Remove(System.Int32,System.Boolean)" /> instead.</remarks>
        /// <param name="slot"></param>
        /// <returns></returns>
        public Item this[int slot]
        {
            get
            {
                if (slot < 0 || slot > this.m_Items.Length)
                    return (Item) null;
                return this.m_Items[slot];
            }
            set
            {
                if (value == null)
                    throw new NullReferenceException(string.Format(
                        "Cannot set Slot {0} in Inventory \"{1}\" to null - Use the Remove-method instead.",
                        (object) slot, (object) this));
                if (this.m_Items[slot] != null)
                {
                    string msg =
                        string.Format(
                            "Cannot add Item \"{0}\" to Slot {1} in Inventory \"{2}\" to - Slot is occupied with Item \"{3}\".",
                            (object) value, (object) slot, (object) this, (object) this.m_Items[slot]);
                    LogUtil.ErrorException(msg, new object[0]);
                    value.Destroy();
                    if (this.Owner == null)
                        return;
                    this.Owner.SendSystemMessage(msg);
                }
                else
                {
                    ++this.m_count;
                    value.Container = this;
                    value.Slot = slot;
                    this.SetContainerEntityId(value.EntityId, slot, this);
                    this.m_Items[slot] = value;
                    IItemSlotHandler handler = this.GetHandler(slot);
                    if (handler == null)
                        return;
                    handler.Added(value);
                }
            }
        }

        public void Add(Item item)
        {
            throw new InvalidOperationException();
        }

        public void Clear()
        {
            throw new InvalidOperationException();
        }

        public bool Contains(Item item)
        {
            return item.Container == this;
        }

        public void CopyTo(Item[] array, int arrayIndex)
        {
            this.m_Items.CopyTo((Array) array, arrayIndex);
        }

        public bool Remove(Item item)
        {
            throw new InvalidOperationException();
        }

        /// <summary>The amount of items, currently in this inventory.</summary>
        public int Count
        {
            get { return this.m_count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// The maximum amount of items, supported by this inventory
        /// </summary>
        public virtual int MaxCount
        {
            get { return this.m_Items.Length; }
        }

        /// <summary>whether there are no items in this inventory</summary>
        public bool IsEmpty
        {
            get { return this.m_count == 0; }
        }

        /// <summary>whether there is no space left in this inventory</summary>
        public bool IsFull
        {
            get { return this.m_count == this.m_Items.Length; }
        }

        /// <summary>
        /// Swaps the items at the given slots without further checks.
        /// </summary>
        /// <param name="slot1"></param>
        /// <param name="slot2"></param>
        /// <remarks>Make sure the slots are valid before calling.</remarks>
        public void SwapUnchecked(BaseInventory cont1, int slot1, BaseInventory cont2, int slot2)
        {
            Item obj = cont1.m_Items[slot1];
            Item otherItem = cont2.m_Items[slot2];
            IItemSlotHandler handler1 = cont1.GetHandler(slot1);
            IItemSlotHandler handler2 = cont2.GetHandler(slot2);
            if (otherItem != null && obj.CanStackWith(otherItem) && otherItem.Amount < otherItem.Template.MaxAmount)
            {
                int num = Math.Min(obj.Template.MaxAmount - otherItem.Amount, obj.Amount);
                obj.Amount -= num;
                otherItem.Amount += num;
            }
            else
            {
                if (obj != null)
                {
                    obj.Slot = slot2;
                    if (handler1 != null)
                        handler1.Removed(slot1, obj);
                }

                if (otherItem != null)
                {
                    otherItem.Slot = slot1;
                    if (handler2 != null)
                        handler2.Removed(slot2, otherItem);
                }

                cont1.m_Items[slot1] = otherItem;
                cont2.m_Items[slot2] = obj;
                if (obj != null)
                {
                    this.SetContainerEntityId(obj.EntityId, slot2, cont2);
                    obj.Container = cont2;
                    if (handler2 != null)
                        handler2.Added(obj);
                }
                else
                    this.SetContainerEntityId(EntityId.Zero, slot2, cont2);

                if (otherItem != null)
                {
                    this.SetContainerEntityId(otherItem.EntityId, slot1, cont1);
                    otherItem.Container = cont1;
                    if (handler1 == null)
                        return;
                    handler1.Added(otherItem);
                }
                else
                    this.SetContainerEntityId(EntityId.Zero, slot1, cont1);
            }
        }

        internal void SwapUnnotified(int slot1, int slot2)
        {
            Item obj1 = this.m_Items[slot1];
            Item obj2 = this.m_Items[slot2];
            if (obj1 != null)
                obj1.Slot = slot2;
            if (obj2 != null)
                obj2.Slot = slot1;
            this.m_Items[slot1] = obj2;
            this.m_Items[slot2] = obj1;
            this.SetContainerEntityId(obj1 != null ? obj1.EntityId : EntityId.Zero, slot2, this);
            this.SetContainerEntityId(obj2 != null ? obj2.EntityId : EntityId.Zero, slot1, this);
        }

        public bool IsValidSlot(int slot)
        {
            if (slot >= 0)
                return slot < this.m_Items.Length;
            return false;
        }

        internal void RemovePlaceHolder(int slot)
        {
            this.m_Items[slot] = (Item) null;
        }

        /// <summary>
        /// Finds a free slot for the given template and occpuies it with a placeholder.
        /// Don't forget to remove it again.
        /// </summary>
        /// <param name="templ"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        internal SimpleSlotId HoldFreeSlot(ItemTemplate templ, int amount)
        {
            SimpleSlotId freeSlot = this.FindFreeSlot((IMountableItem) templ, amount);
            if (freeSlot.Slot != (int) byte.MaxValue)
                freeSlot.Container.m_Items[freeSlot.Slot] = Item.PlaceHolder;
            return freeSlot;
        }

        public virtual SimpleSlotId FindFreeSlot(IMountableItem item, int amount)
        {
            return new SimpleSlotId()
            {
                Container = this,
                Slot = this.FindFreeSlot()
            };
        }

        public virtual int FindFreeSlot()
        {
            for (int index = 0; index < this.m_Items.Length; ++index)
            {
                if (this.m_Items[index] == null)
                    return index;
            }

            return (int) byte.MaxValue;
        }

        /// <summary>Find a free slot within the given range</summary>
        public virtual int FindFreeSlot(int offset, int end)
        {
            for (int index = offset; index < end; ++index)
            {
                if (this.m_Items[index] == null)
                    return index;
            }

            return (int) byte.MaxValue;
        }

        /// <summary>Returns the IItemSlotHandler for the specified spot</summary>
        public abstract IItemSlotHandler GetHandler(int slot);

        public abstract PlayerInventory OwnerInventory { get; }

        /// <summary>
        /// Is called before adding the given amount of the given Item.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        /// <param name="err"></param>
        internal void CheckUniqueness(IMountableItem item, ref int amount, ref InventoryError err, bool isNew)
        {
            ItemTemplate template = item.Template;
            if (!isNew)
                return;
            if (template.UniqueCount > 0 &&
                this.OwnerInventory.GetUniqueCount(template.ItemId) + amount > template.UniqueCount)
            {
                amount -= template.UniqueCount;
                if (amount < 1)
                {
                    err = InventoryError.CANT_CARRY_MORE_OF_THIS;
                    return;
                }
            }

            if (item.Enchantments == null)
                return;
            for (EnchantSlot enchantSlot = EnchantSlot.Socket1; enchantSlot < EnchantSlot.Bonus; ++enchantSlot)
            {
                ItemEnchantment enchantment = item.Enchantments[(uint) enchantSlot];
                if (enchantment != null && enchantment.Entry.GemTemplate != null &&
                    (enchantment.Entry.GemTemplate.UniqueCount > 0 &&
                     this.OwnerInventory.GetUniqueCount(enchantment.Entry.GemTemplate.ItemId) >=
                     enchantment.Entry.GemTemplate.UniqueCount))
                {
                    err = InventoryError.CANT_CARRY_MORE_OF_THIS;
                    break;
                }
            }
        }

        /// <summary>
        /// Tries to add a new item with the given template and amount ot the given slot.
        /// Make sure the given targetSlot is valid before calling this method.
        /// If slot is occupied, method will find another unoccupied slot.
        /// </summary>
        /// <returns>The result (InventoryError.OK in case that it worked)</returns>
        public InventoryError TryAdd(Asda2ItemId id, ref int amount, InventorySlot targetSlot,
            ItemReceptionType reception = ItemReceptionType.Receive)
        {
            ItemTemplate template = ItemMgr.GetTemplate(id);
            if (template != null)
                return this.TryAdd(template, ref amount, (int) targetSlot, reception);
            return InventoryError.Invalid;
        }

        /// <summary>
        /// Tries to add ONE new item with the given template to the given slot.
        /// Make sure the given targetSlot is valid before calling this method.
        /// </summary>
        public InventoryError TryAdd(ItemTemplate template, InventorySlot targetSlot,
            ItemReceptionType reception = ItemReceptionType.Receive)
        {
            int amount = 1;
            return this.TryAdd(template, ref amount, (int) targetSlot, reception);
        }

        /// <summary>
        /// Tries to add a single new item with the given template to the given slot.
        /// Make sure the given targetSlot is valid before calling this method.
        /// </summary>
        public InventoryError TryAdd(ItemTemplate template, EquipmentSlot targetSlot,
            ItemReceptionType reception = ItemReceptionType.Receive)
        {
            int amount = 1;
            return this.TryAdd(template, ref amount, (int) targetSlot, reception);
        }

        /// <summary>
        /// Tries to add a new item with the given id to a free slot.
        /// </summary>
        /// <returns>The result (InventoryError.OK in case that it worked)</returns>
        public InventoryError TryAdd(Asda2ItemId id, ItemReceptionType reception = ItemReceptionType.Receive)
        {
            return this.TryAdd(ItemMgr.GetTemplate(id), reception);
        }

        /// <summary>
        /// Tries to add a new item with the given id to a free slot.
        /// </summary>
        /// <returns>The result (InventoryError.OK in case that it worked)</returns>
        public InventoryError TryAdd(ItemTemplate templ, ItemReceptionType reception = ItemReceptionType.Receive)
        {
            if (templ == null)
                return InventoryError.Invalid;
            SimpleSlotId freeSlot = this.FindFreeSlot((IMountableItem) templ, 1);
            if (freeSlot.Slot == (int) byte.MaxValue)
                return this.FullError;
            return freeSlot.Container.TryAdd(templ, freeSlot.Slot, reception);
        }

        /// <summary>
        /// Tries to add ONE new item with the given template to the given slot.
        /// Make sure the given targetSlot is valid before calling this method.
        /// </summary>
        /// <returns>The result (InventoryError.OK in case that it worked)</returns>
        public InventoryError TryAdd(Asda2ItemId id, int targetSlot,
            ItemReceptionType reception = ItemReceptionType.Receive)
        {
            ItemTemplate template = ItemMgr.GetTemplate(id);
            if (template != null)
                return this.TryAdd(template, targetSlot, reception);
            return InventoryError.Invalid;
        }

        /// <summary>
        /// Tries to add ONE new item with the given template to the given slot.
        /// Make sure the given targetSlot is valid before calling this method.
        /// </summary>
        /// <returns>The result (InventoryError.OK in case that it worked)</returns>
        public InventoryError TryAdd(Asda2ItemId id, InventorySlot targetSlot,
            ItemReceptionType reception = ItemReceptionType.Receive)
        {
            ItemTemplate template = ItemMgr.GetTemplate(id);
            if (template != null)
                return this.TryAdd(template, (int) targetSlot, reception);
            return InventoryError.ITEM_NOT_FOUND;
        }

        /// <summary>
        /// Tries to add an item with the given template and amount
        /// </summary>
        /// <returns>The result (InventoryError.OK in case that it worked)</returns>
        public InventoryError TryAdd(Asda2ItemId templId, ref int amount,
            ItemReceptionType reception = ItemReceptionType.Receive)
        {
            ItemTemplate template = ItemMgr.GetTemplate(templId);
            if (template != null)
                return this.TryAdd(template, ref amount, reception);
            return InventoryError.ITEM_NOT_FOUND;
        }

        /// <summary>
        /// Tries to add a new item with the given template and amount
        /// </summary>
        /// <returns>The result (InventoryError.OK in case that it worked)</returns>
        public InventoryError TryAdd(uint templId, ref int amount,
            ItemReceptionType reception = ItemReceptionType.Receive)
        {
            return this.TryAdd((Asda2ItemId) templId, ref amount, reception);
        }

        /// <summary>
        /// Tries to add ONE new item with the given template to the given slot.
        /// Make sure the given targetSlot is valid before calling this method.
        /// </summary>
        public InventoryError TryAdd(ItemTemplate template, int targetSlot,
            ItemReceptionType reception = ItemReceptionType.Receive)
        {
            int amount = 1;
            return this.TryAdd(template, ref amount, targetSlot, reception);
        }

        /// <summary>
        /// Tries to add an Item with the given template and amount ot the given slot.
        /// Make sure the given targetSlot is valid before calling this method.
        /// </summary>
        /// <returns>The result (InventoryError.OK in case that it worked)</returns>
        public InventoryError TryAdd(Asda2ItemId id, ref int amount, int targetSlot,
            ItemReceptionType reception = ItemReceptionType.Receive)
        {
            ItemTemplate template = ItemMgr.GetTemplate(id);
            if (template != null)
                return this.TryAdd(template, ref amount, targetSlot, reception);
            return InventoryError.ITEM_NOT_FOUND;
        }

        /// <summary>
        /// Tries to add a new item with the given template and amount ot the given slot.
        /// Make sure the given targetSlot is valid before calling this method.
        /// </summary>
        /// <returns>The result (InventoryError.OK in case that it worked)</returns>
        public InventoryError TryAdd(ItemTemplate template, ref int amount, int slot,
            ItemReceptionType reception = ItemReceptionType.Receive)
        {
            if (this.m_Items[slot] != null)
            {
                LogManager.GetCurrentClassLogger().Warn("Tried to add Item {0} to {1} in occupied slot {2}",
                    (object) template, (object) this, (object) slot);
                return InventoryError.ITEM_DOESNT_GO_TO_SLOT;
            }

            InventoryError err = InventoryError.OK;
            this.CheckUniqueness((IMountableItem) template, ref amount, ref err, true);
            if (err == InventoryError.OK)
            {
                IItemSlotHandler handler = this.GetHandler(slot);
                if (handler != null)
                {
                    err = InventoryError.OK;
                    handler.CheckAdd(slot, amount, (IMountableItem) template, ref err);
                    if (err != InventoryError.OK)
                        return err;
                }

                this.OnAdded(this.AddUnchecked(slot, template, amount, true), template, amount, reception);
            }

            return err;
        }

        /// <summary>
        /// Tries to add an item with the given template and amount
        /// </summary>
        /// <param name="amount">Amount of items to be added: Will be set to the amount of Items that have actually been added.</param>
        /// <returns>The result (InventoryError.OK in case that it worked)</returns>
        public InventoryError TryAdd(ItemTemplate template, ref int amount,
            ItemReceptionType reception = ItemReceptionType.Receive)
        {
            int amount1 = amount;
            InventoryError err = InventoryError.OK;
            Item obj = (Item) null;
            if (!this.Distribute(template, ref amount1))
            {
                amount -= amount1;
                this.CheckUniqueness((IMountableItem) template, ref amount, ref err, true);
                if (err == InventoryError.OK)
                {
                    SimpleSlotId freeSlot = this.FindFreeSlot((IMountableItem) template, amount);
                    if (freeSlot.Slot == (int) byte.MaxValue)
                        return this.FullError;
                    obj = freeSlot.Container.AddUnchecked(freeSlot.Slot, template, amount, true);
                }
            }

            this.OnAdded(obj, template, amount1 + (obj != null ? obj.Amount : 0), reception);
            return err;
        }

        /// <summary>
        /// Tries to distribute the given item over all available stacks and add the remainder to a free slot.
        /// IMPORTANT:
        /// 1. The Item will be destroyed if it could successfully be distributed over existing stacks of Items.
        /// 2. If item.Container == null, parts of the item-stack might have distributed over other stacks of the same type
        /// but the remainder did not find a free slot or exceeded the max count of the item.
        /// item.Amount will hold the remaining amount.
        /// </summary>
        /// <returns>The result (InventoryError.OK in case that it worked)</returns>
        public InventoryError TryAdd(Item item, bool isNew, ItemReceptionType reception = ItemReceptionType.Receive)
        {
            int amount1 = item.Amount;
            InventoryError err = InventoryError.OK;
            int amount2 = amount1;
            if (!this.Distribute(item.Template, ref amount2))
            {
                int amount3 = amount1 - amount2;
                item.Amount = amount3;
                this.CheckUniqueness((IMountableItem) item, ref amount3, ref err, isNew);
                if (err == InventoryError.OK)
                {
                    SimpleSlotId freeSlot = this.FindFreeSlot((IMountableItem) item, amount3);
                    if (freeSlot.Slot == (int) byte.MaxValue)
                        return this.FullError;
                    freeSlot.Container.AddUnchecked(freeSlot.Slot, item, isNew);
                }
            }
            else
            {
                int num = amount1 - amount2;
                item.Amount = num;
            }

            if (isNew)
                this.OnAdded(item, item.Template, amount2 + item.Amount, reception);
            return err;
        }

        /// <summary>
        /// Tries to distribute the given amount of the given Item over all available stacks and add the remainder to a free slot.
        /// Parts of the stack might have distributed over existing stacks, even if adding the remainder failed.
        /// </summary>
        /// <returns>InventoryError.OK in case that it could be added</returns>
        public InventoryError TryAddAmount(Item item, int amount, bool isNew,
            ItemReceptionType reception = ItemReceptionType.Receive)
        {
            int amount1 = amount;
            InventoryError err = InventoryError.OK;
            if (!this.Distribute(item.Template, ref amount1))
            {
                this.CheckUniqueness((IMountableItem) item, ref amount, ref err, isNew);
                if (err == InventoryError.OK)
                {
                    amount -= amount1;
                    item.Amount = amount;
                    SimpleSlotId freeSlot = this.FindFreeSlot((IMountableItem) item, amount);
                    if (freeSlot.Slot == (int) byte.MaxValue)
                        return InventoryError.BAG_FULL;
                    freeSlot.Container.AddUnchecked(freeSlot.Slot, item.Template, amount, isNew);
                }
            }
            else
            {
                amount -= amount1;
                item.Amount = amount;
            }

            if (isNew)
                this.OnAdded(item, item.Template, amount1 + amount, reception);
            return err;
        }

        /// <summary>
        /// Tries to add the given item to the given slot (make sure the slot is valid and not occupied).
        /// Fails if not all items of this stack can be added.
        /// </summary>
        /// <returns>InventoryError.OK in case that it worked</returns>
        public InventoryError TryAdd(int slot, Item item, bool isNew,
            ItemReceptionType reception = ItemReceptionType.Receive)
        {
            int amount = item.Amount;
            InventoryError err = this.CheckAdd(slot, (IMountableItem) item, amount);
            if (err == InventoryError.OK)
            {
                this.CheckUniqueness((IMountableItem) item, ref amount, ref err, isNew);
                if (err == InventoryError.OK && amount != item.Amount)
                {
                    err = InventoryError.CANT_CARRY_MORE_OF_THIS;
                }
                else
                {
                    this.AddUnchecked(slot, item, isNew);
                    if (isNew)
                        this.OnAdded(item, item.Template, amount, reception);
                }
            }

            return err;
        }

        public virtual void OnAdded(Item item, ItemTemplate templ, int amount,
            ItemReceptionType reception = ItemReceptionType.Receive)
        {
            if (item != null && item.IsBuyback)
                return;
            ItemHandler.SendItemPushResult(this.Owner, item, templ, amount, reception);
        }

        /// <summary>
        /// Tries to merge an item with the given template and amount to the stack at the given slot.
        /// If the given slot is empty it adds the item to the slot.
        /// Make sure the given targetSlot is valid before calling this method.
        /// </summary>
        /// <param name="amount">Set to the number of items actually added.</param>
        /// <returns>The result (InventoryError.OK in case that it worked)</returns>
        public InventoryError TryMerge(ItemTemplate template, ref int amount, int slot, bool isNew)
        {
            InventoryError err = InventoryError.OK;
            this.CheckUniqueness((IMountableItem) template, ref amount, ref err, isNew);
            if (err == InventoryError.OK)
            {
                IItemSlotHandler handler = this.GetHandler(slot);
                if (handler != null)
                {
                    err = InventoryError.OK;
                    handler.CheckAdd(slot, amount, (IMountableItem) template, ref err);
                    if (err != InventoryError.OK)
                        return err;
                }

                this.MergeUnchecked(slot, template, ref amount, isNew);
            }

            return err;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public InventoryError CheckAdd(int slot, IMountableItem item, int amount)
        {
            IItemSlotHandler handler = this.GetHandler(slot);
            if (handler == null)
                return InventoryError.OK;
            InventoryError err = InventoryError.OK;
            handler.CheckAdd(slot, amount, item, ref err);
            return err;
        }

        public Item AddUnchecked(int slot, Asda2ItemId id, int amount, bool isNew)
        {
            ItemTemplate template = ItemMgr.GetTemplate(id);
            return this.AddUnchecked(slot, template, amount, isNew);
        }

        /// <summary>
        /// Adds the Item to the given slot without any further checks.
        /// Make sure all parameters are valid (eg by calling <code>CheckAdd</code> beforehand)
        /// or use <code>TryAdd</code> instead.
        /// </summary>
        /// <param name="slot"></param>
        public Item AddUnchecked(int slot, ItemTemplate template, int amount, bool isNew)
        {
            Item obj = Item.CreateItem(template, this.Owner, amount);
            this[slot] = obj;
            if (isNew)
                this.OwnerInventory.OnNewStack(obj);
            return obj;
        }

        /// <summary>
        /// Adds an amount of Items with ItemTemplate to the Item in the given slot, without further checks.
        /// If the given slot is empty, it AddsUnchecked.
        /// </summary>
        /// <param name="amount">Set to the number of Items actually added.</param>
        /// <returns>The Item in the slot you merged to.</returns>
        public Item MergeUnchecked(int slot, ItemTemplate template, ref int amount, bool isNew)
        {
            Item obj = this.m_Items[slot];
            if (obj == null)
                return this.AddUnchecked(slot, template, amount, isNew);
            int num = Math.Min(obj.Template.MaxAmount - obj.Amount, amount);
            amount = num;
            obj.Amount += num;
            if (isNew)
                this.OwnerInventory.OnNewStack(obj);
            return obj;
        }

        /// <summary>
        /// Adds the Item to the given slot without any further checks.
        /// Make sure all parameters are valid (eg by calling <code>CheckAdd</code> beforehand)
        /// or use <code>TryAdd</code> instead.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="item"></param>
        public void AddUnchecked(int slot, Item item, bool isNew)
        {
            this[slot] = item;
            if (isNew)
            {
                this.OwnerInventory.OnNewStack(item);
            }
            else
            {
                Character owner = this.Owner;
                IContextHandler contextHandler = owner.ContextHandler;
                if (contextHandler == null)
                    return;
                contextHandler.AddMessage((Action) (() =>
                {
                    if (!owner.IsInWorld)
                        return;
                    owner.AddItemToUpdate(item);
                }));
            }
        }

        internal void AddLoadedItem(Item item)
        {
            try
            {
                int slot = item.Slot;
                BaseInventory baseInventory = this;
                if (!this.IsValidSlot(slot))
                {
                    SimpleSlotId freeSlot = this.OwnerInventory.FindFreeSlot(item, false);
                    if (slot == (int) byte.MaxValue)
                    {
                        LogManager.GetCurrentClassLogger()
                            .Warn("Ignoring loaded Item {0} in {1} because it has an invalid Slot: {2}", (object) item,
                                (object) this, (object) item.Slot);
                        return;
                    }

                    LogManager.GetCurrentClassLogger().Warn("Loaded Item {0} in {1} has invalid Slot: {2}",
                        (object) item, (object) this, (object) item.Slot);
                    slot = freeSlot.Slot;
                    baseInventory = freeSlot.Container;
                }

                if (baseInventory[slot] != null)
                {
                    LogManager.GetCurrentClassLogger()
                        .Warn("Ignoring Item {0} for {1} because slot is already occupied by: {2}", (object) item,
                            (object) this.Owner, (object) baseInventory[slot]);
                    item.Destroy();
                }
                else
                {
                    Character owner = this.Owner;
                    PlayerInventory ownerInventory = this.OwnerInventory;
                    baseInventory[slot] = item;
                    owner.AddItemToUpdate(item);
                    ownerInventory.OnAddDontNotify(item);
                }
            }
            catch (Exception ex)
            {
                LogUtil.ErrorException(ex, "Unable to add Item \"{0}\" to {1}", new object[2]
                {
                    (object) item,
                    (object) this
                });
            }
        }

        public Item Remove(InventorySlot slot, bool ownerChange)
        {
            return this.Remove((int) slot, ownerChange);
        }

        /// <summary>
        /// Make sure that you have a valid slot before calling this method (see IsValidSlot).
        /// </summary>
        /// <param name="ownerChange">whether the owner will change</param>
        public Item Remove(int slot, bool ownerChange)
        {
            if (slot >= this.m_Items.Length)
            {
                LogUtil.ErrorException(new Exception(string.Format(
                    "Tried to remove Item from invalid Slot {0}/{1} in {2} (belongs to {3})", (object) slot,
                    (object) this.m_Items.Length, (object) this, (object) this.Owner)));
                return (Item) null;
            }

            Item obj = this.m_Items[slot];
            if (obj != null)
                obj.Remove(ownerChange);
            return obj;
        }

        /// <summary>Don't use this method - but use item.Remove instead.</summary>
        /// <param name="item"></param>
        /// <param name="ownerChange"></param>
        internal void Remove(Item item, bool ownerChange)
        {
            if (ownerChange)
            {
                item.m_unknown = true;
                this.OwnerInventory.OnRemove(item);
            }

            int slot1 = item.Slot;
            IItemSlotHandler handler = this.GetHandler(slot1);
            if (handler != null)
                handler.Removed(slot1, item);
            int slot2 = item.Slot;
            this.m_Items[slot2] = (Item) null;
            if (this.Container != null)
                this.SetContainerEntityId(EntityId.Zero, slot2, this);
            item.Container = (BaseInventory) null;
            --this.m_count;
        }

        /// <summary>
        /// Checks for all requirements before destroying the Item in the given slot.
        /// If it cannot be destroyed, sends error to owner.
        /// </summary>
        public InventoryError TryDestroy(int slot)
        {
            Item obj = this[slot];
            InventoryError error;
            if (obj == null)
                error = InventoryError.ITEM_NOT_FOUND;
            else if (obj.IsContainer && ((WCell.RealmServer.Entities.Container) obj).BaseInventory.Count > 0)
                error = InventoryError.CAN_ONLY_DO_WITH_EMPTY_BAGS;
            else if (!obj.CanBeTraded)
            {
                error = InventoryError.CANT_DO_RIGHT_NOW;
            }
            else
            {
                if (this.Destroy(slot))
                    return InventoryError.OK;
                error = InventoryError.DontReport;
            }

            ItemHandler.SendInventoryError((IPacketReceiver) this.Owner.Client, obj, (Item) null, error);
            return error;
        }

        /// <summary>Destroys the item at the given slot</summary>
        public bool Destroy(int slot)
        {
            Item obj1 = this.Remove(slot, true);
            if (obj1 == null)
                return false;
            if (obj1 is WCell.RealmServer.Entities.Container)
            {
                WCell.RealmServer.Entities.Container container = (WCell.RealmServer.Entities.Container) obj1;
                if (!container.BaseInventory.IsEmpty)
                {
                    foreach (Item obj2 in container.BaseInventory)
                        obj2.Destroy();
                }
            }

            obj1.DoDestroy();
            return true;
        }

        public InventoryError GetSlots<T>(T[] items, out SimpleSlotId[] slots) where T : IItemStack
        {
            int length = items.Length;
            InventoryError err = InventoryError.OK;
            int index1 = 0;
            slots = new SimpleSlotId[length];
            SimpleSlotId freeSlot;
            for (; index1 < length; ++index1)
            {
                T obj = items[index1];
                ItemTemplate template = obj.Template;
                int amount = obj.Amount;
                this.CheckUniqueness((IMountableItem) template, ref amount, ref err, true);
                if (err == InventoryError.OK && amount != obj.Amount)
                {
                    err = InventoryError.CANT_CARRY_MORE_OF_THIS;
                    break;
                }

                freeSlot = this.FindFreeSlot((IMountableItem) template, amount);
                if (freeSlot.Slot == (int) byte.MaxValue)
                {
                    err = InventoryError.INVENTORY_FULL;
                    break;
                }

                freeSlot.Container.m_Items[freeSlot.Slot] = Item.PlaceHolder;
                slots[index1] = freeSlot;
            }

            if (err != InventoryError.OK)
            {
                for (int index2 = 0; index2 < index1; ++index2)
                {
                    freeSlot = slots[index2];
                    freeSlot.Container.m_Items[freeSlot.Slot] = (Item) null;
                }

                slots = (SimpleSlotId[]) null;
            }

            return err;
        }

        /// <summary>
        /// Adds all given Items to the given slots.
        /// Tries to distribute over existing stacks first.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public void AddAllUnchecked<T>(T[] items, SimpleSlotId[] slots) where T : IItemStack
        {
            int length = items.Length;
            for (int index = 0; index < length; ++index)
            {
                T obj = items[index];
                int amount1 = obj.Amount;
                if (!this.Distribute(obj.Template, ref amount1))
                {
                    SimpleSlotId slot = slots[index];
                    int amount2 = obj.Amount - amount1;
                    slot.Container.m_Items[slot.Slot] = (Item) null;
                    slot.Container.AddUnchecked(slot.Slot, items[index].Template, amount2, true);
                }
            }
        }

        /// <summary>
        /// Tries to add all given Items to this Inventory.
        /// Does not add any if not all could be added.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public InventoryError TryAddAll<T>(T[] items) where T : IItemStack
        {
            SimpleSlotId[] slots1;
            InventoryError slots2 = this.GetSlots<T>(items, out slots1);
            if (slots2 == InventoryError.OK)
                this.AddAllUnchecked<T>(items, slots1);
            return slots2;
        }

        /// <summary>
        /// Tries to distribute Items of the given Template and amount amongst all other stacks of the same Type
        /// </summary>
        /// <param name="amount">Will be set to the amount that has actually been distributed</param>
        /// <returns>whether the complete amount has been fully distributed.</returns>
        public virtual bool Distribute(ItemTemplate template, ref int amount)
        {
            if (template.IsStackable)
            {
                int amountLeft = amount;
                bool done = false;
                this.Iterate((Func<Item, bool>) (invItem =>
                {
                    if (invItem.Template == template)
                    {
                        int num = template.MaxAmount - invItem.Amount;
                        if (num > 0)
                        {
                            if (num >= amountLeft)
                            {
                                amountLeft -= invItem.ModAmount(amountLeft);
                                done = true;
                                return false;
                            }

                            amountLeft -= invItem.ModAmount(num);
                        }
                    }

                    return true;
                }));
                amount -= amountLeft;
                return done;
            }

            amount = 0;
            return false;
        }

        /// <summary>
        /// Checks whether Items of the given Template and amount can be distributed
        /// amongst already existing stacks of the same Type without actually changing anything.
        /// </summary>
        /// <param name="amount">The number of items to try and distribute.
        /// Will be set to the number of items that would be distributed if a real Distribute is run.</param>
        /// <returns>true if the whole amount can be distributed.</returns>
        public bool CheckDistribute(ItemTemplate template, ref int amount)
        {
            int amountLeft = amount;
            if (template.IsStackable)
                this.Iterate((Func<Item, bool>) (invItem =>
                {
                    if (invItem.Template == template && invItem.Amount < template.MaxAmount)
                    {
                        int num = template.MaxAmount - invItem.Amount;
                        if (num >= amountLeft)
                            return false;
                        amountLeft -= num;
                    }

                    return true;
                }));
            amount -= amountLeft;
            return amountLeft == 0;
        }

        public virtual void SetContainerEntityId(EntityId entityId, int slot, BaseInventory baseInventory)
        {
            int field = baseInventory.m_baseField + slot * 2;
            baseInventory.Container.SetEntityId(field, entityId);
        }

        /// <summary>
        /// Counts and returns the amount of items in between the given slots.
        /// </summary>
        public int GetCount(int offset, int end)
        {
            int num = 0;
            for (int index = offset; index <= end; ++index)
            {
                if (this.m_Items[index] != null)
                    ++num;
            }

            return num;
        }

        /// <summary>Iterates over all Items within this Inventory.</summary>
        public virtual bool Iterate(Func<Item, bool> iterator)
        {
            for (int index = 0; index < this.m_Items.Length; ++index)
            {
                Item obj1 = this.m_Items[index];
                if (obj1 != null)
                {
                    if (obj1 is WCell.RealmServer.Entities.Container)
                    {
                        BaseInventory baseInventory = ((WCell.RealmServer.Entities.Container) obj1).BaseInventory;
                        if (!baseInventory.IsEmpty)
                        {
                            foreach (Item obj2 in baseInventory.Items)
                            {
                                obj1 = obj2;
                                if (obj1 != null && !iterator(obj1))
                                    return false;
                            }
                        }
                    }

                    if (!iterator(obj1))
                        return false;
                }
            }

            return true;
        }

        /// <summary>All items that are currently in this inventory</summary>
        public virtual IEnumerator<Item> GetEnumerator()
        {
            for (int i = 0; i < this.m_Items.Length; ++i)
            {
                Item item = this.m_Items[i];
                if (item != null)
                    yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) this.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Format("Inventory of {0}: {1}", (object) this.Owner, (object) this.ToString<Item>(" / "));
        }
    }
}
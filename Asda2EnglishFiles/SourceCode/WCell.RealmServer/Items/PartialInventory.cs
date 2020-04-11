using System;
using System.Collections;
using System.Collections.Generic;
using WCell.Constants.Items;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Items
{
    /// <summary>
    /// A part of another inventory (eg backpack, bank, equipment, buyback etc are all parts of the PlayerInventory)
    /// </summary>
    public abstract class PartialInventory : IInventory, IList<Item>, ICollection<Item>, IEnumerable<Item>, IEnumerable
    {
        protected PlayerInventory m_inventory;

        protected PartialInventory(PlayerInventory baseInventory)
        {
            this.m_inventory = baseInventory;
        }

        /// <summary>
        /// The offset within the underlying inventory of what this part occupies.
        /// </summary>
        public abstract int Offset { get; }

        /// <summary>
        /// The end within the underlying inventory of what this part occupies
        /// </summary>
        public abstract int End { get; }

        /// <summary>The actual owner of this srcCont.</summary>
        public Character Owner
        {
            get { return this.m_inventory.Owner; }
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

        Item IList<Item>.this[int index]
        {
            get { return this[index]; }
            set { this[index] = value; }
        }

        /// <summary>Gets the corresponding slot without further checks</summary>
        public Item this[int slot]
        {
            get { return this.m_inventory[slot]; }
            internal set { this.m_inventory.Items[slot] = value; }
        }

        public Container GetBag(int slot)
        {
            return this.m_inventory[this.Offset + slot] as Container;
        }

        /// <summary>A copy of the items of this PartialInventory.</summary>
        public Item[] Items
        {
            get
            {
                Item[] objArray = new Item[this.MaxCount];
                Array.Copy((Array) this.m_inventory.Items, this.Offset, (Array) objArray, 0, objArray.Length);
                return objArray;
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
            throw new InvalidOperationException();
        }

        public void CopyTo(Item[] array, int arrayIndex)
        {
            throw new InvalidOperationException();
        }

        public bool Remove(Item item)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// The amount of items, currently in this part of the inventory.
        /// Recounts everytime.
        /// </summary>
        public int Count
        {
            get { return this.m_inventory.GetCount(this.Offset, this.End); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// The maximum amount of items, supported by this inventory
        /// </summary>
        public int MaxCount
        {
            get { return this.End - this.Offset + 1; }
        }

        /// <summary>whether there are no items in this inventory</summary>
        public bool IsEmpty
        {
            get
            {
                foreach (Item obj in this.m_inventory.Items)
                {
                    if (obj != null)
                        return false;
                }

                return true;
            }
        }

        /// <summary>whether there is no space left in this inventory</summary>
        public bool IsFull
        {
            get { return this.Count >= this.MaxCount; }
        }

        /// <summary>
        /// Returns the next free slot in this part of the inventory, else BaseInventory.INVALID_SLOT
        /// </summary>
        public virtual int FindFreeSlot()
        {
            for (int offset = this.Offset; offset <= this.End; ++offset)
            {
                if (this.m_inventory.Items[offset] == null)
                    return offset;
            }

            return (int) byte.MaxValue;
        }

        /// <summary>
        /// Returns whether the given slot is valid to access items of this inventory
        /// </summary>
        public bool IsValidSlot(int slot)
        {
            if (slot >= 0)
                return slot <= this.MaxCount;
            return false;
        }

        /// <summary>
        /// Tries to add the given item to the corresponding slot: The offset of this inventory + the given slot
        /// </summary>
        /// <returns>whether the item could be added</returns>
        public virtual InventoryError TryAdd(int slot, Item item, bool isNew,
            ItemReceptionType reception = ItemReceptionType.Receive)
        {
            return this.m_inventory.TryAdd(slot, item, isNew, reception);
        }

        /// <summary>Tries to add the item to a free slot in this srcCont</summary>
        /// <returns>whether the item could be added</returns>
        public virtual InventoryError TryAdd(Item item, bool isNew,
            ItemReceptionType reception = ItemReceptionType.Receive)
        {
            return this.m_inventory.TryAdd(this.FindFreeSlot(), item, isNew, reception);
        }

        /// <summary>
        /// Removes the item at the given slot. If you intend to enable the user continuing to use that item, do not use this method
        /// but use PlayerInventory.TrySwap instead.
        /// </summary>
        /// <returns>whether there was an item to be removed and removal was successful</returns>
        public Item Remove(int slot, bool ownerChange)
        {
            return this.m_inventory.Remove(slot, ownerChange);
        }

        public Container RemoveBag(int slot, bool fullRemove)
        {
            return this.m_inventory.Remove(slot + this.Offset, fullRemove) as Container;
        }

        /// <summary>
        /// Deletes the item in the given slot (item cannot be re-used afterwards).
        /// </summary>
        /// <returns>whether the given item could be deleted</returns>
        public bool Destroy(int slot)
        {
            return this.m_inventory.Destroy(slot);
        }

        /// <summary>
        /// Returns all existing inventories (only containers have them)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BaseInventory> GetInventories()
        {
            foreach (Item obj in this.m_inventory.Items)
            {
                if (obj is Container)
                {
                    BaseInventory inv = ((Container) obj).BaseInventory;
                    if (inv != null)
                        yield return inv;
                }
            }
        }

        public bool Contains(Asda2ItemId id)
        {
            foreach (Item obj in this.m_inventory.Items)
            {
                if (obj != null && (Asda2ItemId) obj.EntryId == id)
                    return true;
            }

            return false;
        }

        public IEnumerator<Item> GetEnumerator()
        {
            foreach (Item obj in this.m_inventory.Items)
            {
                if (obj != null)
                    yield return obj;
            }
        }

        public override string ToString()
        {
            return this.GetType().Name + " of " + (object) this.Owner;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) this.GetEnumerator();
        }
    }
}
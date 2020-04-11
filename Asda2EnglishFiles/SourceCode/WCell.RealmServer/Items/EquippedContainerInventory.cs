using WCell.Constants.Items;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Items
{
    /// <summary>Represents all equippable bags</summary>
    public class EquippedContainerInventory : PartialInventory, IItemSlotHandler
    {
        public EquippedContainerInventory(PlayerInventory baseInventory)
            : base(baseInventory)
        {
        }

        public override int Offset
        {
            get { return 19; }
        }

        public override int End
        {
            get { return 22; }
        }

        /// <summary>
        /// Is called before adding to check whether the item may be added to the corresponding slot
        /// (given the case that the corresponding slot is valid and unoccupied)
        /// </summary>
        public virtual void CheckAdd(int slot, int amount, IMountableItem item, ref InventoryError err)
        {
            ItemTemplate template = item.Template;
            err = template.CheckEquip(this.Owner);
            if (err != InventoryError.OK)
                return;
            if (!template.IsBag)
                err = InventoryError.NOT_A_BAG;
            else if (this.m_inventory[slot] != null)
            {
                err = InventoryError.NONEMPTY_BAG_OVER_OTHER_BAG;
            }
            else
            {
                if (item.IsEquipped)
                    return;
                err = this.m_inventory.CheckEquipCount(item);
            }
        }

        /// <summary>Is called before a bag is removed</summary>
        public void CheckRemove(int slot, IMountableItem item, ref InventoryError err)
        {
            Container container = item as Container;
            if (container == null || container.BaseInventory.IsEmpty)
                return;
            err = InventoryError.CAN_ONLY_DO_WITH_EMPTY_BAGS;
        }

        /// <summary>
        /// Is called after the given item is added to the given slot
        /// </summary>
        public void Added(Item item)
        {
            item.OnEquipDecision();
        }

        /// <summary>
        /// Is called after the given item is removed from the given slot
        /// </summary>
        public void Removed(int slot, Item item)
        {
            item.OnUnequipDecision((InventorySlot) slot);
        }
    }
}
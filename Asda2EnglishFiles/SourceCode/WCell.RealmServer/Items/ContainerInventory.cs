using WCell.Constants.Items;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Items
{
    public class ContainerInventory : BaseInventory, IItemSlotHandler
    {
        /// <summary>16 Initial item slots</summary>
        public ContainerInventory(Container owner, UpdateFieldId baseField, int slots)
            : base((IContainer) owner, baseField, slots)
        {
        }

        /// <summary>Returns the IItemSlotHandler for the specified spot</summary>
        public override IItemSlotHandler GetHandler(int slot)
        {
            if (this.IsValidSlot(slot))
                return (IItemSlotHandler) this;
            return (IItemSlotHandler) null;
        }

        public override PlayerInventory OwnerInventory
        {
            get { return this.Owner.Inventory; }
        }

        public override InventoryError FullError
        {
            get { return InventoryError.BAG_FULL; }
        }

        /// <summary>
        /// Is called before adding to check whether the item may be added to the corresponding slot
        /// (given the case that the corresponding slot is valid and unoccupied)
        /// </summary>
        public void CheckAdd(int slot, int amount, IMountableItem item, ref InventoryError err)
        {
            if (item.Template.IsKey)
                err = InventoryError.ITEM_DOESNT_GO_INTO_BAG;
            else if (item.Template.IsContainer)
            {
                err = InventoryError.NONEMPTY_BAG_OVER_OTHER_BAG;
            }
            else
            {
                if (err != InventoryError.OK || !(this.m_container is Item) ||
                    ((Item) this.m_container).Template.MayAddToContainer(item.Template))
                    return;
                err = InventoryError.ITEM_DOESNT_GO_INTO_BAG;
            }
        }

        /// <summary>
        /// Is called before removing the given item to check whether it may actually be removed
        /// </summary>
        public void CheckRemove(int slot, IMountableItem item, ref InventoryError err)
        {
        }

        /// <summary>
        /// Is called after the given item is added to the given slot
        /// </summary>
        public void Added(Item item)
        {
        }

        /// <summary>
        /// Is called after the given item is removed from the given slot
        /// </summary>
        public void Removed(int slot, Item item)
        {
        }

        public override string ToString()
        {
            return this.GetType().Name + " " + (object) this.Container + " of " + (object) this.Owner;
        }
    }
}
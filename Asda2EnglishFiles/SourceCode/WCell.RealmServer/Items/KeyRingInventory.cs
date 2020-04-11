using WCell.Constants.Items;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Items
{
    /// <summary>Represents the keyring</summary>
    public class KeyRingInventory : PartialInventory, IItemSlotHandler
    {
        public KeyRingInventory(PlayerInventory baseInventory)
            : base(baseInventory)
        {
        }

        public override int Offset
        {
            get { return 86; }
        }

        public override int End
        {
            get { return 117; }
        }

        /// <summary>
        /// Is called before adding to check whether the item may be added to the corresponding slot
        /// (given the case that the corresponding slot is valid and unoccupied)
        /// </summary>
        public void CheckAdd(int slot, int amount, IMountableItem item, ref InventoryError err)
        {
            if (item.Template.Class == ItemClass.Key)
                return;
            err = InventoryError.ITEM_DOESNT_GO_TO_SLOT;
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
    }
}
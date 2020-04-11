using WCell.Constants.Items;

namespace WCell.RealmServer.Items
{
    public struct SlotItemInfo
    {
        public ItemTemplate Template;
        public uint Amount;
        public InventorySlot Slot;

        public override string ToString()
        {
            return (this.Amount > 0U ? (object) (((int) this.Amount).ToString() + "x ") : (object) "").ToString() +
                   (object) this.Template + " at " + (object) this.Slot;
        }
    }
}
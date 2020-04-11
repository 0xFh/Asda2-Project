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
      return (Amount > 0U ? ((int) Amount) + "x " : "") +
             Template + " at " + Slot;
    }
  }
}
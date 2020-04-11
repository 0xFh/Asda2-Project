using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Items
{
  /// <summary>
  /// Represents a unique slot-descriptor per PlayerInventory (the slot number and its containing BaseInventory)
  /// </summary>
  public struct SimpleSlotId
  {
    public static readonly SimpleSlotId Default = new SimpleSlotId
    {
      Slot = byte.MaxValue
    };

    public BaseInventory Container;
    public int Slot;

    /// <summary>The item at this slot</summary>
    public Item Item
    {
      get { return Container[Slot]; }
    }

    public override string ToString()
    {
      return string.Format("Container: {0}, Slot: {1}, Item: {2}", Container, Slot,
        Item);
    }
  }
}
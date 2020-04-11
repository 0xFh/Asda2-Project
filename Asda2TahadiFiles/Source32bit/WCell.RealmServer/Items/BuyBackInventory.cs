using System;
using WCell.Constants.Items;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Items
{
  /// <summary>
  /// Represents all BuyBack items (that can be re-purchased at a vendor)
  /// </summary>
  public class BuyBackInventory : PartialInventory, IItemSlotHandler
  {
    public BuyBackInventory(PlayerInventory baseInventory)
      : base(baseInventory)
    {
    }

    public override int Offset
    {
      get { return 74; }
    }

    public override int End
    {
      get { return 85; }
    }

    public override InventoryError TryAdd(int slot, Item item, bool isNew,
      ItemReceptionType reception = ItemReceptionType.Receive)
    {
      return AddBuyBackItem(slot, item, isNew);
    }

    public override InventoryError TryAdd(Item item, bool isNew,
      ItemReceptionType reception = ItemReceptionType.Receive)
    {
      return AddBuyBackItem(item, isNew);
    }

    /// <summary>
    /// Adds an Item to the BuyBack inventory. If the BuyBack is full, the item in BuyBackSlot1 is destroyed
    /// and the items in the other slots are shifted up one to free-up the BuyBackLast slot.
    /// This method should only be called by the CMSG_SELL_ITEM handler.
    /// </summary>
    /// <param name="item">The item to Add to the BuyBack PartialInventory</param>
    public InventoryError AddBuyBackItem(Item item, bool isNew)
    {
      int slot = FindFreeSlot();
      if(slot == byte.MaxValue)
        slot = PushBack();
      return AddBuyBackItem(slot, item, isNew);
    }

    public InventoryError AddBuyBackItem(int slot, Item item, bool isNew)
    {
      if(!isNew)
        item.Remove(true);
      m_inventory.AddUnchecked(slot, item, isNew);
      return InventoryError.OK;
    }

    /// <summary>
    /// The BuyBack inventory is full. In order to make room for further items, push the top item off the list
    /// and then move every other item up one slot, thereby freeing the last slot.
    /// </summary>
    /// <returns></returns>
    private int PushBack()
    {
      Destroy(Offset);
      return End;
    }

    private uint GetBuyBackPriceField(int buyBackSlot)
    {
      return m_inventory.Owner.GetUInt32((PlayerFields) (1201 + (buyBackSlot - 74)));
    }

    private void SetBuyBackPriceField(int buyBackSlot, uint price)
    {
      m_inventory.Owner.SetUInt32((PlayerFields) (1201 + (buyBackSlot - 74)), price);
    }

    private DateTime GetBuyBackTimeStampField(int buyBackSlot)
    {
      return m_inventory.Owner.LastLogin.AddSeconds(
        m_inventory.Owner.GetUInt32((PlayerFields) (1213 + (buyBackSlot - 74))));
    }

    private void SetBuyBackTimeStampField(int buyBackSlot)
    {
      uint secondsFromLogin = (uint) ((DateTime.Now - m_inventory.Owner.LastLogin).Seconds + 108000);
      SetBuyBackTimeStampField(buyBackSlot, secondsFromLogin);
    }

    private void SetBuyBackTimeStampField(int buyBackSlot, DateTime stamp)
    {
      uint seconds = (uint) (stamp - m_inventory.Owner.LastLogin).Seconds;
      SetBuyBackTimeStampField(buyBackSlot, seconds);
    }

    private void SetBuyBackTimeStampField(int buyBackSlot, uint secondsFromLogin)
    {
      m_inventory.Owner.SetUInt32((PlayerFields) (1213 + (buyBackSlot - 74)),
        secondsFromLogin);
    }

    private void RemoveItemFromBuyBackField(int buyBackSlot)
    {
      SetBuyBackPriceField(buyBackSlot, 0U);
      SetBuyBackTimeStampField(buyBackSlot, 0U);
    }

    public void MoveItem(int from, int to)
    {
      uint buyBackPriceField = GetBuyBackPriceField(from);
      DateTime backTimeStampField = GetBuyBackTimeStampField(from);
      RemoveItemFromBuyBackField(from);
      SetBuyBackPriceField(to, buyBackPriceField);
      SetBuyBackTimeStampField(to, backTimeStampField);
    }

    /// <summary>
    /// Is called before adding to check whether the item may be added to the corresponding slot
    /// (given the case that the corresponding slot is valid and unoccupied)
    /// </summary>
    public void CheckAdd(int slot, int amount, IMountableItem item, ref InventoryError err)
    {
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
      int slot = item.Slot;
      if(slot < Offset || slot > End)
        return;
      SetBuyBackPriceField(slot, item.Template.SellPrice * (uint) item.Amount);
      SetBuyBackTimeStampField(slot);
    }

    /// <summary>
    /// Is called after the given item is removed from the given slot
    /// </summary>
    public void Removed(int slot, Item item)
    {
      RemoveItemFromBuyBackField(slot);
      for(int index = slot; index < End; ++index)
      {
        if(m_inventory[index + 1] != null)
        {
          MoveItem(index + 1, index);
          m_inventory.SwapUnnotified(index + 1, index);
        }
      }
    }
  }
}
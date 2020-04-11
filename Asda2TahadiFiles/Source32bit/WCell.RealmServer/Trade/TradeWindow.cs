using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Trade
{
  /// <summary>
  /// Represents the progress of trading between two characters
  /// Each trading party has its own instance of a TradeWindow
  /// </summary>
  public class TradeWindow
  {
    private readonly Item[] m_items;
    private bool m_accepted;
    private Character m_chr;
    private uint m_money;
    internal TradeWindow m_otherWindow;

    internal TradeWindow(Character owner)
    {
      m_chr = owner;
      m_items = new Item[7];
    }

    public Character Owner
    {
      get { return m_chr; }
    }

    public bool Accepted
    {
      get { return m_accepted; }
    }

    /// <summary>Other party of the trading progress</summary>
    public TradeWindow OtherWindow
    {
      get { return m_otherWindow; }
    }

    /// <summary>Accepts the proposition of trade</summary>
    public void AcceptTradeProposal()
    {
      SendStatus(TradeStatus.Initiated, true);
    }

    /// <summary>Cancels the trade</summary>
    public void Cancel(TradeStatus status = TradeStatus.Cancelled)
    {
      StopTrade(status, true);
    }

    /// <summary>Puts an item into the trading window</summary>
    /// <param name="tradeSlot">slot in the trading window</param>
    /// <param name="bag">inventory bag number</param>
    /// <param name="slot">inventory slot number</param>
    public void SetTradeItem(byte tradeSlot, byte bag, byte slot, bool updateSelf = true)
    {
    }

    /// <summary>Removes an item from the trading window</summary>
    /// <param name="tradeSlot">slot in the trading window</param>
    public void ClearTradeItem(byte tradeSlot, bool updateSelf = true)
    {
      if(tradeSlot >= 7)
        return;
      m_items[tradeSlot] = null;
      SendTradeInfo(updateSelf);
    }

    /// <summary>Changes the amount of money to trade</summary>
    /// <param name="money">new amount of coins</param>
    public void SetMoney(uint money, bool updateSelf = true)
    {
      m_money = money;
      SendTradeInfo(updateSelf);
    }

    /// <summary>
    /// Accepts the trade
    /// If both parties have accepted, commits the trade
    /// </summary>
    public void AcceptTrade(bool updateSelf = true)
    {
      m_accepted = true;
      if(!CheckMoney() || !CheckItems())
        return;
      if(OtherWindow.m_accepted)
        CommitTrade();
      else
        SendStatus(TradeStatus.Accepted, updateSelf);
    }

    /// <summary>
    /// Unaccepts the trade (usually due to change of traded items or amount of money)
    /// </summary>
    public void UnacceptTrade(bool updateSelf = true)
    {
      m_accepted = false;
      SendStatus(TradeStatus.StateChanged, updateSelf);
    }

    /// <summary>
    /// Sends the notification about the change of trade status and stops the trade
    /// </summary>
    /// <param name="status">new status</param>
    /// <param name="notifySelf">whether to notify the caller himself</param>
    internal void StopTrade(TradeStatus status, bool notifySelf)
    {
      SendStatus(status, notifySelf);
      m_chr.TradeWindow = null;
      m_chr = null;
      OtherWindow.m_chr.TradeWindow = null;
      OtherWindow.m_chr = null;
    }

    /// <summary>
    /// Checks if the party has enough money for trade
    /// If not, unaccepts the trade
    /// </summary>
    /// <returns></returns>
    private bool CheckMoney()
    {
      if(m_money <= m_chr.Money)
        return true;
      m_chr.SendSystemMessage("Not enough gold");
      m_accepted = false;
      TradeHandler.SendTradeStatus(m_chr.Client, TradeStatus.StateChanged);
      return false;
    }

    /// <summary>
    /// Checks if the party can trade selected items
    /// If not, unaccepts the trade
    /// </summary>
    /// <returns></returns>
    private bool CheckItems()
    {
      for(int index = 0; index < 7; ++index)
      {
        if(m_items[index] != null && !m_items[index].CanBeTraded)
        {
          m_accepted = false;
          TradeHandler.SendTradeStatus(m_chr.Client, TradeStatus.StateChanged);
          return false;
        }
      }

      return true;
    }

    /// <summary>Commits the trade between two parties</summary>
    private void CommitTrade()
    {
      TradeHandler.SendTradeStatus(OtherWindow.m_chr.Client, TradeStatus.Accepted);
      IList<SimpleSlotId> slotsForTradedItems1 = GetSlotsForTradedItems();
      IList<SimpleSlotId> slotsForTradedItems2 = m_otherWindow.GetSlotsForTradedItems();
      if(!CheckFreeSlots(slotsForTradedItems1, OtherWindow.m_items) ||
         !m_otherWindow.CheckFreeSlots(slotsForTradedItems2, m_items))
      {
        m_accepted = false;
      }
      else
      {
        TransferItems(slotsForTradedItems1, m_otherWindow.m_items);
        m_otherWindow.TransferItems(slotsForTradedItems2, m_items);
        GiveMoney();
        m_otherWindow.GiveMoney();
        StopTrade(TradeStatus.Complete, true);
      }
    }

    private bool CheckFreeSlots(IList<SimpleSlotId> myFreeSlots, Item[] items)
    {
      bool flag = true;
      int index1 = 0;
      for(int index2 = 0; index2 < 6; ++index2)
      {
        if(items[index2] != null)
        {
          if(myFreeSlots.Count <= index1)
          {
            flag = false;
            break;
          }

          int slot = myFreeSlots[index1].Slot;
          BaseInventory container = myFreeSlots[index1].Container;
          IItemSlotHandler handler = container.GetHandler(slot);
          InventoryError err = InventoryError.OK;
          handler.CheckAdd(slot, items[index2].Amount, items[index2], ref err);
          if(err != InventoryError.OK)
          {
            flag = false;
            break;
          }

          int amount = items[index2].Amount;
          container.CheckUniqueness(items[index2], ref amount, ref err, true);
          if(err != InventoryError.OK)
          {
            flag = false;
            break;
          }

          ++index1;
        }
      }

      if(!flag)
      {
        m_chr.SendSystemMessage("You don't have enough free slots");
        m_otherWindow.m_chr.SendSystemMessage("Other party doesn't have enough free slots");
        SendStatus(TradeStatus.StateChanged, true);
      }

      return flag;
    }

    private IList<SimpleSlotId> GetSlotsForTradedItems()
    {
      return null;
    }

    private void GiveMoney()
    {
    }

    private void TransferItems(IList<SimpleSlotId> simpleSlots, Item[] items)
    {
      int index1 = 0;
      for(int index2 = 0; index2 < items.Length; ++index2)
      {
        Item obj = items[index2];
        if(obj != null)
        {
          SimpleSlotId simpleSlot = simpleSlots[index1];
          int amount = obj.Amount;
          simpleSlot.Container.Distribute(obj.Template, ref amount);
          if(amount < obj.Amount)
          {
            obj.Remove(true);
            obj.Amount -= amount;
            simpleSlot.Container.AddUnchecked(simpleSlot.Slot, obj, true);
          }
          else
            obj.Destroy();

          ++index1;
        }
      }
    }

    /// <summary>
    /// Sends new information about the trading process to other party
    /// </summary>
    private void SendTradeInfo(bool updateSelf)
    {
      if(updateSelf)
      {
        TradeHandler.SendTradeUpdate(m_chr.Client, false, m_money, m_items);
        TradeHandler.SendTradeUpdate(m_chr.Client, true, OtherWindow.m_money,
          OtherWindow.m_items);
      }

      TradeHandler.SendTradeUpdate(OtherWindow.m_chr.Client, true, m_money,
        m_items);
    }

    /// <summary>
    /// Sets new status of trade and sends notification about the change to both parties
    /// </summary>
    /// <param name="status">new status</param>
    private void SendStatus(TradeStatus status, bool notifySelf = true)
    {
      if(notifySelf)
        TradeHandler.SendTradeStatus(m_chr.Client, status);
      TradeHandler.SendTradeStatus(OtherWindow.m_chr.Client, status);
    }
  }
}
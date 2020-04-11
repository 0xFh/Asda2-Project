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
            this.m_chr = owner;
            this.m_items = new Item[7];
        }

        public Character Owner
        {
            get { return this.m_chr; }
        }

        public bool Accepted
        {
            get { return this.m_accepted; }
        }

        /// <summary>Other party of the trading progress</summary>
        public TradeWindow OtherWindow
        {
            get { return this.m_otherWindow; }
        }

        /// <summary>Accepts the proposition of trade</summary>
        public void AcceptTradeProposal()
        {
            this.SendStatus(TradeStatus.Initiated, true);
        }

        /// <summary>Cancels the trade</summary>
        public void Cancel(TradeStatus status = TradeStatus.Cancelled)
        {
            this.StopTrade(status, true);
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
            if (tradeSlot >= (byte) 7)
                return;
            this.m_items[(int) tradeSlot] = (Item) null;
            this.SendTradeInfo(updateSelf);
        }

        /// <summary>Changes the amount of money to trade</summary>
        /// <param name="money">new amount of coins</param>
        public void SetMoney(uint money, bool updateSelf = true)
        {
            this.m_money = money;
            this.SendTradeInfo(updateSelf);
        }

        /// <summary>
        /// Accepts the trade
        /// If both parties have accepted, commits the trade
        /// </summary>
        public void AcceptTrade(bool updateSelf = true)
        {
            this.m_accepted = true;
            if (!this.CheckMoney() || !this.CheckItems())
                return;
            if (this.OtherWindow.m_accepted)
                this.CommitTrade();
            else
                this.SendStatus(TradeStatus.Accepted, updateSelf);
        }

        /// <summary>
        /// Unaccepts the trade (usually due to change of traded items or amount of money)
        /// </summary>
        public void UnacceptTrade(bool updateSelf = true)
        {
            this.m_accepted = false;
            this.SendStatus(TradeStatus.StateChanged, updateSelf);
        }

        /// <summary>
        /// Sends the notification about the change of trade status and stops the trade
        /// </summary>
        /// <param name="status">new status</param>
        /// <param name="notifySelf">whether to notify the caller himself</param>
        internal void StopTrade(TradeStatus status, bool notifySelf)
        {
            this.SendStatus(status, notifySelf);
            this.m_chr.TradeWindow = (TradeWindow) null;
            this.m_chr = (Character) null;
            this.OtherWindow.m_chr.TradeWindow = (TradeWindow) null;
            this.OtherWindow.m_chr = (Character) null;
        }

        /// <summary>
        /// Checks if the party has enough money for trade
        /// If not, unaccepts the trade
        /// </summary>
        /// <returns></returns>
        private bool CheckMoney()
        {
            if (this.m_money <= this.m_chr.Money)
                return true;
            this.m_chr.SendSystemMessage("Not enough gold");
            this.m_accepted = false;
            TradeHandler.SendTradeStatus((IPacketReceiver) this.m_chr.Client, TradeStatus.StateChanged);
            return false;
        }

        /// <summary>
        /// Checks if the party can trade selected items
        /// If not, unaccepts the trade
        /// </summary>
        /// <returns></returns>
        private bool CheckItems()
        {
            for (int index = 0; index < 7; ++index)
            {
                if (this.m_items[index] != null && !this.m_items[index].CanBeTraded)
                {
                    this.m_accepted = false;
                    TradeHandler.SendTradeStatus((IPacketReceiver) this.m_chr.Client, TradeStatus.StateChanged);
                    return false;
                }
            }

            return true;
        }

        /// <summary>Commits the trade between two parties</summary>
        private void CommitTrade()
        {
            TradeHandler.SendTradeStatus((IPacketReceiver) this.OtherWindow.m_chr.Client, TradeStatus.Accepted);
            IList<SimpleSlotId> slotsForTradedItems1 = this.GetSlotsForTradedItems();
            IList<SimpleSlotId> slotsForTradedItems2 = this.m_otherWindow.GetSlotsForTradedItems();
            if (!this.CheckFreeSlots(slotsForTradedItems1, this.OtherWindow.m_items) ||
                !this.m_otherWindow.CheckFreeSlots(slotsForTradedItems2, this.m_items))
            {
                this.m_accepted = false;
            }
            else
            {
                this.TransferItems(slotsForTradedItems1, this.m_otherWindow.m_items);
                this.m_otherWindow.TransferItems(slotsForTradedItems2, this.m_items);
                this.GiveMoney();
                this.m_otherWindow.GiveMoney();
                this.StopTrade(TradeStatus.Complete, true);
            }
        }

        private bool CheckFreeSlots(IList<SimpleSlotId> myFreeSlots, Item[] items)
        {
            bool flag = true;
            int index1 = 0;
            for (int index2 = 0; index2 < 6; ++index2)
            {
                if (items[index2] != null)
                {
                    if (myFreeSlots.Count <= index1)
                    {
                        flag = false;
                        break;
                    }

                    int slot = myFreeSlots[index1].Slot;
                    BaseInventory container = myFreeSlots[index1].Container;
                    IItemSlotHandler handler = container.GetHandler(slot);
                    InventoryError err = InventoryError.OK;
                    handler.CheckAdd(slot, items[index2].Amount, (IMountableItem) items[index2], ref err);
                    if (err != InventoryError.OK)
                    {
                        flag = false;
                        break;
                    }

                    int amount = items[index2].Amount;
                    container.CheckUniqueness((IMountableItem) items[index2], ref amount, ref err, true);
                    if (err != InventoryError.OK)
                    {
                        flag = false;
                        break;
                    }

                    ++index1;
                }
            }

            if (!flag)
            {
                this.m_chr.SendSystemMessage("You don't have enough free slots");
                this.m_otherWindow.m_chr.SendSystemMessage("Other party doesn't have enough free slots");
                this.SendStatus(TradeStatus.StateChanged, true);
            }

            return flag;
        }

        private IList<SimpleSlotId> GetSlotsForTradedItems()
        {
            return (IList<SimpleSlotId>) null;
        }

        private void GiveMoney()
        {
        }

        private void TransferItems(IList<SimpleSlotId> simpleSlots, Item[] items)
        {
            int index1 = 0;
            for (int index2 = 0; index2 < items.Length; ++index2)
            {
                Item obj = items[index2];
                if (obj != null)
                {
                    SimpleSlotId simpleSlot = simpleSlots[index1];
                    int amount = obj.Amount;
                    simpleSlot.Container.Distribute(obj.Template, ref amount);
                    if (amount < obj.Amount)
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
            if (updateSelf)
            {
                TradeHandler.SendTradeUpdate((IPacketReceiver) this.m_chr.Client, false, this.m_money, this.m_items);
                TradeHandler.SendTradeUpdate((IPacketReceiver) this.m_chr.Client, true, this.OtherWindow.m_money,
                    this.OtherWindow.m_items);
            }

            TradeHandler.SendTradeUpdate((IPacketReceiver) this.OtherWindow.m_chr.Client, true, this.m_money,
                this.m_items);
        }

        /// <summary>
        /// Sets new status of trade and sends notification about the change to both parties
        /// </summary>
        /// <param name="status">new status</param>
        private void SendStatus(TradeStatus status, bool notifySelf = true)
        {
            if (notifySelf)
                TradeHandler.SendTradeStatus((IPacketReceiver) this.m_chr.Client, status);
            TradeHandler.SendTradeStatus((IPacketReceiver) this.OtherWindow.m_chr.Client, status);
        }
    }
}
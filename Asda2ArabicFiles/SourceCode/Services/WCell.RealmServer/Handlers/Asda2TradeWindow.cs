using System;
using System.Collections.Generic;
using System.Linq;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Logs;
using WCell.Util.NLog;

namespace WCell.RealmServer.Handlers
{
    public class Asda2TradeWindow
    {
        public Asda2TradeType TradeType { get; set; }
        public bool Accepted
        {
            get { return _accepted; }
            set
            {
                _accepted = value;
                if (value)
                {
                    FisrtChar.Stunned++;
                    SecondChar.Stunned++;
                    Asda2TradeHandler.SendTradeStartedResponse(FisrtChar.Client, Asda2TradeStartedStatus.Started, SecondChar, TradeType == Asda2TradeType.RedularTrade);
                    Asda2TradeHandler.SendTradeStartedResponse(SecondChar.Client, Asda2TradeStartedStatus.Started, FisrtChar, TradeType == Asda2TradeType.RedularTrade);
                }
            }
        }

        public Asda2TradeWindow()
        {
            Created = DateTime.Now;
        }
        public DateTime Created { get; private set; }
        private Asda2ItemTradeRef[] _firstCharacterItems;
        private Asda2ItemTradeRef[] _secondCharacterItems;
        private bool _accepted;
        private bool _cleanuped;
        public Character FisrtChar { get; set; }
        public Character SecondChar { get; set; }
        public Asda2ItemTradeRef[] FirstCharacterItems
        {
            get { return _firstCharacterItems ?? (_firstCharacterItems = new Asda2ItemTradeRef[5]); }
        }
        public Asda2ItemTradeRef[] SecondCharacterItems
        {
            get { return _secondCharacterItems ?? (_secondCharacterItems = new Asda2ItemTradeRef[5]); }
        }

        public bool Expired
        {
            get { return !Accepted && Created - DateTime.Now > new TimeSpan(0, 0, 30); }
        }

        public void CancelTrade()
        {
            if (FisrtChar != null)
            {
                Asda2TradeHandler.SendTradeRejectedResponse(FisrtChar.Client);
                FisrtChar.Asda2TradeWindow = null;
            }
            if (SecondChar != null)
            {
                Asda2TradeHandler.SendTradeRejectedResponse(SecondChar.Client);
                SecondChar.Asda2TradeWindow = null;
            }
            CleanUp();
        }

        private void CleanUp()
        {
            if (_cleanuped)
                return;
            _cleanuped = true;
            FisrtChar.Stunned--;
            SecondChar.Stunned--;
            FisrtChar.Asda2TradeWindow = null;
            SecondChar.Asda2TradeWindow = null;
            FisrtChar = null;
            SecondChar = null;
            _firstCharacterItems = null;
            _secondCharacterItems = null;
        }

        Asda2PushItemToTradeStatus SetCharacterItemRefs(Character character, Asda2ItemTradeRef itemRef)
        {
            if (itemRef.Item.IsDeleted)
            {
                character.YouAreFuckingCheater("Trying to add to trade deleted item.");
                CancelTrade();
                return Asda2PushItemToTradeStatus.AnErrorWasFoundWithTransferedItem;
            }
            if (itemRef.Item.Record == null)
            {
                character.YouAreFuckingCheater("Trying to add to trade item record null.");
                CancelTrade();
                return Asda2PushItemToTradeStatus.AnErrorWasFoundWithTransferedItem;
            }
            Asda2ItemTradeRef[] chrItemsRefs;
            if (FisrtChar == character)
                chrItemsRefs = FirstCharacterItems;
            else
                chrItemsRefs = SecondChar == character ? SecondCharacterItems : null;
            if (chrItemsRefs == null || itemRef.Item == null ||
                (itemRef.Item.Amount < itemRef.Amount && itemRef.Item.ItemId != 20551) ||
                (itemRef.Item.ItemId == 20551 && itemRef.Amount >= character.Money))
            {
                character.YouAreFuckingCheater("Trying to cheat while trading (1).", 50);
                CancelTrade();
                return Asda2PushItemToTradeStatus.AnErrorWasFoundWithTransferedItem;
            }
            var itemExist = chrItemsRefs.FirstOrDefault(i => i != null && i.Item == itemRef.Item);
            if (itemExist != null)
            {
                if ((itemExist.Amount + itemRef.Amount > itemRef.Item.Amount && itemRef.Item.ItemId != 20551) || (itemExist.Amount + itemRef.Amount >= character.Money && itemRef.Item.ItemId == 20551))
                {
                    character.YouAreFuckingCheater("Trying to cheat while trading (2).", 50);
                    CancelTrade();
                    return Asda2PushItemToTradeStatus.AnErrorWasFoundWithTransferedItem;
                }
                itemExist.Amount += itemRef.Amount;
                return Asda2PushItemToTradeStatus.Ok;
            }
            //item not exist
            var freeSlot = -1;
            for (int i = 0; i < 5; i++)
            {
                if (chrItemsRefs[i] == null)
                {
                    freeSlot = i;
                    break;
                }
            }
            if (freeSlot == -1)
            {
                character.YouAreFuckingCheater("Trying to cheat while trading (3).", 50);
                CancelTrade();
                return Asda2PushItemToTradeStatus.AnErrorWasFoundWithTransferedItem;
            }
            if (itemRef.Item.IsSoulbound)
                return Asda2PushItemToTradeStatus.ItemCantBeTraded;
            chrItemsRefs[freeSlot] = itemRef;
            return Asda2PushItemToTradeStatus.Ok;
        }
        public Asda2PushItemToTradeStatus PushItemToTrade(Character activeCharacter, short cellNum, int quantity, byte invNum, ref Asda2ItemTradeRef item)
        {
            if (cellNum < 0 || cellNum >= activeCharacter.Asda2Inventory.ShopItems.Length || quantity < 0 || (activeCharacter == FisrtChar && FirstCharShowItems) || (activeCharacter == SecondChar && SecondCharShowItems))
            {
                activeCharacter.YouAreFuckingCheater("Trying to cheat while pushing item to trade.", 50);
                CancelTrade();
                return Asda2PushItemToTradeStatus.AnErrorWasFoundWithTransferedItem;
            }
            if (invNum == 2 && cellNum == 0)
            {
                //gold item
                return SetCharacterItemRefs(activeCharacter, item = new Asda2ItemTradeRef() { Amount = quantity, Item = activeCharacter.Asda2Inventory.RegularItems[cellNum] });
            }
            if (TradeType == Asda2TradeType.RedularTrade)
                return SetCharacterItemRefs(activeCharacter, item = new Asda2ItemTradeRef() { Amount = quantity, Item = activeCharacter.Asda2Inventory.RegularItems[cellNum] });
            else
                return SetCharacterItemRefs(activeCharacter, item = new Asda2ItemTradeRef() { Amount = quantity, Item = activeCharacter.Asda2Inventory.ShopItems[cellNum] });
        }

        public void PopItem(Character character, byte inv, short cell)
        {
            if ((character == FisrtChar && FirstCharShowItems) || (character == SecondChar && SecondCharShowItems))
            {
                character.YouAreFuckingCheater("Poping item prom trade with prong params", 20);
                CancelTrade();
            }
            Asda2ItemTradeRef[] chrItemsRefs;
            if (FisrtChar == character)
                chrItemsRefs = FirstCharacterItems;
            chrItemsRefs = SecondChar == character ? SecondCharacterItems : null;
            if (chrItemsRefs == null)
            {
                character.YouAreFuckingCheater("Trying to pop items from trade while items not initialized", 20);
                CancelTrade();
                return;
            }
            var slot = -1;
            for (int i = 0; i < 5; i++)
            {
                if (chrItemsRefs[i] == null)
                    continue;
                if (chrItemsRefs[i].Item.InventoryType == (Asda2InventoryType)inv && chrItemsRefs[i].Item.Slot == cell)
                {
                    slot = i;
                    break;
                }
            }
            if (slot == -1)
            {
                character.YouAreFuckingCheater("Trying to pop items from trade from frong slot", 20);
                CancelTrade();
                return;
            }
            chrItemsRefs[slot].Item = null;
            chrItemsRefs[slot] = null;
        }

        public void ShowItemToOtherPlayer(Character activeCharacter)
        {
            if (activeCharacter == FisrtChar && !FirstCharShowItems)
            {
                FirstCharShowItems = true;
                Asda2TradeHandler.SendConfimTradeFromOponentResponse(SecondChar.Client, FirstCharacterItems);
            }
            else if (activeCharacter == SecondChar && !SecondCharShowItems)
            {
                SecondCharShowItems = true;
                Asda2TradeHandler.SendConfimTradeFromOponentResponse(FisrtChar.Client, SecondCharacterItems);
            }
        }

        protected bool FirstCharShowItems { get; set; }
        protected bool SecondCharShowItems { get; set; }

        public void Init()
        {

        }

        public void ConfirmTrade(Character activeCharacter)
        {
            if (_cleanuped)
                return;
            if (!FirstCharShowItems || !SecondCharShowItems)
            {
                activeCharacter.YouAreFuckingCheater("Confirm trade while items not shown", 40);
                CancelTrade();
                return;
            }
            if (activeCharacter == FisrtChar)
            {
                if (FirstCharConfirmedTrade)
                    return;
                FirstCharConfirmedTrade = true;
            }
            else if (activeCharacter == SecondChar)
            {
                if (SecondCharConfirmedTrade)
                    return;
                SecondCharConfirmedTrade = true;
            }
            if (FirstCharConfirmedTrade && SecondCharConfirmedTrade)
            {
                TransferItems();
                CleanUp();
            }
        }

        private void TransferItems()
        {
            if (_cleanuped)
                return;
            if (ItemsTransfered) return;
            ItemsTransfered = true;
            var firstCharItemsCount =
                FirstCharacterItems.Count(i => i != null && i.Item != null && i.Item.ItemId != 20551);
            var secondCharItemsCount =
                SecondCharacterItems.Count(i => i != null && i.Item != null && i.Item.ItemId != 20551);
            if (TradeType == Asda2TradeType.RedularTrade)
            {
                if (SecondChar.Asda2Inventory.FreeRegularSlotsCount < firstCharItemsCount || FisrtChar.Asda2Inventory.FreeRegularSlotsCount < secondCharItemsCount)
                {
                    FisrtChar.SendSystemMessage("Check free space in inventory!.");
                    SecondChar.SendSystemMessage("Check free space in inventory!.");
                    CancelTrade();
                    return;
                }
            }
            else
            {
                if (SecondChar.Asda2Inventory.FreeShopSlotsCount < firstCharItemsCount || FisrtChar.Asda2Inventory.FreeShopSlotsCount < secondCharItemsCount)
                {
                    FisrtChar.SendSystemMessage("Check free space in inventory!.");
                    SecondChar.SendSystemMessage("Check free space in inventory!.");
                    CancelTrade();
                    return;
                }
            }
            var newItemsOfSecond = Transfer(FirstCharacterItems, SecondChar);
            var newItemsOfFirst = Transfer(SecondCharacterItems, FisrtChar);
            if (TradeType == Asda2TradeType.RedularTrade)
            {
                Asda2TradeHandler.SendRegularTradeCompleteResponse(FisrtChar.Client, newItemsOfFirst);
                Asda2TradeHandler.SendRegularTradeCompleteResponse(SecondChar.Client, newItemsOfSecond);
            }
            else
            {
                Asda2TradeHandler.SendShopTradeCompleteResponse(FisrtChar.Client, newItemsOfFirst);
                Asda2TradeHandler.SendShopTradeCompleteResponse(SecondChar.Client, newItemsOfSecond);
            }
            FisrtChar.SendMoneyUpdate();
            SecondChar.SendMoneyUpdate();
        }
        Asda2Item[] Transfer(IEnumerable<Asda2ItemTradeRef> items, Character rcvr)
        {
            var transferedItems = new Asda2Item[5];
            var i = 0;
            foreach (var itemRef in items)
            {
                if (itemRef == null || itemRef.Item == null)
                    continue;
                var transferingItem = itemRef.Item;
                if (transferingItem.IsDeleted)
                {
                    LogUtil.WarnException("Trying to add to {0} item {1} which is deleted", rcvr.Name, transferingItem);
                    continue;
                }
                if (transferingItem.Record == null)
                {
                    LogUtil.WarnException("Trying to add to {0} item {1} which record is null", rcvr.Name, transferingItem);
                    continue;
                }
                if (transferingItem.ItemId == 20551)
                {
                    if (!itemRef.Item.OwningCharacter.SubtractMoney((uint)itemRef.Amount))
                    {
                        itemRef.Item.OwningCharacter.YouAreFuckingCheater("transfering items while not enoght money", 40);
                        itemRef.Item.OwningCharacter.Money = 1;
                        continue;
                    }
                    rcvr.AddMoney((uint)itemRef.Amount);
                    continue;
                }
                if (transferingItem.Amount < itemRef.Amount || itemRef.Amount <= 0)
                    itemRef.Amount = transferingItem.Amount;
                Asda2Item addedItem = null;
                rcvr.Asda2Inventory.TryAdd((int)itemRef.Item.Template.ItemId, itemRef.Amount, false, ref addedItem, null, transferingItem);

                transferingItem.Amount -= itemRef.Amount;
                transferedItems[i] = addedItem;
                i++;
            }
            return transferedItems;
        }
        private bool ItemsTransfered { get; set; }
        protected bool FirstCharConfirmedTrade { get; set; }
        protected bool SecondCharConfirmedTrade { get; set; }
    }
}
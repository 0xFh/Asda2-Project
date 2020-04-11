using System;
using System.Collections.Generic;
using System.Linq;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Entities;
using WCell.Util.NLog;

namespace WCell.RealmServer.Handlers
{
    public class Asda2TradeWindow
    {
        private Asda2ItemTradeRef[] _firstCharacterItems;
        private Asda2ItemTradeRef[] _secondCharacterItems;
        private bool _accepted;
        private bool _cleanuped;

        public Asda2TradeType TradeType { get; set; }

        public bool Accepted
        {
            get { return this._accepted; }
            set
            {
                this._accepted = value;
                if (!value)
                    return;
                ++this.FisrtChar.Stunned;
                ++this.SecondChar.Stunned;
                Asda2TradeHandler.SendTradeStartedResponse(this.FisrtChar.Client, Asda2TradeStartedStatus.Started,
                    this.SecondChar, this.TradeType == Asda2TradeType.RedularTrade);
                Asda2TradeHandler.SendTradeStartedResponse(this.SecondChar.Client, Asda2TradeStartedStatus.Started,
                    this.FisrtChar, this.TradeType == Asda2TradeType.RedularTrade);
            }
        }

        public Asda2TradeWindow()
        {
            this.Created = DateTime.Now;
        }

        public DateTime Created { get; private set; }

        public Character FisrtChar { get; set; }

        public Character SecondChar { get; set; }

        public Asda2ItemTradeRef[] FirstCharacterItems
        {
            get { return this._firstCharacterItems ?? (this._firstCharacterItems = new Asda2ItemTradeRef[5]); }
        }

        public Asda2ItemTradeRef[] SecondCharacterItems
        {
            get { return this._secondCharacterItems ?? (this._secondCharacterItems = new Asda2ItemTradeRef[5]); }
        }

        public bool Expired
        {
            get
            {
                if (!this.Accepted)
                    return this.Created - DateTime.Now > new TimeSpan(0, 0, 30);
                return false;
            }
        }

        public void CancelTrade()
        {
            if (this.FisrtChar != null)
            {
                Asda2TradeHandler.SendTradeRejectedResponse(this.FisrtChar.Client);
                this.FisrtChar.Asda2TradeWindow = (Asda2TradeWindow) null;
            }

            if (this.SecondChar != null)
            {
                Asda2TradeHandler.SendTradeRejectedResponse(this.SecondChar.Client);
                this.SecondChar.Asda2TradeWindow = (Asda2TradeWindow) null;
            }

            this.CleanUp();
        }

        private void CleanUp()
        {
            if (this._cleanuped)
                return;
            this._cleanuped = true;
            --this.FisrtChar.Stunned;
            --this.SecondChar.Stunned;
            this.FisrtChar.Asda2TradeWindow = (Asda2TradeWindow) null;
            this.SecondChar.Asda2TradeWindow = (Asda2TradeWindow) null;
            this.FisrtChar = (Character) null;
            this.SecondChar = (Character) null;
            this._firstCharacterItems = (Asda2ItemTradeRef[]) null;
            this._secondCharacterItems = (Asda2ItemTradeRef[]) null;
        }

        private Asda2PushItemToTradeStatus SetCharacterItemRefs(Character character, Asda2ItemTradeRef itemRef)
        {
            if (itemRef.Item.IsDeleted)
            {
                character.YouAreFuckingCheater("Trying to add to trade deleted item.", 1);
                this.CancelTrade();
                return Asda2PushItemToTradeStatus.AnErrorWasFoundWithTransferedItem;
            }

            if (itemRef.Item.Record == null)
            {
                character.YouAreFuckingCheater("Trying to add to trade item record null.", 1);
                this.CancelTrade();
                return Asda2PushItemToTradeStatus.AnErrorWasFoundWithTransferedItem;
            }

            Asda2ItemTradeRef[] asda2ItemTradeRefArray = this.FisrtChar != character
                ? (this.SecondChar == character ? this.SecondCharacterItems : (Asda2ItemTradeRef[]) null)
                : this.FirstCharacterItems;
            if (asda2ItemTradeRefArray == null || itemRef.Item == null ||
                itemRef.Item.Amount < itemRef.Amount && itemRef.Item.ItemId != 20551 || itemRef.Item.ItemId == 20551 &&
                (long) itemRef.Amount >= (long) character.Money)
            {
                character.YouAreFuckingCheater("Trying to cheat while trading (1).", 50);
                this.CancelTrade();
                return Asda2PushItemToTradeStatus.AnErrorWasFoundWithTransferedItem;
            }

            Asda2ItemTradeRef asda2ItemTradeRef =
                ((IEnumerable<Asda2ItemTradeRef>) asda2ItemTradeRefArray).FirstOrDefault<Asda2ItemTradeRef>(
                    (Func<Asda2ItemTradeRef, bool>) (i =>
                    {
                        if (i != null)
                            return i.Item == itemRef.Item;
                        return false;
                    }));
            if (asda2ItemTradeRef != null)
            {
                if (asda2ItemTradeRef.Amount + itemRef.Amount > itemRef.Item.Amount && itemRef.Item.ItemId != 20551 ||
                    (long) (asda2ItemTradeRef.Amount + itemRef.Amount) >= (long) character.Money &&
                    itemRef.Item.ItemId == 20551)
                {
                    character.YouAreFuckingCheater("Trying to cheat while trading (2).", 50);
                    this.CancelTrade();
                    return Asda2PushItemToTradeStatus.AnErrorWasFoundWithTransferedItem;
                }

                asda2ItemTradeRef.Amount += itemRef.Amount;
                return Asda2PushItemToTradeStatus.Ok;
            }

            int index1 = -1;
            for (int index2 = 0; index2 < 5; ++index2)
            {
                if (asda2ItemTradeRefArray[index2] == null)
                {
                    index1 = index2;
                    break;
                }
            }

            if (index1 == -1)
            {
                character.YouAreFuckingCheater("Trying to cheat while trading (3).", 50);
                this.CancelTrade();
                return Asda2PushItemToTradeStatus.AnErrorWasFoundWithTransferedItem;
            }

            if (itemRef.Item.IsSoulbound)
                return Asda2PushItemToTradeStatus.ItemCantBeTraded;
            asda2ItemTradeRefArray[index1] = itemRef;
            return Asda2PushItemToTradeStatus.Ok;
        }

        public Asda2PushItemToTradeStatus PushItemToTrade(Character activeCharacter, short cellNum, int quantity,
            byte invNum, ref Asda2ItemTradeRef item)
        {
            if (cellNum < (short) 0 || (int) cellNum >= activeCharacter.Asda2Inventory.ShopItems.Length ||
                quantity < 0 || (activeCharacter == this.FisrtChar && this.FirstCharShowItems ||
                                 activeCharacter == this.SecondChar && this.SecondCharShowItems))
            {
                activeCharacter.YouAreFuckingCheater("Trying to cheat while pushing item to trade.", 50);
                this.CancelTrade();
                return Asda2PushItemToTradeStatus.AnErrorWasFoundWithTransferedItem;
            }

            if (invNum == (byte) 2 && cellNum == (short) 0)
                return this.SetCharacterItemRefs(activeCharacter, item = new Asda2ItemTradeRef()
                {
                    Amount = quantity,
                    Item = activeCharacter.Asda2Inventory.RegularItems[(int) cellNum]
                });
            if (this.TradeType == Asda2TradeType.RedularTrade)
                return this.SetCharacterItemRefs(activeCharacter, item = new Asda2ItemTradeRef()
                {
                    Amount = quantity,
                    Item = activeCharacter.Asda2Inventory.RegularItems[(int) cellNum]
                });
            return this.SetCharacterItemRefs(activeCharacter, item = new Asda2ItemTradeRef()
            {
                Amount = quantity,
                Item = activeCharacter.Asda2Inventory.ShopItems[(int) cellNum]
            });
        }

        public void PopItem(Character character, byte inv, short cell)
        {
            if (character == this.FisrtChar && this.FirstCharShowItems ||
                character == this.SecondChar && this.SecondCharShowItems)
            {
                character.YouAreFuckingCheater("Poping item prom trade with prong params", 20);
                this.CancelTrade();
            }

            if (this.FisrtChar == character)
            {
                Asda2ItemTradeRef[] firstCharacterItems = this.FirstCharacterItems;
            }

            Asda2ItemTradeRef[] asda2ItemTradeRefArray =
                this.SecondChar == character ? this.SecondCharacterItems : (Asda2ItemTradeRef[]) null;
            if (asda2ItemTradeRefArray == null)
            {
                character.YouAreFuckingCheater("Trying to pop items from trade while items not initialized", 20);
                this.CancelTrade();
            }
            else
            {
                int index1 = -1;
                for (int index2 = 0; index2 < 5; ++index2)
                {
                    if (asda2ItemTradeRefArray[index2] != null &&
                        asda2ItemTradeRefArray[index2].Item.InventoryType == (Asda2InventoryType) inv &&
                        (int) asda2ItemTradeRefArray[index2].Item.Slot == (int) cell)
                    {
                        index1 = index2;
                        break;
                    }
                }

                if (index1 == -1)
                {
                    character.YouAreFuckingCheater("Trying to pop items from trade from frong slot", 20);
                    this.CancelTrade();
                }
                else
                {
                    asda2ItemTradeRefArray[index1].Item = (Asda2Item) null;
                    asda2ItemTradeRefArray[index1] = (Asda2ItemTradeRef) null;
                }
            }
        }

        public void ShowItemToOtherPlayer(Character activeCharacter)
        {
            if (activeCharacter == this.FisrtChar && !this.FirstCharShowItems)
            {
                this.FirstCharShowItems = true;
                Asda2TradeHandler.SendConfimTradeFromOponentResponse(this.SecondChar.Client, this.FirstCharacterItems);
            }
            else
            {
                if (activeCharacter != this.SecondChar || this.SecondCharShowItems)
                    return;
                this.SecondCharShowItems = true;
                Asda2TradeHandler.SendConfimTradeFromOponentResponse(this.FisrtChar.Client, this.SecondCharacterItems);
            }
        }

        protected bool FirstCharShowItems { get; set; }

        protected bool SecondCharShowItems { get; set; }

        public void Init()
        {
        }

        public void ConfirmTrade(Character activeCharacter)
        {
            if (this._cleanuped)
                return;
            if (!this.FirstCharShowItems || !this.SecondCharShowItems)
            {
                activeCharacter.YouAreFuckingCheater("Confirm trade while items not shown", 40);
                this.CancelTrade();
            }
            else
            {
                if (activeCharacter == this.FisrtChar)
                {
                    if (this.FirstCharConfirmedTrade)
                        return;
                    this.FirstCharConfirmedTrade = true;
                }
                else if (activeCharacter == this.SecondChar)
                {
                    if (this.SecondCharConfirmedTrade)
                        return;
                    this.SecondCharConfirmedTrade = true;
                }

                if (!this.FirstCharConfirmedTrade || !this.SecondCharConfirmedTrade)
                    return;
                this.TransferItems();
                this.CleanUp();
            }
        }

        private void TransferItems()
        {
            if (this._cleanuped || this.ItemsTransfered)
                return;
            this.ItemsTransfered = true;
            int num1 = ((IEnumerable<Asda2ItemTradeRef>) this.FirstCharacterItems).Count<Asda2ItemTradeRef>(
                (Func<Asda2ItemTradeRef, bool>) (i =>
                {
                    if (i != null && i.Item != null)
                        return i.Item.ItemId != 20551;
                    return false;
                }));
            int num2 = ((IEnumerable<Asda2ItemTradeRef>) this.SecondCharacterItems).Count<Asda2ItemTradeRef>(
                (Func<Asda2ItemTradeRef, bool>) (i =>
                {
                    if (i != null && i.Item != null)
                        return i.Item.ItemId != 20551;
                    return false;
                }));
            if (this.TradeType == Asda2TradeType.RedularTrade)
            {
                if (this.SecondChar.Asda2Inventory.FreeRegularSlotsCount < num1 ||
                    ((IEnumerable<Asda2Item>) this.FisrtChar.Asda2Inventory.RegularItems).Count<Asda2Item>(
                        (Func<Asda2Item, bool>) (i => i == null)) < num2)
                {
                    this.FisrtChar.SendSystemMessage("Check free space in inventory!.");
                    this.SecondChar.SendSystemMessage("Check free space in inventory!.");
                    this.CancelTrade();
                    return;
                }
            }
            else if (((IEnumerable<Asda2Item>) this.SecondChar.Asda2Inventory.ShopItems).Count<Asda2Item>(
                         (Func<Asda2Item, bool>) (i => i == null)) < num1 ||
                     ((IEnumerable<Asda2Item>) this.FisrtChar.Asda2Inventory.ShopItems).Count<Asda2Item>(
                         (Func<Asda2Item, bool>) (i => i == null)) < num2)
            {
                this.FisrtChar.SendSystemMessage("Check free space in inventory!.");
                this.SecondChar.SendSystemMessage("Check free space in inventory!.");
                this.CancelTrade();
                return;
            }

            Asda2Item[] items1 =
                this.Transfer((IEnumerable<Asda2ItemTradeRef>) this.FirstCharacterItems, this.SecondChar);
            Asda2Item[] items2 =
                this.Transfer((IEnumerable<Asda2ItemTradeRef>) this.SecondCharacterItems, this.FisrtChar);
            if (this.TradeType == Asda2TradeType.RedularTrade)
            {
                Asda2TradeHandler.SendRegularTradeCompleteResponse(this.FisrtChar.Client, items2);
                Asda2TradeHandler.SendRegularTradeCompleteResponse(this.SecondChar.Client, items1);
            }
            else
            {
                Asda2TradeHandler.SendShopTradeCompleteResponse(this.FisrtChar.Client, items2);
                Asda2TradeHandler.SendShopTradeCompleteResponse(this.SecondChar.Client, items1);
            }

            this.FisrtChar.SendMoneyUpdate();
            this.SecondChar.SendMoneyUpdate();
        }

        private Asda2Item[] Transfer(IEnumerable<Asda2ItemTradeRef> items, Character rcvr)
        {
            Asda2Item[] asda2ItemArray = new Asda2Item[5];
            int index = 0;
            foreach (Asda2ItemTradeRef asda2ItemTradeRef in items)
            {
                if (asda2ItemTradeRef != null && asda2ItemTradeRef.Item != null)
                {
                    Asda2Item itemToCopyStats = asda2ItemTradeRef.Item;
                    if (itemToCopyStats.IsDeleted)
                        LogUtil.WarnException("Trying to add to {0} item {1} which is deleted", new object[2]
                        {
                            (object) rcvr.Name,
                            (object) itemToCopyStats
                        });
                    else if (itemToCopyStats.Record == null)
                        LogUtil.WarnException("Trying to add to {0} item {1} which record is null", new object[2]
                        {
                            (object) rcvr.Name,
                            (object) itemToCopyStats
                        });
                    else if (itemToCopyStats.ItemId == 20551)
                    {
                        if (!asda2ItemTradeRef.Item.OwningCharacter.SubtractMoney((uint) asda2ItemTradeRef.Amount))
                        {
                            asda2ItemTradeRef.Item.OwningCharacter.YouAreFuckingCheater(
                                "transfering items while not enoght money", 40);
                            asda2ItemTradeRef.Item.OwningCharacter.Money = 1U;
                        }
                        else
                            rcvr.AddMoney((uint) asda2ItemTradeRef.Amount);
                    }
                    else
                    {
                        if (itemToCopyStats.Amount < asda2ItemTradeRef.Amount || asda2ItemTradeRef.Amount <= 0)
                            asda2ItemTradeRef.Amount = itemToCopyStats.Amount;
                        Asda2Item asda2Item = (Asda2Item) null;
                        int num = (int) rcvr.Asda2Inventory.TryAdd((int) asda2ItemTradeRef.Item.Template.ItemId,
                            asda2ItemTradeRef.Amount, false, ref asda2Item, new Asda2InventoryType?(), itemToCopyStats);
                        itemToCopyStats.Amount -= asda2ItemTradeRef.Amount;
                        asda2ItemArray[index] = asda2Item;
                        ++index;
                    }
                }
            }

            return asda2ItemArray;
        }

        private bool ItemsTransfered { get; set; }

        protected bool FirstCharConfirmedTrade { get; set; }

        protected bool SecondCharConfirmedTrade { get; set; }
    }
}
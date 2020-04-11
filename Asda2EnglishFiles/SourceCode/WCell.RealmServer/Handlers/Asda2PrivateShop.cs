using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Core.Network;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Logs;

namespace WCell.RealmServer.Handlers
{
    public class Asda2PrivateShop
    {
        public static Asda2Item SelledItem = new Asda2Item()
        {
            ItemId = 36830,
            IsDeleted = true
        };

        private Asda2ItemTradeRef[] _itemsOnTrade;
        private List<Character> _joinedCharacters;

        public List<Character> JoinedCharacters
        {
            get { return this._joinedCharacters ?? (this._joinedCharacters = new List<Character>()); }
        }

        public Asda2PrivateShop(Character owner)
        {
            this.Owner = owner;
        }

        public Character Owner { get; set; }

        public Asda2ItemTradeRef[] ItemsOnTrade
        {
            get { return this._itemsOnTrade ?? (this._itemsOnTrade = new Asda2ItemTradeRef[10]); }
        }

        public byte ItemsCount
        {
            get
            {
                return (byte) ((IEnumerable<Asda2ItemTradeRef>) this.ItemsOnTrade).Count<Asda2ItemTradeRef>(
                    (Func<Asda2ItemTradeRef, bool>) (i => i != null));
            }
        }

        public void Send(RealmPacketOut packet)
        {
            foreach (Character joinedCharacter in this.JoinedCharacters)
                joinedCharacter.Send(packet, false);
        }

        public PrivateShopOpenedResult StartTrade(List<Asda2ItemTradeRef> itemsToTrade, string title)
        {
            this.Owner.Asda2TradeDescription = title;
            for (int index = 0; index < this.ItemsOnTrade.Length; ++index)
                this.ItemsOnTrade[index] = itemsToTrade.Count <= index ? (Asda2ItemTradeRef) null : itemsToTrade[index];
            Asda2PrivateShopHandler.SendPrivateShopOpenedResponse(this.Owner.Client, PrivateShopOpenedResult.Ok,
                this.ItemsOnTrade);
            this.Owner.IsSitting = true;
            this.Owner.IsAsda2TradeDescriptionEnabled = true;
            this.Trading = true;
            return PrivateShopOpenedResult.Ok;
        }

        public bool Trading { get; set; }

        public void Join(Character activeCharacter)
        {
            List<Character> characterList = new List<Character>()
            {
                this.Owner
            };
            characterList.AddRange((IEnumerable<Character>) this.JoinedCharacters);
            using (RealmPacketOut notificationResponse =
                Asda2PrivateShopHandler.CreatePrivateShopChatNotificationResponse(activeCharacter.AccId,
                    Asda2PrivateShopNotificationType.Joined))
            {
                foreach (Character character in characterList)
                    character.Send(notificationResponse, false);
            }

            ++activeCharacter.Stunned;
            this.JoinedCharacters.Add(activeCharacter);
            Asda2PrivateShopHandler.SendCharacterPrivateShopInfoResponse(activeCharacter.Client,
                Asda2ViewTradeShopInfoStatus.Ok, this);
            activeCharacter.PrivateShop = this;
        }

        public void SendMessage(string msg, Character activeCharacter, Locale locale)
        {
            List<Character> characterList = new List<Character>()
            {
                this.Owner
            };
            characterList.AddRange((IEnumerable<Character>) this.JoinedCharacters);
            using (RealmPacketOut shopChatResResponse =
                Asda2PrivateShopHandler.CreatePrivateShopChatResResponse(activeCharacter, msg, locale))
            {
                foreach (Character character in characterList)
                {
                    if (locale == Locale.Any || character.Client.Locale == locale)
                        character.Send(shopChatResResponse, false);
                }
            }
        }

        public void Exit(Character activeCharacter)
        {
            if (activeCharacter == this.Owner)
            {
                using (RealmPacketOut shopToOwnerResponse =
                    Asda2PrivateShopHandler.CreateCloseCharacterTradeShopToOwnerResponse(
                        Asda2PrivateShopClosedToOwnerResult.HostClosedShop))
                {
                    foreach (Character joinedCharacter in this.JoinedCharacters)
                    {
                        joinedCharacter.Send(shopToOwnerResponse, false);
                        joinedCharacter.PrivateShop = (Asda2PrivateShop) null;
                        --joinedCharacter.Stunned;
                    }
                }

                this.JoinedCharacters.Clear();
                this._joinedCharacters = (List<Character>) null;
                this._itemsOnTrade = (Asda2ItemTradeRef[]) null;
                activeCharacter.PrivateShop = (Asda2PrivateShop) null;
                activeCharacter.IsAsda2TradeDescriptionEnabled = false;
                activeCharacter.Asda2TradeDescription = "";
                Asda2PrivateShopHandler.SendCloseCharacterTradeShopToOwnerResponse(activeCharacter.Client,
                    Asda2PrivateShopClosedToOwnerResult.Ok);
            }
            else
            {
                this.JoinedCharacters.Remove(activeCharacter);
                List<Character> characterList = new List<Character>()
                {
                    this.Owner
                };
                characterList.AddRange((IEnumerable<Character>) this.JoinedCharacters);
                using (RealmPacketOut notificationResponse =
                    Asda2PrivateShopHandler.CreatePrivateShopChatNotificationResponse(activeCharacter.AccId,
                        Asda2PrivateShopNotificationType.Left))
                {
                    foreach (Character character in characterList)
                        character.Send(notificationResponse, false);
                }

                --activeCharacter.Stunned;
                activeCharacter.PrivateShop = (Asda2PrivateShop) null;
                Asda2PrivateShopHandler.SendCloseCharacterTradeShopToOwnerResponse(activeCharacter.Client,
                    Asda2PrivateShopClosedToOwnerResult.Ok);
            }
        }

        public void BuyItems(Character activeCharacter, List<Asda2ItemTradeRef> itemsToBuyRefs)
        {
            this.Owner.Map.AddMessage((Action) (() =>
            {
                List<Asda2ItemTradeRef> source = new List<Asda2ItemTradeRef>();
                foreach (Asda2ItemTradeRef itemsToBuyRef in itemsToBuyRefs)
                {
                    Asda2ItemTradeRef asda2ItemTradeRef = this.ItemsOnTrade[(int) itemsToBuyRef.TradeSlot];
                    if (asda2ItemTradeRef == null || asda2ItemTradeRef.Amount == -1 || asda2ItemTradeRef.Amount != 0 &&
                        asda2ItemTradeRef.Amount < itemsToBuyRef.Amount)
                    {
                        Asda2PrivateShopHandler.SendItemBuyedFromPrivateShopResponse(activeCharacter,
                            PrivateShopBuyResult.RequestedNumberOfItemsIsNoLongerAvaliable, (List<Asda2Item>) null);
                        return;
                    }

                    source.Add(new Asda2ItemTradeRef()
                    {
                        Amount = itemsToBuyRef.Amount,
                        Item = asda2ItemTradeRef.Item,
                        Price = asda2ItemTradeRef.Price,
                        TradeSlot = asda2ItemTradeRef.TradeSlot
                    });
                }

                ulong num1 = source.Aggregate<Asda2ItemTradeRef, ulong>(0UL,
                    (Func<ulong, Asda2ItemTradeRef, ulong>) ((current, asda2ItemTradeRef) =>
                        current + (ulong) (asda2ItemTradeRef.Price * asda2ItemTradeRef.Amount)));
                if (num1 > (ulong) int.MaxValue)
                {
                    activeCharacter.YouAreFuckingCheater("Trying to buy items with wrong money amount.", 50);
                    Asda2PrivateShopHandler.SendItemBuyedFromPrivateShopResponse(activeCharacter,
                        PrivateShopBuyResult.NotEnoghtGold, (List<Asda2Item>) null);
                }
                else if ((ulong) activeCharacter.Money < num1)
                    Asda2PrivateShopHandler.SendItemBuyedFromPrivateShopResponse(activeCharacter,
                        PrivateShopBuyResult.NotEnoghtGold, (List<Asda2Item>) null);
                else if ((ulong) this.Owner.Money + num1 > (ulong) int.MaxValue)
                {
                    Asda2PrivateShopHandler.SendItemBuyedFromPrivateShopResponse(activeCharacter,
                        PrivateShopBuyResult.Error, (List<Asda2Item>) null);
                    this.SendMessage(this.Owner.Name + " has to much gold.", this.Owner, Locale.Start);
                }
                else if (source.Any<Asda2ItemTradeRef>((Func<Asda2ItemTradeRef, bool>) (asda2ItemTradeRef =>
                {
                    if (asda2ItemTradeRef.Item == null || asda2ItemTradeRef.Item.IsDeleted)
                        return true;
                    if (asda2ItemTradeRef.Item.Amount != 0)
                        return asda2ItemTradeRef.Item.Amount < asda2ItemTradeRef.Amount;
                    return false;
                })))
                {
                    this.Owner.YouAreFuckingCheater("Trying to cheat while trading items from private shop", 10);
                    this.Exit(this.Owner);
                }
                else if (activeCharacter.Asda2Inventory.FreeRegularSlotsCount <
                         source.Count<Asda2ItemTradeRef>((Func<Asda2ItemTradeRef, bool>) (i =>
                             i.Item.InventoryType == Asda2InventoryType.Regular)) ||
                         activeCharacter.Asda2Inventory.FreeShopSlotsCount < source.Count<Asda2ItemTradeRef>(
                             (Func<Asda2ItemTradeRef, bool>) (i => i.Item.InventoryType == Asda2InventoryType.Shop)))
                {
                    Asda2PrivateShopHandler.SendItemBuyedFromPrivateShopResponse(activeCharacter,
                        PrivateShopBuyResult.NoSlotAreAvailable, (List<Asda2Item>) null);
                }
                else
                {
                    activeCharacter.SubtractMoney((uint) num1);
                    this.Owner.AddMoney((uint) num1);
                    List<Asda2Item> buyedItems = new List<Asda2Item>();
                    List<Asda2ItemTradeRef> itemsBuyed = new List<Asda2ItemTradeRef>();
                    foreach (Asda2ItemTradeRef asda2ItemTradeRef1 in source)
                    {
                        if (asda2ItemTradeRef1.Amount == 0)
                            asda2ItemTradeRef1.Amount = 1;
                        if (asda2ItemTradeRef1.Amount >= asda2ItemTradeRef1.Item.Amount)
                        {
                            LogHelperEntry lgDelete = Log
                                .Create(Log.Types.ItemOperations, LogSourceType.Character, this.Owner.EntryId)
                                .AddAttribute("source", 0.0, "selled_from_private_shop")
                                .AddItemAttributes(asda2ItemTradeRef1.Item, "")
                                .AddAttribute("buyer_id", (double) activeCharacter.EntryId, "")
                                .AddAttribute("amount", (double) asda2ItemTradeRef1.Amount, "").Write();
                            Asda2Item itemToCopyStats = asda2ItemTradeRef1.Item;
                            Asda2Item asda2Item = (Asda2Item) null;
                            int num2 = (int) activeCharacter.Asda2Inventory.TryAdd(itemToCopyStats.ItemId,
                                itemToCopyStats.Amount, true, ref asda2Item, new Asda2InventoryType?(),
                                itemToCopyStats);
                            Log.Create(Log.Types.ItemOperations, LogSourceType.Character, activeCharacter.EntryId)
                                .AddAttribute("source", 0.0, "buyed_from_private_shop").AddItemAttributes(asda2Item, "")
                                .AddAttribute("seller_id", (double) this.Owner.EntryId, "").AddReference(lgDelete)
                                .AddAttribute("amount", (double) asda2ItemTradeRef1.Amount, "").Write();
                            buyedItems.Add(asda2Item);
                            itemToCopyStats.Destroy();
                            this.ItemsOnTrade[(int) asda2ItemTradeRef1.TradeSlot].Amount = -1;
                            this.ItemsOnTrade[(int) asda2ItemTradeRef1.TradeSlot].Price = 0;
                        }
                        else
                        {
                            LogHelperEntry lgDelete = Log
                                .Create(Log.Types.ItemOperations, LogSourceType.Character, this.Owner.EntryId)
                                .AddAttribute("source", 0.0, "selled_from_private_shop_split")
                                .AddItemAttributes(asda2ItemTradeRef1.Item, "")
                                .AddAttribute("buyer_id", (double) activeCharacter.EntryId, "")
                                .AddAttribute("amount", (double) asda2ItemTradeRef1.Amount, "").Write();
                            asda2ItemTradeRef1.Item.Amount -= asda2ItemTradeRef1.Amount;
                            Asda2Item asda2Item = (Asda2Item) null;
                            int num2 = (int) activeCharacter.Asda2Inventory.TryAdd(
                                (int) asda2ItemTradeRef1.Item.Template.ItemId, asda2ItemTradeRef1.Amount, true,
                                ref asda2Item, new Asda2InventoryType?(), asda2ItemTradeRef1.Item);
                            Log.Create(Log.Types.ItemOperations, LogSourceType.Character, activeCharacter.EntryId)
                                .AddAttribute("source", 0.0, "buyed_from_private_shop_split")
                                .AddItemAttributes(asda2Item, "new_item")
                                .AddItemAttributes(asda2ItemTradeRef1.Item, "old_item")
                                .AddAttribute("amount", (double) asda2ItemTradeRef1.Amount, "")
                                .AddAttribute("seller_id", (double) this.Owner.EntryId, "").AddReference(lgDelete)
                                .Write();
                            asda2ItemTradeRef1.Item.Save();
                            buyedItems.Add(asda2Item);
                        }

                        Asda2ItemTradeRef asda2ItemTradeRef2 = this.ItemsOnTrade[(int) asda2ItemTradeRef1.TradeSlot];
                        itemsBuyed.Add(asda2ItemTradeRef2);
                        if (asda2ItemTradeRef2 != null && asda2ItemTradeRef2.Amount > 0)
                        {
                            asda2ItemTradeRef2.Amount -= asda2ItemTradeRef1.Amount;
                            if (asda2ItemTradeRef2.Amount <= 0)
                                asda2ItemTradeRef2.Amount = -1;
                        }
                    }

                    Asda2PrivateShopHandler.SendItemBuyedFromPrivateShopResponse(activeCharacter,
                        PrivateShopBuyResult.Ok, buyedItems);
                    Asda2PrivateShopHandler.SendItemBuyedFromPrivateShopToOwnerNotifyResponse(this, itemsBuyed,
                        activeCharacter);
                    Asda2PrivateShopHandler.SendPrivateShopChatNotificationAboutBuyResponse(this, itemsBuyed,
                        activeCharacter);
                    this.Owner.SendMoneyUpdate();
                    activeCharacter.SendMoneyUpdate();
                }
            }));
        }

        public void ShowOnLogin(Character character)
        {
            if (character == this.Owner)
                Asda2PrivateShopHandler.SendPrivateShopOpenedResponse(this.Owner.Client, PrivateShopOpenedResult.Ok,
                    this.ItemsOnTrade);
            else
                Asda2PrivateShopHandler.SendCharacterPrivateShopInfoResponse(character.Client,
                    Asda2ViewTradeShopInfoStatus.Ok, this);
        }
    }
}
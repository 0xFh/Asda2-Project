using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Items;
using WCell.RealmServer.Logs;
using WCell.RealmServer.Network;
using System.Linq;

namespace WCell.RealmServer.Handlers
{
    public static class Asda2PrivateShopHandler
    {
        [PacketHandler(RealmServerOpCode.OpenPrivateShopWindow)] //6300
        public static void OpenPrivateShopWindowRequest(IRealmClient client, RealmPacketIn packet)
        {
            //todo add checks PrivateShopWindowOpenedResult
            if (client.ActiveCharacter.IsAsda2BattlegroundInProgress)
            {
                client.ActiveCharacter.SendInfoMsg("You can't trade on war.");
                SendPrivateShopWindoOpenedResponse(client, PrivateShopWindowOpenedResult.Fail);
                return;
            }
            if (RealmServer.IsPreparingShutdown || RealmServer.IsShuttingDown)
            {
                SendPrivateShopWindoOpenedResponse(client, PrivateShopWindowOpenedResult.Fail);
                return;

            }
            if(client.ActiveCharacter.ChatBanned)
            {
                SendPrivateShopWindoOpenedResponse(client, PrivateShopWindowOpenedResult.Fail);
                return;

            }
            if (client.ActiveCharacter.Asda2TradeWindow != null)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Is trying to open private shop window while trading.");
                SendPrivateShopWindoOpenedResponse(client, PrivateShopWindowOpenedResult.Fail);
                return;
            }
            if (client.ActiveCharacter.PrivateShop == null)
            {
                client.ActiveCharacter.PrivateShop = new Asda2PrivateShop(client.ActiveCharacter);
                SendPrivateShopWindoOpenedResponse(client, PrivateShopWindowOpenedResult.Ok);
            }
            else
            {
                SendPrivateShopWindoOpenedResponse(client, PrivateShopWindowOpenedResult.YouAreInYourShop);
            }
        }

        public static void SendPrivateShopWindoOpenedResponse(IRealmClient client, PrivateShopWindowOpenedResult status)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.PrivateShopWindoOpened)) //6301
            {
                packet.WriteByte((byte)status); //{status}default value : 1 Len : 1
                client.Send(packet, addEnd: false);
            }
        }

        [PacketHandler(RealmServerOpCode.OpenPrivateShop)] //6302
        public static void OpenPrivateShopRequest(IRealmClient client, RealmPacketIn packet)
        {
            //accId(Int32;340701)unk8(Byte;4)[for|0|10|1|itemId(Int32;13387)invNum(Byte;2)slot(Int16;2)quantity(Int16;0)(int16;0)tradeSlot(int16;0)price(Int32;500)(Skip;FFFFFFFFFFFFFFFFFFFF0000FFFF0000FFFF0000FFFF0000FFFF0000)]Title(fAsciiStr;;50)
            if (client.ActiveCharacter.PrivateShop == null)
            {
                SendPrivateShopOpenedResponse(client, PrivateShopOpenedResult.Error, null);
                client.ActiveCharacter.YouAreFuckingCheater("Trying to open not existing private shop", 2);
                return;
            }
            packet.Position += 1; //nk8 default : 4Len : 1
            var itemsToTrade = new List<Asda2ItemTradeRef>();
            for (int i = 0; i < 10; i += 1)
            {
                var exist = false;
                var itemId = packet.ReadInt32(); //default : 13387Len : 4
                if (itemId == 0)
                    exist = true;
                var invNum = (Asda2InventoryType)packet.ReadByte(); //default : 2Len : 1
                var slot = packet.ReadInt16(); //default : 2Len : 4
                var quantity = packet.ReadInt16(); //default : 0Len : 4
                packet.Position += 4;

                if (slot < 0 || slot >= client.ActiveCharacter.Asda2Inventory.ShopItems.Length || quantity < 0)
                {
                    //bad data
                    /*SendPrivateShopOpenedResponse(client, PrivateShopOpenedResult.Error, null);
                    client.ActiveCharacter.YouAreFuckingCheater();
                    return;*/
                    exist = true;
                }
                var price = packet.ReadInt32(); //default : 500Len : 4
                if (!exist)
                {
                    Asda2Item item;
                    item = invNum == Asda2InventoryType.Regular
                               ? client.ActiveCharacter.Asda2Inventory.RegularItems[slot]
                               : client.ActiveCharacter.Asda2Inventory.ShopItems[slot];
                    if (item == null)
                    {
                        //bad data
                        SendPrivateShopOpenedResponse(client, PrivateShopOpenedResult.ThereIsNoItemInfo, null);
                        return;
                    }
                    if (item.Amount < quantity)
                        quantity = (short)item.Amount;
                    foreach (var asda2ItemTradeRef in itemsToTrade)
                    {
                        if (asda2ItemTradeRef.Item.InventoryType == item.InventoryType && asda2ItemTradeRef.Item.Slot == item.Slot)
                        {
                            exist = true;
                            break;
                        }
                    }
                    if (!exist)
                        itemsToTrade.Add(new Asda2ItemTradeRef()
                                             {
                                                 Item = item,
                                                 Amount = quantity,
                                                 Price = price,
                                                 TradeSlot = (byte)itemsToTrade.Count()
                                             });
                }
                packet.Position += 28; //default : stub17Len : 28
            }
            var title = packet.ReadAsdaString(50, Locale.En); //default : Len : 50
            /*if (!Asda2EncodingHelper.IsPrueEnglish(title))
            {
                client.ActiveCharacter.SendOnlyEnglishCharactersAllowed("Shop title");
                SendPrivateShopOpenedResponse(client, PrivateShopOpenedResult.Error, null);
                return;
            }*/
            client.ActiveCharacter.PrivateShop.StartTrade(itemsToTrade, title);
        }


        public static void SendPrivateShopOpenedResponse(IRealmClient client, PrivateShopOpenedResult status,
                                                         Asda2ItemTradeRef[] items)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.PrivateShopOpened)) //6303
            {
                packet.WriteByte((byte)status); //{status}default value : 1 Len : 1
                if (status == PrivateShopOpenedResult.Ok)
                {
                    packet.WriteByte(items.Length); //value name : unk6 default value : 4Len : 1
                    for (int i = 0; i < 10; i += 1)
                    {
                        var item = items == null || items[i] == null || items[i].Item == null ? null : items[i].Item;
                        packet.WriteInt32(item == null ? 0 : item.ItemId); //{itemId}default value : 13387 Len : 4
                        packet.WriteByte((byte)(item == null ? 0 : item.InventoryType));
                        //{invType}default value : 2 Len : 1
                        packet.WriteInt16(item == null ? -1 : item.Slot); //{slot}default value : 2 Len : 2
                        packet.WriteInt32(items == null || items[i] == null ? 0 : items[i].Amount);
                        //{quantity}default value : 0 Len : 4
                        packet.WriteByte(item == null ? 0 : item.Durability); //{durability}default value : 0 Len : 1
                        packet.WriteByte(item == null ? 0 : item.Enchant); //{ench}default value : 0 Len : 1
                        packet.WriteInt32(items == null || items[i] == null ? 0 : items[i].Price);
                        //{price}default value : 500 Len : 4
                        packet.WriteInt16(item == null ? -1 : item.Soul1Id); //{soul1Id%}default value : -1 Len : 2
                        packet.WriteInt16(item == null ? -1 : item.Soul2Id); //{soul1Id%}default value : -1 Len : 2
                        packet.WriteInt16(item == null ? -1 : item.Soul3Id); //{soul1Id%}default value : -1 Len : 2
                        packet.WriteInt16(item == null ? -1 : item.Soul4Id); //{soul1Id%}default value : -1 Len : 2
                        packet.WriteInt16(item == null ? -1 : (Int16)item.Parametr1Type);
                        //{parametr1Type%}default value : -1 Len : 2
                        packet.WriteInt16(item == null ? -1 : item.Parametr1Value);
                        //{paramtetr1Value%}default value : -1 Len : 2
                        packet.WriteInt16(item == null ? -1 : (Int16)item.Parametr2Type);
                        //{parametr1Type%}default value : -1 Len : 2
                        packet.WriteInt16(item == null ? -1 : item.Parametr2Value);
                        //{paramtetr1Value%}default value : -1 Len : 2
                        packet.WriteInt16(item == null ? -1 : (Int16)item.Parametr3Type);
                        //{parametr1Type%}default value : -1 Len : 2
                        packet.WriteInt16(item == null ? -1 : item.Parametr3Value);
                        //{paramtetr1Value%}default value : -1 Len : 2
                        packet.WriteInt16(item == null ? -1 : (Int16)item.Parametr4Type);
                        //{parametr1Type%}default value : -1 Len : 2
                        packet.WriteInt16(item == null ? -1 : item.Parametr4Value);
                        //{paramtetr1Value%}default value : -1 Len : 2
                        packet.WriteInt16(item == null ? -1 : (Int16)item.Parametr5Type);
                        //{parametr1Type%}default value : -1 Len : 2
                        packet.WriteInt16(item == null ? -1 : item.Parametr5Value);
                        //{paramtetr1Value%}default value : -1 Len : 2
                    }
                }
                client.Send(packet, addEnd: false);
            }
        }

        public static void SendtradeStatusTextWindowResponse(Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.TradeStatusTextWindow)) //6304
            {
                packet.WriteByte(chr.IsAsda2TradeDescriptionEnabled ? 1 : 0); //{status}default value : 1 Len : 1
                packet.WriteByte(chr.IsAsda2TradeDescriptionPremium ? 1 : 0);//{backgroundStyle}default value : 0 Len : 1
                packet.WriteInt32(chr.AccId); //{accId}default value : 340701 Len : 4
                packet.WriteFixedAsciiString(chr.Asda2TradeDescription, 50); //{text}default value :  Len : 50
                chr.SendPacketToArea(packet);
            }
        }

        public static void SendtradeStatusTextWindowResponseToOne(Character chr, IRealmClient rcv)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.TradeStatusTextWindow)) //6304
            {
                packet.WriteByte(chr.IsAsda2TradeDescriptionEnabled ? 1 : 0); //{status}default value : 1 Len : 1
                packet.WriteByte(chr.IsAsda2TradeDescriptionPremium ? 1 : 0);
                //{backgroundStyle}default value : 0 Len : 1
                packet.WriteInt32(chr.AccId); //{accId}default value : 340701 Len : 4
                packet.WriteFixedAsciiString(chr.Asda2TradeDescription, 50); //{text}default value :  Len : 50
                rcv.Send(packet, addEnd: true);
            }
        }

        [PacketHandler(RealmServerOpCode.ViewCharacterTradeShop)] //6307
        public static void ViewCharacterTradeShopRequest(IRealmClient client, RealmPacketIn packet)
        {
            var targetAccId = packet.ReadUInt32(); //default : 355338Len : 4
            var targetAcc = RealmServer.Instance.GetLoggedInAccount(targetAccId);
            if (targetAcc == null || targetAcc.ActiveCharacter == null)
            {
                //client.ActiveCharacter.YouAreFuckingCheater("Trying to view character trade shop with wrong parametrs",50);
                SendCharacterPrivateShopInfoResponse(client, Asda2ViewTradeShopInfoStatus.TheCapacityHasExided, null);
                return;
            }
            var targetChr = targetAcc.ActiveCharacter;
            if (targetChr.PrivateShop == null || !targetChr.PrivateShop.Trading)
            {
                //SendCharacterPrivateShopInfoResponse(client, Asda2ViewTradeShopInfoStatus.ThereIsNoPrivateShop, null);
                return;
            }
            targetChr.PrivateShop.Join(client.ActiveCharacter);
        }

        public static void SendCharacterPrivateShopInfoResponse(IRealmClient client,
                                                                Asda2ViewTradeShopInfoStatus infoStatus,
                                                                Asda2PrivateShop shop)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.CharacterPrivateShopInfo)) //6308
            {
                packet.WriteByte((byte)infoStatus); //{status}default value : 1 Len : 1
                if (infoStatus == Asda2ViewTradeShopInfoStatus.Ok)
                {
                    packet.WriteInt32(shop.Owner.AccId); //{shopAccId}default value : 355338 Len : 4
                    packet.WriteInt16(shop.Owner.SessionId); //{shopSessId}default value : 141 Len : 2
                    packet.WriteByte(shop.ItemsCount); //{shopItemsCount}default value : 10 Len : 1
                    packet.WriteFixedAsciiString(shop.Owner.Asda2TradeDescription, 50);
                    //{shopMessage}default value :  Len : 50
                    packet.WriteFixedAsciiString(shop.Owner.Name, 20); //{shopOwnerName}default value :  Len : 20
                    for (int i = 0; i < 10; i += 1)
                    {
                        var item = shop.ItemsOnTrade[i] == null || shop.ItemsOnTrade[i].Item == null
                                       ? null
                                       : shop.ItemsOnTrade[i].Item;
                        packet.WriteInt32(item == null ? 0 : item.ItemId); //{itemId%}default value : 0 Len : 4
                        packet.WriteInt32(shop.ItemsOnTrade[i] == null ? -1 : shop.ItemsOnTrade[i].Amount);
                        //{quantity}default value : 0 Len : 4
                        packet.WriteByte(item == null ? 0 : item.Durability); //{durability%}default value : 0 Len : 1
                        packet.WriteInt16(item == null ? 0 : item.Weight); //{weight%}default value : 0 Len : 2
                        packet.WriteInt16(item == null ? 0 : item.Soul1Id); //{soul1Id%}default value : -1 Len : 2
                        packet.WriteInt16(item == null ? 0 : item.Soul2Id); //{soul1Id%}default value : -1 Len : 2
                        packet.WriteInt16(item == null ? 0 : item.Soul3Id); //{soul1Id%}default value : -1 Len : 2
                        packet.WriteInt16(item == null ? 0 : item.Soul4Id); //{soul1Id%}default value : -1 Len : 2
                        packet.WriteInt16(item == null ? 0 : item.Enchant); //{enchant%}default value : 0 Len : 2
                        packet.WriteInt16(0); //value name : unk2 default value : 0Len : 2
                        packet.WriteByte(0); //value name : unk1 default value : 0Len : 1
                        packet.WriteInt16((Int16)(item == null ? 0 : item.Parametr1Type));
                        //{parametr1Type%}default value : -1 Len : 2
                        packet.WriteInt16(item == null ? 0 : item.Parametr1Value);
                        //{paramtetr1Value%}default value : -1 Len : 2
                        packet.WriteInt16((Int16)(item == null ? 0 : item.Parametr2Type));
                        //{parametr1Type%}default value : -1 Len : 2
                        packet.WriteInt16(item == null ? 0 : item.Parametr2Value);
                        //{paramtetr1Value%}default value : -1 Len : 2
                        packet.WriteInt16((Int16)(item == null ? 0 : item.Parametr3Type));
                        //{parametr1Type%}default value : -1 Len : 2
                        packet.WriteInt16(item == null ? 0 : item.Parametr3Value);
                        //{paramtetr1Value%}default value : -1 Len : 2
                        packet.WriteInt16((Int16)(item == null ? 0 : item.Parametr4Type));
                        //{parametr1Type%}default value : -1 Len : 2
                        packet.WriteInt16(item == null ? 0 : item.Parametr4Value);
                        //{paramtetr1Value%}default value : -1 Len : 2
                        packet.WriteInt16((Int16)(item == null ? 0 : item.Parametr5Type));
                        //{parametr1Type%}default value : -1 Len : 2
                        packet.WriteInt16(item == null ? 0 : item.Parametr5Value);
                        //{paramtetr1Value%}default value : -1 Len : 2
                        packet.WriteByte(0); //value name : unk1 default value : 0Len : 1
                        packet.WriteInt32(shop.ItemsOnTrade[i] == null ? -1 : shop.ItemsOnTrade[i].Price);
                        //{price}default value : 1 Len : 4
                        packet.WriteInt32(item == null ? 0 : 264156);
                        //{ifBuyed0IfNotdefault}default value : 264156 Len : 4
                        packet.WriteInt16(item == null ? 0 : 1); //{ifBuyed0IfNot1}default value : 0 Len : 2
                    }
                }
                client.Send(packet, addEnd: false);
            }
        }


        [PacketHandler(RealmServerOpCode.PrivateShopChatReq)] //6313
        public static void PrivateShopChatReqRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 22;

            var msg = packet.ReadAsciiString(client.Locale); //default : Len : 200
            if (client.ActiveCharacter.PrivateShop == null)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to chat to private shop while not exist shop.", 2);
                return;
            }
            //if (client.ActiveCharacter.ChatBanned)
            //{
            //    client.ActiveCharacter.SendInfoMsg("you are banned");
            //    return;
            //}
            var locale = Asda2EncodingHelper.MinimumAvailableLocale(client.Locale, msg);
            client.ActiveCharacter.PrivateShop.SendMessage(msg, client.ActiveCharacter, locale);
        }

        public static RealmPacketOut CreatePrivateShopChatResResponse(Character sender, string message, Locale locale)
        {
            var packet = new RealmPacketOut(RealmServerOpCode.PrivateShopChatRes); //6314
            packet.WriteInt32(1); //value name : unk5 default value : 1Len : 4
            packet.WriteFixedAsciiString(sender.Name, 20, locale); //{senderName}default value :  Len : 20
            packet.WriteAsciiString(message, locale); //{msg}default value :  Len : 200
            packet.WriteByte(0);
            return packet;
        }

        [PacketHandler(RealmServerOpCode.BuyItemFromCharacterPrivateShop)] //6309
        public static void BuyItemFromCharacterPrivateShopRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.PrivateShop == null)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to buy from private shop while it not exist", 0);
                return;
            }
            packet.Position += 7;
            var itemsToBuyRefs = new List<Asda2ItemTradeRef>();
            for (int i = 0; i < 6; i += 1)
            {
                var itemId = packet.ReadInt32(); //default : 37815Len : 4
                if (itemId == 0)
                    break;
                packet.Position += 3; //tab46 default : stab46Len : 3
                var amount = packet.ReadInt32(); //default : 2Len : 4
                var slot = packet.ReadInt16(); //default : 3Len : 2
                if (amount < 0 || slot < 0 || slot > 9)
                {
                    client.ActiveCharacter.YouAreFuckingCheater("Trying to buy items from private shop from wrong slot", 50);
                    return;
                }
                itemsToBuyRefs.Add(new Asda2ItemTradeRef() { Amount = amount == 0 ? 1 : amount, TradeSlot = (byte)slot });
                //var price = packet.ReadInt32(); //default : 50000Len : 4
                packet.Position += 32; //nk15 default : unk15Len : 28
            }
            client.ActiveCharacter.PrivateShop.BuyItems(client.ActiveCharacter, itemsToBuyRefs);
        }

        public static void SendItemBuyedFromPrivateShopResponse(Character chr, PrivateShopBuyResult status,
                                                                List<Asda2Item> buyedItems)
        {
            var items = new Asda2Item[6];
            if (buyedItems != null)
            {
                for (int i = 0; i < buyedItems.Count; i++)
                {
                    items[i] = buyedItems[i];
                }
            }
            using (var packet = new RealmPacketOut(RealmServerOpCode.ItemBuyedFromPrivateShop)) //6310
            {
                packet.WriteByte((byte)status); //{status}default value : 1 Len : 1
                if (status == PrivateShopBuyResult.Ok)
                {
                    packet.WriteInt16(chr.Asda2Inventory.Weight); //{invWeight}default value : 12715 Len : 2
                    packet.WriteInt32(chr.Money); //{money}default value : 7557553 Len : 4
                    packet.WriteByte(buyedItems.Count(i => i != null)); //{itemsCount}default value : 1 Len : 1
                    for (int i = 0; i < 6; i += 1)
                    {
                        Asda2InventoryHandler.WriteItemInfoToPacket(packet, items[i], false);
                    }
                }
                chr.Send(packet, addEnd: false);
            }
        }

        [PacketHandler(RealmServerOpCode.CloseCharacterTradeShop)] //6305
        public static void CloseCharacterTradeShopRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.PrivateShop == null)
                SendCloseCharacterTradeShopToOwnerResponse(client, Asda2PrivateShopClosedToOwnerResult.Ok);
            else
            {
                client.ActiveCharacter.PrivateShop.Exit(client.ActiveCharacter);
            }
        }

        public static void SendCloseCharacterTradeShopToOwnerResponse(IRealmClient client,
                                                                      Asda2PrivateShopClosedToOwnerResult status)
        {
            using (var packet = CreateCloseCharacterTradeShopToOwnerResponse(status))
            {
                client.Send(packet, addEnd: false);
            }
        }

        public static RealmPacketOut CreateCloseCharacterTradeShopToOwnerResponse(
            Asda2PrivateShopClosedToOwnerResult status)
        {
            var packet = new RealmPacketOut(RealmServerOpCode.CloseCharacterTradeShopResponseToOwner); //6306
            packet.WriteByte((byte)status); //{status}default value : 1 Len : 1
            packet.WriteSkip(stub1); //{stub1}default value : stub1 Len : 570
            return packet;
        }

        public static RealmPacketOut CreatePrivateShopChatNotificationResponse(uint trigererAccId,
                                                                               Asda2PrivateShopNotificationType
                                                                                   notificationType)
        {
            var packet = new RealmPacketOut(RealmServerOpCode.PrivateShopChatNotification); //6315
            packet.WriteByte((byte)notificationType); //{notificationType}default value : 2 Len : 1
            packet.WriteInt32(trigererAccId); //{trigerAccId}default value : 326897 Len : 4
            return packet;
        }


        private static readonly byte[] stub1 = new byte[]
                                                   {
                                                       0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00,
                                                       0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                                       0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF,
                                                       0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                                       0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                       0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF,
                                                       0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF,
                                                       0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF,
                                                       0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                                       0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00,
                                                       0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF,
                                                       0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF,
                                                       0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                       0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                                       0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00,
                                                       0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                       0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                       0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00,
                                                       0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                                       0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                                       0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                       0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                       0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00,
                                                       0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                                       0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                                       0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                       0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00,
                                                       0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                                       0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                                       0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                                       0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                       0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00,
                                                       0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                                       0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF,
                                                       0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                                       0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                       0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF,
                                                       0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF,
                                                       0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF,
                                                       0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                                       0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00,
                                                       0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF,
                                                       0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF,
                                                       0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00,
                                                       0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                                       0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00,
                                                       0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                       0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                       0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00,
                                                       0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                                       0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                                       0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                                                   };

        public static void SendItemBuyedFromPrivateShopToOwnerNotifyResponse(Asda2PrivateShop shop,
                                                                             List<Asda2ItemTradeRef> itemsBuyed,
                                                                             Character buyer)
        {
            var items = new Asda2ItemTradeRef[6];
            for (int i = 0; i < itemsBuyed.Count; i++)
            {
                items[i] = itemsBuyed[i];
            }
            using (var packet = new RealmPacketOut(RealmServerOpCode.ItemBuyedFromPrivateShopToOwnerNotify)) //6311
            {
                packet.WriteInt16(shop.Owner.Asda2Inventory.Weight); //{invWeigt}default value : 11845 Len : 2
                packet.WriteInt32(shop.Owner.Money); //{money}default value : 7498054 Len : 4
                packet.WriteByte(items.Count(i => i != null)); //{buyCount}default value : 3 Len : 1
                packet.WriteInt32(buyer.AccId); //{buyerAccId}default value : 354889 Len : 4
                for (int i = 0; i < 6; i += 1)
                {
                    var item = items[i] == null || items[i].Item == null ? null : items[i].Item;
                    packet.WriteInt32(item == null ? 0 : item.ItemId); //{itemId}default value : 37857 Len : 4
                    packet.WriteByte((byte)(item == null ? 0 : item.InventoryType)); //{inv}default value : 2 Len : 1
                    packet.WriteInt16(item == null ? 0 : item.Slot); //{slot}default value : 15 Len : 2
                    packet.WriteInt32(items[i] == null
                                          ? -1
                                          : items[i].Amount);
                    //{amountAfterBuy}default value : -1 Len : 4
                    packet.WriteInt32(items[i] == null ? -1 : items[i].TradeSlot);
                    //{tradeSlot}default value : 5 Len : 4
                    packet.WriteInt16(0); //value name : unk14 default value : 0Len : 2
                    packet.WriteSkip(stub17); //{stub17}default value : stub17 Len : 28

                }
                shop.Owner.Send(packet, addEnd: false);
            }
        }

        private static readonly byte[] stub17 = new byte[]
                                                    {
                                                        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
                                                        ,
                                                        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
                                                        ,
                                                        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
                                                    };

        public static void SendPrivateShopChatNotificationAboutBuyResponse(Asda2PrivateShop shop,
                                                                           List<Asda2ItemTradeRef> itemsBuyed,
                                                                           Character buyer)
        {
            var items = new Asda2ItemTradeRef[6];
            for (int i = 0; i < itemsBuyed.Count; i++)
            {
                items[i] = itemsBuyed[i];
            }
            using (var packet = new RealmPacketOut(RealmServerOpCode.PrivateShopChatNotificationAboutBuy)) //6312
            {
                packet.WriteInt32(shop.Owner.AccId); //{ownerAccId}default value : 340701 Len : 4
                packet.WriteInt16(shop.Owner.SessionId); //{OwnerSessId}default value : 4 Len : 2
                packet.WriteByte(items.Count(i => i != null)); //{buyedItems}default value : 5 Len : 1
                packet.WriteInt32(buyer.AccId); //{buyerAccId}default value : 354889 Len : 4
                for (int i = 0; i < 6; i += 1)
                {
                    var item = items[i] == null || items[i].Item == null ? null : items[i].Item;
                    packet.WriteInt32(item == null ? 0 : item.ItemId); //{itemId}default value : 33702 Len : 4
                    packet.WriteByte(0); //value name : unk10 default value : 0Len : 1
                    packet.WriteInt16(-1); //value name : unk12 default value : -1Len : 2
                    packet.WriteInt32(items[i] == null ? -1 : items[i].Amount);
                    //{amoutOst}default value : -1 Len : 4
                    packet.WriteInt32(items[i] == null ? -1 : items[i].TradeSlot);
                    //{slot}default value : 0 Len : 4
                    packet.WriteInt16(0); //value name : unk14 default value : 0Len : 2
                    packet.WriteSkip(stub18); //{stub17}default value : stub17 Len : 28

                }
                shop.Send(packet);
            }
        }

        private static readonly byte[] stub18 = new byte[]
                                                    {
                                                        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
                                                        ,
                                                        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
                                                        ,
                                                        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
                                                    };

    }

    public enum Asda2PrivateShopNotificationType
    {
        Left = 0,
        Joined = 1
    }

    //emote 109 - stand
    //emote 108 siting
    public enum Asda2ViewTradeShopInfoStatus
    {
        ThereIsNoPrivateShop = 0,
        Ok = 1,
        TheCapacityHasExided = 2,
    }

    public class Asda2PrivateShop
    {
        public List<Character> JoinedCharacters
        {
            get { return _joinedCharacters ?? (_joinedCharacters = new List<Character>()); }
        }
        private Asda2ItemTradeRef[] _itemsOnTrade;
        private List<Character> _joinedCharacters;

        public Asda2PrivateShop(Character owner)
        {
            Owner = owner;
        }

        public Character Owner { get; set; }
        public Asda2ItemTradeRef[] ItemsOnTrade
        {
            get { return _itemsOnTrade ?? (_itemsOnTrade = new Asda2ItemTradeRef[10]); }
        }
        public byte ItemsCount
        {
            get { return (byte)ItemsOnTrade.Count(i => i != null); }
        }
        public void Send(RealmPacketOut packet)
        {
            foreach (var joinedCharacter in JoinedCharacters)
                joinedCharacter.Send(packet, addEnd: false);
        }

        public PrivateShopOpenedResult StartTrade(List<Asda2ItemTradeRef> itemsToTrade, string title)
        {
            Owner.Asda2TradeDescription = title;
            for (int i = 0; i < ItemsOnTrade.Length; i++)
            {
                ItemsOnTrade[i] = itemsToTrade.Count <= i ? null : itemsToTrade[i];
            }
            Asda2PrivateShopHandler.SendPrivateShopOpenedResponse(Owner.Client, PrivateShopOpenedResult.Ok, ItemsOnTrade);
            Owner.IsSitting = true;
            Owner.IsAsda2TradeDescriptionEnabled = true;
            Trading = true;
            return PrivateShopOpenedResult.Ok;
        }

        public bool Trading { get; set; }

        public void Join(Character activeCharacter)
        {
            var rcvrs = new List<Character> { Owner };
            rcvrs.AddRange(JoinedCharacters);
            using (var p = Asda2PrivateShopHandler.CreatePrivateShopChatNotificationResponse(activeCharacter.AccId, Asda2PrivateShopNotificationType.Joined))
            {
                foreach (var character in rcvrs)
                {
                    character.Send(p, addEnd: false);
                }
            }
            activeCharacter.Stunned++;
            JoinedCharacters.Add(activeCharacter);
            Asda2PrivateShopHandler.SendCharacterPrivateShopInfoResponse(activeCharacter.Client, Asda2ViewTradeShopInfoStatus.Ok, this);
            activeCharacter.PrivateShop = this;
        }

        public void SendMessage(string msg, Character activeCharacter, Locale locale)
        {
            var rcvrs = new List<Character> { Owner };
            rcvrs.AddRange(JoinedCharacters);
            using (var p = Asda2PrivateShopHandler.CreatePrivateShopChatResResponse(activeCharacter, msg, locale))
            {
                foreach (var character in rcvrs)
                {
                    if(locale==Locale.Any||character.Client.Locale == locale)
                        character.Send(p, addEnd: false);
                }
            }
        }

        public void Exit(Character activeCharacter)
        {
            if (activeCharacter == Owner)
            {
                using (var p = Asda2PrivateShopHandler.CreateCloseCharacterTradeShopToOwnerResponse(Asda2PrivateShopClosedToOwnerResult.HostClosedShop))
                {
                    foreach (var joinedCharacter in JoinedCharacters)
                    {
                        joinedCharacter.Send(p, addEnd: false);
                        joinedCharacter.PrivateShop = null;
                        joinedCharacter.Stunned--;
                    }
                }
                JoinedCharacters.Clear();
                _joinedCharacters = null;
                _itemsOnTrade = null;
                activeCharacter.PrivateShop = null;
                activeCharacter.IsAsda2TradeDescriptionEnabled = false;
                activeCharacter.Asda2TradeDescription = "";
                Asda2PrivateShopHandler.SendCloseCharacterTradeShopToOwnerResponse(activeCharacter.Client, Asda2PrivateShopClosedToOwnerResult.Ok);
            }
            else
            {
                JoinedCharacters.Remove(activeCharacter);
                var rcvrs = new List<Character> { Owner };
                rcvrs.AddRange(JoinedCharacters);
                using (var p = Asda2PrivateShopHandler.CreatePrivateShopChatNotificationResponse(activeCharacter.AccId, Asda2PrivateShopNotificationType.Left))
                {
                    foreach (var character in rcvrs)
                    {
                        character.Send(p, addEnd: false);
                    }
                }
                activeCharacter.Stunned--;
                activeCharacter.PrivateShop = null;
                Asda2PrivateShopHandler.SendCloseCharacterTradeShopToOwnerResponse(activeCharacter.Client, Asda2PrivateShopClosedToOwnerResult.Ok);
            }
        }

        public void BuyItems(Character activeCharacter, List<Asda2ItemTradeRef> itemsToBuyRefs)
        {
            Owner.Map.AddMessage(() =>
                                     {
                                         var itemsToBy = new List<Asda2ItemTradeRef>();
                                         foreach (var asda2ItemTradeRef in itemsToBuyRefs)
                                         {
                                             var item = ItemsOnTrade[asda2ItemTradeRef.TradeSlot];
                                             if (item == null || item.Amount == -1 || (item.Amount != 0 && item.Amount < asda2ItemTradeRef.Amount))
                                             {
                                                 Asda2PrivateShopHandler.SendItemBuyedFromPrivateShopResponse(activeCharacter, PrivateShopBuyResult.RequestedNumberOfItemsIsNoLongerAvaliable, null);
                                                 return;
                                             }
                                             if (RealmServer.IsPreparingShutdown || RealmServer.IsShuttingDown)
                                             {
                                                 Asda2PrivateShopHandler.SendItemBuyedFromPrivateShopResponse(activeCharacter, PrivateShopBuyResult.SelectedItemsIsNotAvailable, null);
                                                 return;
                                             }
                                             itemsToBy.Add(new Asda2ItemTradeRef { Amount = asda2ItemTradeRef.Amount, Item = item.Item, Price = item.Price, TradeSlot = item.TradeSlot });
                                         }
                                         var dealMoney = itemsToBy.Aggregate<Asda2ItemTradeRef, ulong>(0u, (current, asda2ItemTradeRef) => current + (ulong)(asda2ItemTradeRef.Price * asda2ItemTradeRef.Amount));
                                         if (dealMoney > int.MaxValue)
                                         {
                                             activeCharacter.YouAreFuckingCheater("Trying to buy items with wrong money amount.", 50);
                                             Asda2PrivateShopHandler.SendItemBuyedFromPrivateShopResponse(activeCharacter, PrivateShopBuyResult.NotEnoghtGold, null);
                                             return;
                                         }
                                         if (activeCharacter.Money < dealMoney)
                                         {
                                             Asda2PrivateShopHandler.SendItemBuyedFromPrivateShopResponse(activeCharacter, PrivateShopBuyResult.NotEnoghtGold, null);
                                             return;
                                         }
                                         if (Owner.Money + dealMoney > int.MaxValue)
                                         {
                                             Asda2PrivateShopHandler.SendItemBuyedFromPrivateShopResponse(activeCharacter, PrivateShopBuyResult.Error, null);
                                             SendMessage(Owner.Name + " has to much gold.", Owner, Locale.En);
                                             return;
                                         }
                                         //check items amount
                                         if (itemsToBy.Any(asda2ItemTradeRef => asda2ItemTradeRef.Item == null || asda2ItemTradeRef.Item.IsDeleted || (asda2ItemTradeRef.Item.Amount != 0 && asda2ItemTradeRef.Item.Amount < asda2ItemTradeRef.Amount)))
                                         {
                                             Owner.YouAreFuckingCheater("Trying to cheat while trading items from private shop", 10);
                                             Exit(Owner);
                                             return;
                                         }
                                         //transer items
                                         var regularSlots =
                                             itemsToBy.Count(
                                                 i => i.Item.InventoryType == Asda2InventoryType.Regular);
                                         var shopSlots =
                                             itemsToBy.Count(
                                                 i => i.Item.InventoryType == Asda2InventoryType.Shop);
                                         if (activeCharacter.Asda2Inventory.FreeRegularSlotsCount < regularSlots || activeCharacter.Asda2Inventory.FreeShopSlotsCount < shopSlots)
                                         {
                                             Asda2PrivateShopHandler.SendItemBuyedFromPrivateShopResponse(activeCharacter, PrivateShopBuyResult.NoSlotAreAvailable, null);
                                             return;
                                         }
                                         activeCharacter.SubtractMoney((uint)dealMoney);
                                         Owner.AddMoney((uint)dealMoney);
                                         var addedToBuyer = new List<Asda2Item>();
                                         var buyedItemsRefs = new List<Asda2ItemTradeRef>();
                                         foreach (var asda2ItemTradeRef in itemsToBy)
                                         {
                                             if (asda2ItemTradeRef.Amount == 0)
                                                 asda2ItemTradeRef.Amount = 1;
                                             if (asda2ItemTradeRef.Amount >= asda2ItemTradeRef.Item.Amount)
                                             {
                                                 var sellLog = Log.Create(Log.Types.ItemOperations, LogSourceType.Character, Owner.EntryId)
                                                      .AddAttribute("source", 0, "selled_from_private_shop")
                                                      .AddItemAttributes(asda2ItemTradeRef.Item)
                                                      .AddAttribute("buyer_id", activeCharacter.EntryId)
                                                      .AddAttribute("amount", asda2ItemTradeRef.Amount)
                                                      .Write();
                                                 var item = asda2ItemTradeRef.Item;
                                                 Asda2Item addedItem = null;
                                                 activeCharacter.Asda2Inventory.TryAdd(item.ItemId,item.Amount,true,ref addedItem,null,item);
                                                 Log.Create(Log.Types.ItemOperations, LogSourceType.Character, activeCharacter.EntryId)
                                                    .AddAttribute("source", 0, "buyed_from_private_shop")
                                                    .AddItemAttributes(addedItem)
                                                    .AddAttribute("seller_id", Owner.EntryId)
                                                    .AddReference(sellLog)
                                                    .AddAttribute("amount", asda2ItemTradeRef.Amount)
                                                    .Write();
                                                 addedToBuyer.Add(addedItem);
                                                 item.Destroy();
                                                 ItemsOnTrade[asda2ItemTradeRef.TradeSlot].Amount = -1;
                                                 //ItemsOnTrade[asda2ItemTradeRef.TradeSlot].Item = SelledItem;
                                                 ItemsOnTrade[asda2ItemTradeRef.TradeSlot].Price = 0;
                                             }
                                             else
                                             {
                                                 var sellLog = Log.Create(Log.Types.ItemOperations, LogSourceType.Character, Owner.EntryId)
                                                     .AddAttribute("source", 0, "selled_from_private_shop_split")
                                                     .AddItemAttributes(asda2ItemTradeRef.Item)
                                                     .AddAttribute("buyer_id", activeCharacter.EntryId)
                                                     .AddAttribute("amount", asda2ItemTradeRef.Amount)
                                                     .Write();
                                                 asda2ItemTradeRef.Item.Amount -= asda2ItemTradeRef.Amount;
                                                 Asda2Item newItem = null;
                                                 activeCharacter.Asda2Inventory.TryAdd((int)asda2ItemTradeRef.Item.Template.ItemId, asda2ItemTradeRef.Amount, true, ref newItem,null,asda2ItemTradeRef.Item);
                                                 Log.Create(Log.Types.ItemOperations, LogSourceType.Character, activeCharacter.EntryId)
                                                    .AddAttribute("source", 0, "buyed_from_private_shop_split")
                                                    .AddItemAttributes(newItem, "new_item")
                                                    .AddItemAttributes(asda2ItemTradeRef.Item, "old_item")
                                                    .AddAttribute("amount", asda2ItemTradeRef.Amount)
                                                    .AddAttribute("seller_id", Owner.EntryId)
                                                    .AddReference(sellLog)
                                                    .Write();
                                                 asda2ItemTradeRef.Item.Save();
                                                 addedToBuyer.Add(newItem);
                                             }
                                             var itemTradeRef = ItemsOnTrade[asda2ItemTradeRef.TradeSlot];
                                             buyedItemsRefs.Add(itemTradeRef);
                                             if (itemTradeRef != null && itemTradeRef.Amount > 0)
                                             {
                                                 itemTradeRef.Amount -= asda2ItemTradeRef.Amount;
                                                 if (itemTradeRef.Amount <= 0)
                                                     itemTradeRef.Amount = -1;
                                             }
                                         }
                                         Asda2PrivateShopHandler.SendItemBuyedFromPrivateShopResponse(activeCharacter, PrivateShopBuyResult.Ok, addedToBuyer);
                                         Asda2PrivateShopHandler.SendItemBuyedFromPrivateShopToOwnerNotifyResponse(this, buyedItemsRefs, activeCharacter);
                                         Asda2PrivateShopHandler.SendPrivateShopChatNotificationAboutBuyResponse(this, buyedItemsRefs, activeCharacter);
                                         Owner.SendMoneyUpdate();
                                         activeCharacter.SendMoneyUpdate();
                                     });
        }

        public static Asda2Item SelledItem = new Asda2Item() { ItemId = 36830, IsDeleted = true };

        public void ShowOnLogin(Character character)
        {
            if (character == Owner)
            {
                Asda2PrivateShopHandler.SendPrivateShopOpenedResponse(Owner.Client, PrivateShopOpenedResult.Ok, ItemsOnTrade);
            }
            else
            {
                Asda2PrivateShopHandler.SendCharacterPrivateShopInfoResponse(character.Client, Asda2ViewTradeShopInfoStatus.Ok, this);
            }
        }
    }

    public enum PrivateShopWindowOpenedResult
    {
        Fail = 0,
        Ok = 1,
        YouAreInYourShop = 2,
        YouAreInWar = 3,
        YouAreDead = 4,
        YourLevelMustBeHigherThanTen = 5,
        NoInfoAbountFunctionItem = 6,
        CantOpenPrivateShopInsidePvpZones = 7,

    }
    public enum PrivateShopOpenedResult
    {
        Error = 0,
        Ok = 1,
        ThereIsNoItemInfo = 2,
        ItemIsAlreadyInPlace = 3,
        ThisItemIsUnexchangeable = 4,
        ThisItemCantBeTradedDueToTheequippedSowel = 5,
        YouOwnMoreItemsThanAllowed = 6,
        YouCantTradeTheGold = 7,
        YouMastBeMoreThan24Level = 8,
    }

    public enum PrivateShopChatNotificationType
    {
        Joined = 1,
        Left = 2,
    }
    public enum PrivateShopBuyResult
    {
        Error = 0,
        Ok = 1,
        SelectedItemsIsNotAvailable = 2,
        UserClosedTheWindow = 3,
        WeightValueExceedsTheLimit = 4,
        NoSlotAreAvailable = 5,
        NotEnoghtGold = 6,
        RequestedNumberOfItemsIsNoLongerAvaliable = 7,
        AnotherPlayerHasAlreadyPurchasedTheItemPleasyTryAgain = 8,
        OnlyThoseOver24LevelAreAlowedToExchangePurchasedItems = 9,


    }
    public enum Asda2PrivateShopClosedToOwnerResult
    {
        Error = 0,
        Ok = 1,
        HostClosedShop = 2,
    }
}
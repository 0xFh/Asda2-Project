using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class Asda2PrivateShopHandler
    {
        private static readonly byte[] stub1 = new byte[570]
        {
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0,
            (byte) 0
        };

        private static readonly byte[] stub17 = new byte[28]
        {
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue
        };

        private static readonly byte[] stub18 = new byte[28]
        {
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue,
            byte.MaxValue
        };

        [PacketHandler(RealmServerOpCode.OpenPrivateShopWindow)]
        public static void OpenPrivateShopWindowRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.IsAsda2BattlegroundInProgress)
            {
                client.ActiveCharacter.SendInfoMsg("You can't trade on war.");
                Asda2PrivateShopHandler.SendPrivateShopWindoOpenedResponse(client, PrivateShopWindowOpenedResult.Fail);
            }
            else if (client.ActiveCharacter.Asda2TradeWindow != null)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Is trying to open private shop window while trading.", 1);
                Asda2PrivateShopHandler.SendPrivateShopWindoOpenedResponse(client, PrivateShopWindowOpenedResult.Fail);
            }
            else if (client.ActiveCharacter.PrivateShop == null)
            {
                client.ActiveCharacter.PrivateShop = new Asda2PrivateShop(client.ActiveCharacter);
                Asda2PrivateShopHandler.SendPrivateShopWindoOpenedResponse(client, PrivateShopWindowOpenedResult.Ok);
            }
            else
                Asda2PrivateShopHandler.SendPrivateShopWindoOpenedResponse(client,
                    PrivateShopWindowOpenedResult.YouAreInYourShop);
        }

        public static void SendPrivateShopWindoOpenedResponse(IRealmClient client, PrivateShopWindowOpenedResult status)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PrivateShopWindoOpened))
            {
                packet.WriteByte((byte) status);
                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.OpenPrivateShop)]
        public static void OpenPrivateShopRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.PrivateShop == null)
            {
                Asda2PrivateShopHandler.SendPrivateShopOpenedResponse(client, PrivateShopOpenedResult.Error,
                    (Asda2ItemTradeRef[]) null);
                client.ActiveCharacter.YouAreFuckingCheater("Trying to open not existing private shop", 2);
            }
            else
            {
                ++packet.Position;
                List<Asda2ItemTradeRef> asda2ItemTradeRefList = new List<Asda2ItemTradeRef>();
                for (int index = 0; index < 10; ++index)
                {
                    bool flag = false;
                    if (packet.ReadInt32() == 0)
                        flag = true;
                    Asda2InventoryType asda2InventoryType = (Asda2InventoryType) packet.ReadByte();
                    short num1 = packet.ReadInt16();
                    short num2 = packet.ReadInt16();
                    packet.Position += 4;
                    if (num1 < (short) 0 || (int) num1 >= client.ActiveCharacter.Asda2Inventory.ShopItems.Length ||
                        num2 < (short) 0)
                        flag = true;
                    int num3 = packet.ReadInt32();
                    if (!flag)
                    {
                        Asda2Item asda2Item = asda2InventoryType == Asda2InventoryType.Regular
                            ? client.ActiveCharacter.Asda2Inventory.RegularItems[(int) num1]
                            : client.ActiveCharacter.Asda2Inventory.ShopItems[(int) num1];
                        if (asda2Item == null)
                        {
                            Asda2PrivateShopHandler.SendPrivateShopOpenedResponse(client,
                                PrivateShopOpenedResult.ThereIsNoItemInfo, (Asda2ItemTradeRef[]) null);
                            return;
                        }

                        if (asda2Item.Amount < (int) num2)
                            num2 = (short) asda2Item.Amount;
                        foreach (Asda2ItemTradeRef asda2ItemTradeRef in asda2ItemTradeRefList)
                        {
                            if (asda2ItemTradeRef.Item.InventoryType == asda2Item.InventoryType &&
                                (int) asda2ItemTradeRef.Item.Slot == (int) asda2Item.Slot)
                            {
                                flag = true;
                                break;
                            }
                        }

                        if (!flag)
                            asda2ItemTradeRefList.Add(new Asda2ItemTradeRef()
                            {
                                Item = asda2Item,
                                Amount = (int) num2,
                                Price = num3,
                                TradeSlot = (byte) asda2ItemTradeRefList.Count<Asda2ItemTradeRef>()
                            });
                    }

                    packet.Position += 28;
                }

                string str = packet.ReadAsdaString(50, Locale.Start);
                if (!Asda2EncodingHelper.IsPrueEnglish(str))
                {
                    client.ActiveCharacter.SendOnlyEnglishCharactersAllowed("Shop title");
                    Asda2PrivateShopHandler.SendPrivateShopOpenedResponse(client, PrivateShopOpenedResult.Error,
                        (Asda2ItemTradeRef[]) null);
                }
                else
                {
                    int num = (int) client.ActiveCharacter.PrivateShop.StartTrade(asda2ItemTradeRefList, str);
                }
            }
        }

        public static void SendPrivateShopOpenedResponse(IRealmClient client, PrivateShopOpenedResult status,
            Asda2ItemTradeRef[] items)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PrivateShopOpened))
            {
                packet.WriteByte((byte) status);
                if (status == PrivateShopOpenedResult.Ok)
                {
                    packet.WriteByte(items.Length);
                    for (int index = 0; index < 10; ++index)
                    {
                        Asda2Item asda2Item = items == null || items[index] == null || items[index].Item == null
                            ? (Asda2Item) null
                            : items[index].Item;
                        packet.WriteInt32(asda2Item == null ? 0 : asda2Item.ItemId);
                        packet.WriteByte(asda2Item == null ? (byte) 0 : (byte) asda2Item.InventoryType);
                        packet.WriteInt16(asda2Item == null ? -1 : (int) asda2Item.Slot);
                        packet.WriteInt32(items == null || items[index] == null ? 0 : items[index].Amount);
                        packet.WriteByte(asda2Item == null ? 0 : (int) asda2Item.Durability);
                        packet.WriteByte(asda2Item == null ? 0 : (int) asda2Item.Enchant);
                        packet.WriteInt32(items == null || items[index] == null ? 0 : items[index].Price);
                        packet.WriteInt16(asda2Item == null ? -1 : asda2Item.Soul1Id);
                        packet.WriteInt16(asda2Item == null ? -1 : asda2Item.Soul2Id);
                        packet.WriteInt16(asda2Item == null ? -1 : asda2Item.Soul3Id);
                        packet.WriteInt16(asda2Item == null ? -1 : asda2Item.Soul4Id);
                        packet.WriteInt16(asda2Item == null ? -1 : (int) (short) asda2Item.Parametr1Type);
                        packet.WriteInt16(asda2Item == null ? -1 : (int) asda2Item.Parametr1Value);
                        packet.WriteInt16(asda2Item == null ? -1 : (int) (short) asda2Item.Parametr2Type);
                        packet.WriteInt16(asda2Item == null ? -1 : (int) asda2Item.Parametr2Value);
                        packet.WriteInt16(asda2Item == null ? -1 : (int) (short) asda2Item.Parametr3Type);
                        packet.WriteInt16(asda2Item == null ? -1 : (int) asda2Item.Parametr3Value);
                        packet.WriteInt16(asda2Item == null ? -1 : (int) (short) asda2Item.Parametr4Type);
                        packet.WriteInt16(asda2Item == null ? -1 : (int) asda2Item.Parametr4Value);
                        packet.WriteInt16(asda2Item == null ? -1 : (int) (short) asda2Item.Parametr5Type);
                        packet.WriteInt16(asda2Item == null ? -1 : (int) asda2Item.Parametr5Value);
                    }
                }

                client.Send(packet, false);
            }
        }

        public static void SendtradeStatusTextWindowResponse(Character chr)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.TradeStatusTextWindow))
            {
                packet.WriteByte(chr.IsAsda2TradeDescriptionEnabled ? 1 : 0);
                packet.WriteByte(chr.IsAsda2TradeDescriptionPremium ? 1 : 0);
                packet.WriteInt32(chr.AccId);
                packet.WriteFixedAsciiString(chr.Asda2TradeDescription, 50, Locale.Start);
                chr.SendPacketToArea(packet, true, true, Locale.Any, new float?());
            }
        }

        public static void SendtradeStatusTextWindowResponseToOne(Character chr, IRealmClient rcv)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.TradeStatusTextWindow))
            {
                packet.WriteByte(chr.IsAsda2TradeDescriptionEnabled ? 1 : 0);
                packet.WriteByte(chr.IsAsda2TradeDescriptionPremium ? 1 : 0);
                packet.WriteInt32(chr.AccId);
                packet.WriteFixedAsciiString(chr.Asda2TradeDescription, 50, Locale.Start);
                rcv.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.ViewCharacterTradeShop)]
        public static void ViewCharacterTradeShopRequest(IRealmClient client, RealmPacketIn packet)
        {
            RealmAccount loggedInAccount =
                ServerApp<WCell.RealmServer.RealmServer>.Instance.GetLoggedInAccount(packet.ReadUInt32());
            if (loggedInAccount == null || loggedInAccount.ActiveCharacter == null)
            {
                Asda2PrivateShopHandler.SendCharacterPrivateShopInfoResponse(client,
                    Asda2ViewTradeShopInfoStatus.TheCapacityHasExided, (Asda2PrivateShop) null);
            }
            else
            {
                Character activeCharacter = loggedInAccount.ActiveCharacter;
                if (activeCharacter.PrivateShop == null || !activeCharacter.PrivateShop.Trading)
                    return;
                activeCharacter.PrivateShop.Join(client.ActiveCharacter);
            }
        }

        public static void SendCharacterPrivateShopInfoResponse(IRealmClient client,
            Asda2ViewTradeShopInfoStatus infoStatus, Asda2PrivateShop shop)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.CharacterPrivateShopInfo))
            {
                packet.WriteByte((byte) infoStatus);
                if (infoStatus == Asda2ViewTradeShopInfoStatus.Ok)
                {
                    packet.WriteInt32(shop.Owner.AccId);
                    packet.WriteInt16(shop.Owner.SessionId);
                    packet.WriteByte(shop.ItemsCount);
                    packet.WriteFixedAsciiString(shop.Owner.Asda2TradeDescription, 50, Locale.Start);
                    packet.WriteFixedAsciiString(shop.Owner.Name, 20, Locale.Start);
                    for (int index = 0; index < 10; ++index)
                    {
                        Asda2Item asda2Item = shop.ItemsOnTrade[index] == null || shop.ItemsOnTrade[index].Item == null
                            ? (Asda2Item) null
                            : shop.ItemsOnTrade[index].Item;
                        packet.WriteInt32(asda2Item == null ? 0 : asda2Item.ItemId);
                        packet.WriteInt32(shop.ItemsOnTrade[index] == null ? -1 : shop.ItemsOnTrade[index].Amount);
                        packet.WriteByte(asda2Item == null ? 0 : (int) asda2Item.Durability);
                        packet.WriteInt16(asda2Item == null ? 0 : (int) asda2Item.Weight);
                        packet.WriteInt16(asda2Item == null ? 0 : asda2Item.Soul1Id);
                        packet.WriteInt16(asda2Item == null ? 0 : asda2Item.Soul2Id);
                        packet.WriteInt16(asda2Item == null ? 0 : asda2Item.Soul3Id);
                        packet.WriteInt16(asda2Item == null ? 0 : asda2Item.Soul4Id);
                        packet.WriteInt16(asda2Item == null ? 0 : (int) asda2Item.Enchant);
                        packet.WriteInt16(0);
                        packet.WriteByte(0);
                        packet.WriteInt16(asda2Item == null ? (short) 0 : (short) asda2Item.Parametr1Type);
                        packet.WriteInt16(asda2Item == null ? 0 : (int) asda2Item.Parametr1Value);
                        packet.WriteInt16(asda2Item == null ? (short) 0 : (short) asda2Item.Parametr2Type);
                        packet.WriteInt16(asda2Item == null ? 0 : (int) asda2Item.Parametr2Value);
                        packet.WriteInt16(asda2Item == null ? (short) 0 : (short) asda2Item.Parametr3Type);
                        packet.WriteInt16(asda2Item == null ? 0 : (int) asda2Item.Parametr3Value);
                        packet.WriteInt16(asda2Item == null ? (short) 0 : (short) asda2Item.Parametr4Type);
                        packet.WriteInt16(asda2Item == null ? 0 : (int) asda2Item.Parametr4Value);
                        packet.WriteInt16(asda2Item == null ? (short) 0 : (short) asda2Item.Parametr5Type);
                        packet.WriteInt16(asda2Item == null ? 0 : (int) asda2Item.Parametr5Value);
                        packet.WriteByte(0);
                        packet.WriteInt32(shop.ItemsOnTrade[index] == null ? -1 : shop.ItemsOnTrade[index].Price);
                        packet.WriteInt32(asda2Item == null ? 0 : 264156);
                        packet.WriteInt16(asda2Item == null ? 0 : 1);
                    }
                }

                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.PrivateShopChatReq)]
        public static void PrivateShopChatReqRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 22;
            string str = packet.ReadAsciiString(client.Locale);
            if (client.ActiveCharacter.PrivateShop == null)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to chat to private shop while not exist shop.", 2);
            }
            else
            {
                Locale locale = Asda2EncodingHelper.MinimumAvailableLocale(client.Locale, str);
                client.ActiveCharacter.PrivateShop.SendMessage(str, client.ActiveCharacter, locale);
            }
        }

        public static RealmPacketOut CreatePrivateShopChatResResponse(Character sender, string message, Locale locale)
        {
            RealmPacketOut realmPacketOut = new RealmPacketOut(RealmServerOpCode.PrivateShopChatRes);
            realmPacketOut.WriteInt32(1);
            realmPacketOut.WriteFixedAsciiString(sender.Name, 20, locale);
            realmPacketOut.WriteAsciiString(message, locale);
            realmPacketOut.WriteByte(0);
            return realmPacketOut;
        }

        [PacketHandler(RealmServerOpCode.BuyItemFromCharacterPrivateShop)]
        public static void BuyItemFromCharacterPrivateShopRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.PrivateShop == null)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to buy from private shop while it not exist", 0);
            }
            else
            {
                packet.Position += 7;
                List<Asda2ItemTradeRef> itemsToBuyRefs = new List<Asda2ItemTradeRef>();
                for (int index = 0; index < 6 && packet.ReadInt32() != 0; ++index)
                {
                    packet.Position += 3;
                    int num1 = packet.ReadInt32();
                    short num2 = packet.ReadInt16();
                    if (num1 < 0 || num2 < (short) 0 || num2 > (short) 9)
                    {
                        client.ActiveCharacter.YouAreFuckingCheater(
                            "Trying to buy items from private shop from wrong slot", 50);
                        return;
                    }

                    itemsToBuyRefs.Add(new Asda2ItemTradeRef()
                    {
                        Amount = num1 == 0 ? 1 : num1,
                        TradeSlot = (byte) num2
                    });
                    packet.Position += 32;
                }

                client.ActiveCharacter.PrivateShop.BuyItems(client.ActiveCharacter, itemsToBuyRefs);
            }
        }

        public static void SendItemBuyedFromPrivateShopResponse(Character chr, PrivateShopBuyResult status,
            List<Asda2Item> buyedItems)
        {
            Asda2Item[] asda2ItemArray = new Asda2Item[6];
            if (buyedItems != null)
            {
                for (int index = 0; index < buyedItems.Count; ++index)
                    asda2ItemArray[index] = buyedItems[index];
            }

            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ItemBuyedFromPrivateShop))
            {
                packet.WriteByte((byte) status);
                if (status == PrivateShopBuyResult.Ok)
                {
                    packet.WriteInt16(chr.Asda2Inventory.Weight);
                    packet.WriteInt32(chr.Money);
                    packet.WriteByte(buyedItems.Count<Asda2Item>((Func<Asda2Item, bool>) (i => i != null)));
                    for (int index = 0; index < 6; ++index)
                        Asda2InventoryHandler.WriteItemInfoToPacket(packet, asda2ItemArray[index], false);
                }

                chr.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.CloseCharacterTradeShop)]
        public static void CloseCharacterTradeShopRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.PrivateShop == null)
                Asda2PrivateShopHandler.SendCloseCharacterTradeShopToOwnerResponse(client,
                    Asda2PrivateShopClosedToOwnerResult.Ok);
            else
                client.ActiveCharacter.PrivateShop.Exit(client.ActiveCharacter);
        }

        public static void SendCloseCharacterTradeShopToOwnerResponse(IRealmClient client,
            Asda2PrivateShopClosedToOwnerResult status)
        {
            using (RealmPacketOut shopToOwnerResponse =
                Asda2PrivateShopHandler.CreateCloseCharacterTradeShopToOwnerResponse(status))
                client.Send(shopToOwnerResponse, false);
        }

        public static RealmPacketOut CreateCloseCharacterTradeShopToOwnerResponse(
            Asda2PrivateShopClosedToOwnerResult status)
        {
            RealmPacketOut realmPacketOut =
                new RealmPacketOut(RealmServerOpCode.CloseCharacterTradeShopResponseToOwner);
            realmPacketOut.WriteByte((byte) status);
            realmPacketOut.WriteSkip(Asda2PrivateShopHandler.stub1);
            return realmPacketOut;
        }

        public static RealmPacketOut CreatePrivateShopChatNotificationResponse(uint trigererAccId,
            Asda2PrivateShopNotificationType notificationType)
        {
            RealmPacketOut realmPacketOut = new RealmPacketOut(RealmServerOpCode.PrivateShopChatNotification);
            realmPacketOut.WriteByte((byte) notificationType);
            realmPacketOut.WriteInt32(trigererAccId);
            return realmPacketOut;
        }

        public static void SendItemBuyedFromPrivateShopToOwnerNotifyResponse(Asda2PrivateShop shop,
            List<Asda2ItemTradeRef> itemsBuyed, Character buyer)
        {
            Asda2ItemTradeRef[] asda2ItemTradeRefArray = new Asda2ItemTradeRef[6];
            for (int index = 0; index < itemsBuyed.Count; ++index)
                asda2ItemTradeRefArray[index] = itemsBuyed[index];
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ItemBuyedFromPrivateShopToOwnerNotify))
            {
                packet.WriteInt16(shop.Owner.Asda2Inventory.Weight);
                packet.WriteInt32(shop.Owner.Money);
                packet.WriteByte(
                    ((IEnumerable<Asda2ItemTradeRef>) asda2ItemTradeRefArray).Count<Asda2ItemTradeRef>(
                        (Func<Asda2ItemTradeRef, bool>) (i => i != null)));
                packet.WriteInt32(buyer.AccId);
                for (int index = 0; index < 6; ++index)
                {
                    Asda2Item asda2Item =
                        asda2ItemTradeRefArray[index] == null || asda2ItemTradeRefArray[index].Item == null
                            ? (Asda2Item) null
                            : asda2ItemTradeRefArray[index].Item;
                    packet.WriteInt32(asda2Item == null ? 0 : asda2Item.ItemId);
                    packet.WriteByte(asda2Item == null ? (byte) 0 : (byte) asda2Item.InventoryType);
                    packet.WriteInt16(asda2Item == null ? 0 : (int) asda2Item.Slot);
                    packet.WriteInt32(asda2ItemTradeRefArray[index] == null
                        ? -1
                        : asda2ItemTradeRefArray[index].Amount);
                    packet.WriteInt32(asda2ItemTradeRefArray[index] == null
                        ? -1
                        : (int) asda2ItemTradeRefArray[index].TradeSlot);
                    packet.WriteInt16(0);
                    packet.WriteSkip(Asda2PrivateShopHandler.stub17);
                }

                shop.Owner.Send(packet, false);
            }
        }

        public static void SendPrivateShopChatNotificationAboutBuyResponse(Asda2PrivateShop shop,
            List<Asda2ItemTradeRef> itemsBuyed, Character buyer)
        {
            Asda2ItemTradeRef[] asda2ItemTradeRefArray = new Asda2ItemTradeRef[6];
            for (int index = 0; index < itemsBuyed.Count; ++index)
                asda2ItemTradeRefArray[index] = itemsBuyed[index];
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.PrivateShopChatNotificationAboutBuy))
            {
                packet.WriteInt32(shop.Owner.AccId);
                packet.WriteInt16(shop.Owner.SessionId);
                packet.WriteByte(
                    ((IEnumerable<Asda2ItemTradeRef>) asda2ItemTradeRefArray).Count<Asda2ItemTradeRef>(
                        (Func<Asda2ItemTradeRef, bool>) (i => i != null)));
                packet.WriteInt32(buyer.AccId);
                for (int index = 0; index < 6; ++index)
                {
                    Asda2Item asda2Item =
                        asda2ItemTradeRefArray[index] == null || asda2ItemTradeRefArray[index].Item == null
                            ? (Asda2Item) null
                            : asda2ItemTradeRefArray[index].Item;
                    packet.WriteInt32(asda2Item == null ? 0 : asda2Item.ItemId);
                    packet.WriteByte(0);
                    packet.WriteInt16(-1);
                    packet.WriteInt32(asda2ItemTradeRefArray[index] == null
                        ? -1
                        : asda2ItemTradeRefArray[index].Amount);
                    packet.WriteInt32(asda2ItemTradeRefArray[index] == null
                        ? -1
                        : (int) asda2ItemTradeRefArray[index].TradeSlot);
                    packet.WriteInt16(0);
                    packet.WriteSkip(Asda2PrivateShopHandler.stub18);
                }

                shop.Send(packet);
            }
        }
    }
}
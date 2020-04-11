using System;
using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Items;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    internal class Asda2TradeHandler
    {
        [PacketHandler(RealmServerOpCode.TradeRequest)]
        public static void TradeRequestRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.Asda2TradeWindow != null)
            {
                if (client.ActiveCharacter.Asda2TradeWindow.Expired)
                {
                    client.ActiveCharacter.Asda2TradeWindow.CancelTrade();
                }
                else
                {
                    client.ActiveCharacter.SendSystemMessage("You already trading.");
                    Asda2TradeHandler.SendTradeRejectedResponse(client);
                    return;
                }
            }

            ushort sessId = packet.ReadUInt16();
            Asda2TradeType tradeType = (Asda2TradeType) packet.ReadByte();
            Character characterBySessionId = World.GetCharacterBySessionId(sessId);
            if (characterBySessionId == null)
            {
                client.ActiveCharacter.SendSystemMessage("Sorry, but i can't find character you want trade.");
                Asda2TradeHandler.SendTradeRejectedResponse(client);
            }
            else if (characterBySessionId.Map != client.ActiveCharacter.Map)
            {
                characterBySessionId.YouAreFuckingCheater("Trying to buy items from character with another map.", 50);
                Asda2TradeHandler.SendTradeRejectedResponse(client);
            }
            else if (!characterBySessionId.EnableGeneralTradeRequest && tradeType == Asda2TradeType.RedularTrade)
            {
                client.ActiveCharacter.SendSystemMessage(string.Format("{0} rejected all general trade requests.",
                    (object) characterBySessionId.Name));
                Asda2TradeHandler.SendTradeRejectedResponse(client);
            }
            else if (!characterBySessionId.EnableGearTradeRequest && tradeType == Asda2TradeType.ShopItemsTrade)
            {
                client.ActiveCharacter.SendSystemMessage(string.Format("{0} rejected all gear trade requests.",
                    (object) characterBySessionId.Name));
                Asda2TradeHandler.SendTradeRejectedResponse(client);
            }
            else if (client.ActiveCharacter.PrivateShop != null)
            {
                client.ActiveCharacter.YouAreFuckingCheater(
                    "Target character is in private shop and can't accept trade.", 20);
                Asda2TradeHandler.SendTradeRejectedResponse(client);
            }
            else if (client.ActiveCharacter.IsAsda2BattlegroundInProgress)
            {
                client.ActiveCharacter.SendInfoMsg("Can't trade on war.");
                Asda2TradeHandler.SendTradeRejectedResponse(client);
            }
            else if (characterBySessionId.PrivateShop != null)
            {
                Asda2TradeHandler.SendTradeRejectedResponse(client);
            }
            else
            {
                Asda2TradeWindow asda2TradeWindow = new Asda2TradeWindow()
                {
                    FisrtChar = client.ActiveCharacter,
                    SecondChar = characterBySessionId
                };
                asda2TradeWindow.Init();
                client.ActiveCharacter.Asda2TradeWindow = asda2TradeWindow;
                characterBySessionId.Asda2TradeWindow = asda2TradeWindow;
                asda2TradeWindow.TradeType = tradeType;
                Asda2TradeHandler.SendTradeRequestResponse(characterBySessionId.Client, client.ActiveCharacter,
                    tradeType);
            }
        }

        public static void SendTradeRequestResponse(IRealmClient client, Character sender, Asda2TradeType tradeType)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.TradeRequestResponse))
            {
                packet.WriteByte(1);
                packet.WriteInt16(sender.SessionId);
                packet.WriteFixedAsciiString(sender.Name, 20, Locale.Start);
                packet.WriteByte((byte) tradeType);
                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.StartTradeResponse)]
        public static void StartTradeResponseRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.Asda2TradeWindow == null)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to start trade response with wrong parametrs", 40);
            }
            else
            {
                packet.Position -= 4;
                if (packet.ReadByte() != (byte) 1)
                    client.ActiveCharacter.Asda2TradeWindow.CancelTrade();
                else
                    client.ActiveCharacter.Asda2TradeWindow.Accepted = true;
            }
        }

        public static void SendTradeStartedResponse(IRealmClient client, Asda2TradeStartedStatus status,
            Character tradeWith, bool isRegularTrade)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.TradeStarted))
            {
                packet.WriteByte((byte) status);
                packet.WriteByte(isRegularTrade ? 0 : 1);
                packet.WriteInt32(1);
                packet.WriteInt16(client.ActiveCharacter.SessionId);
                packet.WriteFixedAsciiString(client.ActiveCharacter.Name, 20, Locale.Start);
                packet.WriteInt16(tradeWith.SessionId);
                packet.WriteFixedAsciiString(tradeWith.Name, 20, Locale.Start);
                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.CancelTrade)]
        public static void CancelTradeRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.Asda2TradeWindow == null)
                return;
            client.ActiveCharacter.Asda2TradeWindow.CancelTrade();
        }

        public static void SendTradeRejectedResponse(IRealmClient client)
        {
            if (client == null)
                return;
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.TradeRejected))
                client.Send(packet, false);
        }

        [PacketHandler(RealmServerOpCode.PushItemToTrade)]
        public static void PushItemToTradeRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 5;
            byte invNum = packet.ReadByte();
            short cellNum = packet.ReadInt16();
            int quantity = packet.ReadInt32();
            if (client.ActiveCharacter.Asda2TradeWindow == null)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to push items to trade while not trading", 0);
            }
            else
            {
                Asda2ItemTradeRef asda2ItemTradeRef = (Asda2ItemTradeRef) null;
                Asda2PushItemToTradeStatus trade =
                    client.ActiveCharacter.Asda2TradeWindow.PushItemToTrade(client.ActiveCharacter, cellNum, quantity,
                        invNum, ref asda2ItemTradeRef);
                Asda2TradeHandler.SendItemToTradePushedResponse(client, trade, asda2ItemTradeRef);
            }
        }

        [PacketHandler(RealmServerOpCode.PopItemFromTrade)]
        public static void PopItemFromTradeRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 4;
            byte inv = packet.ReadByte();
            short cell = packet.ReadInt16();
            if (client.ActiveCharacter.Asda2TradeWindow == null)
                client.ActiveCharacter.YouAreFuckingCheater("Trying to poping items from trade while not trading", 0);
            else
                client.ActiveCharacter.Asda2TradeWindow.PopItem(client.ActiveCharacter, inv, cell);
        }

        public static void SendItemToTradePushedResponse(IRealmClient client, Asda2PushItemToTradeStatus status,
            Asda2ItemTradeRef item = null)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ItemToTradePushed))
            {
                packet.WriteByte((byte) status);
                packet.WriteByte(item == null || item.Item == null ? (byte) 0 : (byte) item.Item.InventoryType);
                packet.WriteInt16(item == null || item.Item == null ? 0 : (int) item.Item.Slot);
                packet.WriteInt32(item == null || item.Item == null ? 0 : item.Amount);
                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.ConfimTradeOne)]
        public static void ConfimTradeOneRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.Asda2TradeWindow == null)
                client.ActiveCharacter.YouAreFuckingCheater("Trying to confirm trade while not trading", 0);
            else
                client.ActiveCharacter.Asda2TradeWindow.ShowItemToOtherPlayer(client.ActiveCharacter);
        }

        public static void SendConfimTradeFromOponentResponse(IRealmClient client, Asda2ItemTradeRef[] items)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ConfimTradeFromOponent))
            {
                packet.WriteInt32(1);
                packet.WriteByte(1);
                packet.WriteByte(1);
                for (int index = 0; index < 5; ++index)
                {
                    Asda2ItemTradeRef asda2ItemTradeRef = items[index];
                    packet.WriteInt32(asda2ItemTradeRef == null ? 0 : asda2ItemTradeRef.Item.ItemId);
                    packet.WriteByte(asda2ItemTradeRef == null ? 0 : (int) asda2ItemTradeRef.Item.Durability);
                    packet.WriteInt16(asda2ItemTradeRef == null ? 0 : (int) asda2ItemTradeRef.Item.Weight);
                    packet.WriteInt32(asda2ItemTradeRef == null ? 0 : asda2ItemTradeRef.Amount);
                    packet.WriteByte(asda2ItemTradeRef == null ? 0 : (int) asda2ItemTradeRef.Item.Enchant);
                    packet.WriteInt64(0L);
                    packet.WriteInt16(0);
                    packet.WriteInt16(asda2ItemTradeRef == null
                        ? -1
                        : (int) (short) asda2ItemTradeRef.Item.Parametr1Type);
                    packet.WriteInt16(asda2ItemTradeRef == null ? -1 : (int) asda2ItemTradeRef.Item.Parametr1Value);
                    packet.WriteInt16(asda2ItemTradeRef == null
                        ? -1
                        : (int) (short) asda2ItemTradeRef.Item.Parametr2Type);
                    packet.WriteInt16(asda2ItemTradeRef == null ? -1 : (int) asda2ItemTradeRef.Item.Parametr2Value);
                    packet.WriteInt16(asda2ItemTradeRef == null
                        ? -1
                        : (int) (short) asda2ItemTradeRef.Item.Parametr3Type);
                    packet.WriteInt16(asda2ItemTradeRef == null ? -1 : (int) asda2ItemTradeRef.Item.Parametr3Value);
                    packet.WriteInt16(asda2ItemTradeRef == null
                        ? -1
                        : (int) (short) asda2ItemTradeRef.Item.Parametr4Type);
                    packet.WriteInt16(asda2ItemTradeRef == null ? -1 : (int) asda2ItemTradeRef.Item.Parametr4Value);
                    packet.WriteInt16(asda2ItemTradeRef == null
                        ? -1
                        : (int) (short) asda2ItemTradeRef.Item.Parametr5Type);
                    packet.WriteInt16(asda2ItemTradeRef == null ? -1 : (int) asda2ItemTradeRef.Item.Parametr5Value);
                    packet.WriteByte(0);
                    packet.WriteInt16(asda2ItemTradeRef == null ? 0 : asda2ItemTradeRef.Item.Soul1Id);
                    packet.WriteInt16(asda2ItemTradeRef == null ? 0 : asda2ItemTradeRef.Item.Soul2Id);
                    packet.WriteInt16(asda2ItemTradeRef == null ? 0 : asda2ItemTradeRef.Item.Soul3Id);
                    packet.WriteInt16(asda2ItemTradeRef == null ? 0 : asda2ItemTradeRef.Item.Soul4Id);
                }

                client.Send(packet, false);
            }
        }

        [PacketHandler(RealmServerOpCode.ConfimTradeTwo)]
        public static void ConfimTradeTwoRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.Asda2TradeWindow == null)
                client.ActiveCharacter.YouAreFuckingCheater("Trying to confirm trade two while not trading", 0);
            else
                client.ActiveCharacter.AddMessage((Action) (() =>
                    client.ActiveCharacter.Asda2TradeWindow.ConfirmTrade(client.ActiveCharacter)));
        }

        public static void SendRegularTradeCompleteResponse(IRealmClient client, Asda2Item[] items)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.RegularTradeComplete))
            {
                packet.WriteByte(5);
                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);
                for (int index = 0; index < 5; ++index)
                {
                    Asda2Item asda2Item = items[index];
                    if (asda2Item == null)
                    {
                        packet.WriteInt32(-1);
                        packet.WriteByte(0);
                        packet.WriteInt16(-1);
                        packet.WriteInt32(0);
                        packet.WriteInt16(0);
                    }
                    else
                    {
                        packet.WriteInt32(asda2Item.ItemId);
                        packet.WriteByte((byte) asda2Item.InventoryType);
                        packet.WriteInt16(asda2Item.Slot);
                        packet.WriteInt32(asda2Item.Amount);
                        packet.WriteInt16(asda2Item.Weight);
                    }
                }

                client.Send(packet, false);
            }
        }

        public static void SendShopTradeCompleteResponse(IRealmClient client, Asda2Item[] items)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.ShopTradeComplete))
            {
                packet.WriteByte(5);
                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight);
                for (int index = 0; index < 5; ++index)
                {
                    Asda2Item asda2Item = items[index];
                    packet.WriteInt32(asda2Item == null ? 0 : asda2Item.ItemId);
                    packet.WriteByte(asda2Item == null ? (byte) 0 : (byte) asda2Item.InventoryType);
                    packet.WriteInt16(asda2Item == null ? -1 : (int) asda2Item.Slot);
                    packet.WriteInt16(asda2Item == null ? -1 : (asda2Item.IsDeleted ? -1 : 0));
                    packet.WriteByte(asda2Item == null ? -1 : (int) asda2Item.Durability);
                    packet.WriteInt16(asda2Item == null ? -1 : (int) asda2Item.Weight);
                    packet.WriteInt32(0);
                    packet.WriteByte(asda2Item == null ? -1 : (int) asda2Item.Enchant);
                    packet.WriteByte(0);
                    packet.WriteByte(0);
                    packet.WriteByte(0);
                    packet.WriteByte(asda2Item == null ? -1 : (int) asda2Item.SealCount);
                    packet.WriteInt16(asda2Item == null ? -1 : (int) (short) asda2Item.Parametr1Type);
                    packet.WriteInt16(asda2Item == null ? -1 : (int) asda2Item.Parametr1Value);
                    packet.WriteInt16(asda2Item == null ? -1 : (int) (short) asda2Item.Parametr2Type);
                    packet.WriteInt16(asda2Item == null ? -1 : (int) asda2Item.Parametr2Value);
                    packet.WriteInt16(asda2Item == null ? -1 : (int) (short) asda2Item.Parametr3Type);
                    packet.WriteInt16(asda2Item == null ? -1 : (int) asda2Item.Parametr3Value);
                    packet.WriteInt16(asda2Item == null ? -1 : (int) (short) asda2Item.Parametr4Type);
                    if (asda2Item != null && asda2Item.Template.IsAvatar)
                    {
                        Asda2ItemTemplate template1 = Asda2ItemMgr.GetTemplate(asda2Item.Soul2Id);
                        Asda2ItemTemplate template2 = Asda2ItemMgr.GetTemplate(asda2Item.Soul3Id);
                        packet.WriteInt16(template1 == null ? -1 : template1.SowelBonusValue);
                        packet.WriteInt16(-1);
                        packet.WriteInt16(template2 == null ? -1 : template2.SowelBonusValue);
                    }
                    else
                    {
                        packet.WriteInt16(asda2Item == null ? -1 : (int) asda2Item.Parametr4Value);
                        packet.WriteInt16(asda2Item == null ? -1 : (int) (short) asda2Item.Parametr5Type);
                        packet.WriteInt16(asda2Item == null ? -1 : (int) asda2Item.Parametr5Value);
                    }

                    packet.WriteByte(0);
                    packet.WriteItemAmount(asda2Item, false);
                    packet.WriteInt16(asda2Item == null ? -1 : asda2Item.Soul1Id);
                    packet.WriteInt16(asda2Item == null ? -1 : asda2Item.Soul2Id);
                    packet.WriteInt16(asda2Item == null ? -1 : asda2Item.Soul3Id);
                    packet.WriteInt16(asda2Item == null ? -1 : asda2Item.Soul4Id);
                }

                client.Send(packet, false);
            }
        }
    }
}
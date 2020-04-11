using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    internal class Asda2TradeHandler
    {
        [PacketHandler(RealmServerOpCode.TradeRequest)] //5282
        public static void TradeRequestRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.Asda2TradeWindow != null)
            {
                if (client.ActiveCharacter.Asda2TradeWindow.Expired)
                    client.ActiveCharacter.Asda2TradeWindow.CancelTrade();
                else
                {
                    client.ActiveCharacter.SendSystemMessage("You already trading.");
                    SendTradeRejectedResponse(client);
                    return;
                }
                /*if (RealmServer.IsPreparingShutdown || RealmServer.IsShuttingDown)
                    client.acti*/
            }
            var characterSessionNum = packet.ReadUInt16(); //default : 0Len : 2
            var typeOfTrade = (Asda2TradeType)packet.ReadByte(); //default : 0Len : 1
            var targetChr = World.GetCharacterBySessionId(characterSessionNum);
            if (targetChr == null)
            {
                client.ActiveCharacter.SendSystemMessage("Sorry, but i can't find character you want trade.");
                SendTradeRejectedResponse(client);
                return;
            }
            if (targetChr.Map != client.ActiveCharacter.Map)
            {
                targetChr.YouAreFuckingCheater("Trying to buy items from character with another map.", 50);
                SendTradeRejectedResponse(client);
                return;
            }
            if (!targetChr.EnableGeneralTradeRequest && typeOfTrade == Asda2TradeType.RedularTrade)
            {
                client.ActiveCharacter.SendSystemMessage(string.Format("{0} rejected all general trade requests.",
                                                                       targetChr.Name));
                SendTradeRejectedResponse(client);
                return;
            }
            if (!targetChr.EnableGearTradeRequest && typeOfTrade == Asda2TradeType.ShopItemsTrade)
            {
                client.ActiveCharacter.SendSystemMessage(string.Format("{0} rejected all gear trade requests.",
                                                                       targetChr.Name));
                SendTradeRejectedResponse(client);
                return;
            }
            if (client.ActiveCharacter.PrivateShop != null)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Target character is in private shop and can't accept trade.", 20);
                SendTradeRejectedResponse(client);
                return;
            }
            if (client.ActiveCharacter.IsAsda2BattlegroundInProgress)
            {
                client.ActiveCharacter.SendInfoMsg("Can't trade on war.");
                SendTradeRejectedResponse(client);
                return;
            }
            if (RealmServer.IsPreparingShutdown || RealmServer.IsShuttingDown)
            {
                SendTradeRejectedResponse(client);
                return;
            }
            if (targetChr.PrivateShop != null)
            {
                SendTradeRejectedResponse(client);
                return;
            }
            var tradeWindow = new Asda2TradeWindow { FisrtChar = client.ActiveCharacter, SecondChar = targetChr };
            tradeWindow.Init();
            client.ActiveCharacter.Asda2TradeWindow = tradeWindow;
            targetChr.Asda2TradeWindow = tradeWindow;
            tradeWindow.TradeType = typeOfTrade;
            SendTradeRequestResponse(targetChr.Client, client.ActiveCharacter, typeOfTrade);
        }

        public static void SendTradeRequestResponse(IRealmClient client, Character sender, Asda2TradeType tradeType)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.TradeRequestResponse)) //5283
            {
                packet.WriteByte(1); //value name : unk4 default value : 1Len : 1
                packet.WriteInt16(sender.SessionId); //{sessId}default value : 3 Len : 2
                packet.WriteFixedAsciiString(sender.Name, 20); //{fromName}default value :  Len : 20
                packet.WriteByte((byte)tradeType); //{tradeType}default value : 0 Len : 1
                client.Send(packet, addEnd: false);
            }
        }

        [PacketHandler(RealmServerOpCode.StartTradeResponse)] //5284
        public static void StartTradeResponseRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.Asda2TradeWindow == null)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to start trade response with wrong parametrs", 40);
                return;
            }
            packet.Position -= 4;
            var status = packet.ReadByte(); //default : 0Len : 1
            var accepted = status == 1;
            if (!accepted)
            {
                client.ActiveCharacter.Asda2TradeWindow.CancelTrade();
                return;
            }
            client.ActiveCharacter.Asda2TradeWindow.Accepted = true;
        }

        public static void SendTradeStartedResponse(IRealmClient client, Asda2TradeStartedStatus status,
                                                    Character tradeWith, bool isRegularTrade)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.TradeStarted)) //5285
            {
                packet.WriteByte((byte)status);//{status}default value : 1 Len : 1
                packet.WriteByte(isRegularTrade ? 0 : 1);//{tradeType}default value : 1 Len : 1
                packet.WriteInt32(1);//{tradeSessionID}default value : 1 Len : 4
                packet.WriteInt16(client.ActiveCharacter.SessionId);//value name : unk7 default value : 23Len : 2
                packet.WriteFixedAsciiString(client.ActiveCharacter.Name, 20);//{characterName}default value :  Len : 20
                packet.WriteInt16(tradeWith.SessionId);//{secondSessId}default value : 34 Len : 2
                packet.WriteFixedAsciiString(tradeWith.Name, 20);//{secondName}default value :  Len : 20
                client.Send(packet);
            }
        }

        [PacketHandler(RealmServerOpCode.CancelTrade)] //5286
        public static void CancelTradeRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.Asda2TradeWindow != null)
                client.ActiveCharacter.Asda2TradeWindow.CancelTrade();
        }

        public static void SendTradeRejectedResponse(IRealmClient client)
        {
            if (client == null) return;
            using (var packet = new RealmPacketOut(RealmServerOpCode.TradeRejected)) //5287
            {
                client.Send(packet, addEnd: false);
            }
        }

        [PacketHandler(RealmServerOpCode.PushItemToTrade)] //5288
        public static void PushItemToTradeRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 5; //nk4 default : 1Len : 4
            var invNum = packet.ReadByte(); //default : 2Len : 1
            var cellNum = packet.ReadInt16(); //default : 5Len : 2
            var quantity = packet.ReadInt32(); //default : 500Len : 4
            if (client.ActiveCharacter.Asda2TradeWindow == null)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to push items to trade while not trading", 0);
                return;
            }
            Asda2ItemTradeRef item = null;
            var r = client.ActiveCharacter.Asda2TradeWindow.PushItemToTrade(client.ActiveCharacter, cellNum, quantity, invNum, ref item);
            SendItemToTradePushedResponse(client, r, item);
        }

        [PacketHandler(RealmServerOpCode.PopItemFromTrade)]//5290
        public static void PopItemFromTradeRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.Position += 4;//nk7 default : 1Len : 4
            var inv = packet.ReadByte();//default : 2Len : 1
            var cell = packet.ReadInt16();//default : 2Len : 2
            // var amount = packet.ReadInt32();//default : 5Len : 4
            if (client.ActiveCharacter.Asda2TradeWindow == null)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to poping items from trade while not trading", 0);
                return;
            }
            client.ActiveCharacter.Asda2TradeWindow.PopItem(client.ActiveCharacter, inv, cell);
        }

        public static void SendItemToTradePushedResponse(IRealmClient client, Asda2PushItemToTradeStatus status, Asda2ItemTradeRef item = null)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ItemToTradePushed)) //5289
            {
                packet.WriteByte((byte)status); //{status}default value : 1 Len : 1
                packet.WriteByte((byte)(item == null || item.Item == null ? 0 : item.Item.InventoryType)); //{inv}default value : 2 Len : 1
                packet.WriteInt16(item == null || item.Item == null ? 0 : item.Item.Slot); //{cell}default value : 2 Len : 2
                packet.WriteInt32(item == null || item.Item == null ? 0 : item.Amount); //{quanity}default value : 5 Len : 4
                client.Send(packet, addEnd: false);
            }
        }

        [PacketHandler(RealmServerOpCode.ConfimTradeOne)] //5291
        public static void ConfimTradeOneRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.Asda2TradeWindow == null)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to confirm trade while not trading", 0);
                return;
            }
            client.ActiveCharacter.Asda2TradeWindow.ShowItemToOtherPlayer(client.ActiveCharacter);
        }

        public static void SendConfimTradeFromOponentResponse(IRealmClient client, Asda2ItemTradeRef[] items)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ConfimTradeFromOponent)) //5292
            {
                packet.WriteInt32(1); //{itemsCount}default value : 1 Len : 4
                packet.WriteByte(1); //value name : unk1 default value : 1Len : 1
                packet.WriteByte(1); //value name : unk1 default value : 1Len : 1
                for (int i = 0; i < 5; i += 1)
                {
                    var item = items[i];
                    packet.WriteInt32(item == null ? 0 : item.Item.ItemId); //{itemId%}default value : 0 Len : 4
                    packet.WriteByte(item == null ? 0 : item.Item.Durability); //{durability%}default value : 0 Len : 1
                    packet.WriteInt16(item == null ? 0 : item.Item.Weight); //{weight%}default value : 0 Len : 2
                    packet.WriteInt32(item == null ? 0 : item.Amount); //{quantity}default value : 0 Len : 4
                    packet.WriteByte(item == null ? 0 : item.Item.Enchant); //{enchant%}default value : 0 Len : 1
                    packet.WriteInt64(0); //value name : unk8 default value : 0Len : 8
                    packet.WriteInt16(0); //value name : unk2 default value : 0Len : 2
                    packet.WriteInt16(item == null ? -1 : (short)item.Item.Parametr1Type); //{parametr1Type%}default value : -1 Len : 2
                    packet.WriteInt16(item == null ? -1 : item.Item.Parametr1Value); //{paramtetr1Value%}default value : -1 Len : 2
                    packet.WriteInt16(item == null ? -1 : (short)item.Item.Parametr2Type); //{parametr1Type%}default value : -1 Len : 2
                    packet.WriteInt16(item == null ? -1 : item.Item.Parametr2Value); //{paramtetr1Value%}default value : -1 Len : 2
                    packet.WriteInt16(item == null ? -1 : (short)item.Item.Parametr3Type); //{parametr1Type%}default value : -1 Len : 2
                    packet.WriteInt16(item == null ? -1 : item.Item.Parametr3Value); //{paramtetr1Value%}default value : -1 Len : 2
                    packet.WriteInt16(item == null ? -1 : (short)item.Item.Parametr4Type); //{parametr1Type%}default value : -1 Len : 2
                    packet.WriteInt16(item == null ? -1 : item.Item.Parametr4Value); //{paramtetr1Value%}default value : -1 Len : 2
                    packet.WriteInt16(item == null ? -1 : (short)item.Item.Parametr5Type); //{parametr1Type%}default value : -1 Len : 2
                    packet.WriteInt16(item == null ? -1 : item.Item.Parametr5Value); //{paramtetr1Value%}default value : -1 Len : 2
                    packet.WriteByte(0); //value name : unk1 default value : 0Len : 1
                    packet.WriteInt16(item == null ? 0 : item.Item.Soul1Id); //{soul1Id%}default value : -1 Len : 2
                    packet.WriteInt16(item == null ? 0 : item.Item.Soul2Id); //{soul1Id%}default value : -1 Len : 2
                    packet.WriteInt16(item == null ? 0 : item.Item.Soul3Id); //{soul1Id%}default value : -1 Len : 2
                    packet.WriteInt16(item == null ? 0 : item.Item.Soul4Id); //{soul1Id%}default value : -1 Len : 2

                }
                client.Send(packet, addEnd: false);
            }
        }

        [PacketHandler(RealmServerOpCode.ConfimTradeTwo)] //5293
        public static void ConfimTradeTwoRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.Asda2TradeWindow == null)
            {
                client.ActiveCharacter.YouAreFuckingCheater("Trying to confirm trade two while not trading", 0);
                return;
            }
            client.ActiveCharacter.AddMessage(() => client.ActiveCharacter.Asda2TradeWindow.ConfirmTrade(client.ActiveCharacter));
        }

        public static void SendRegularTradeCompleteResponse(IRealmClient client, Asda2Item[] items)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.RegularTradeComplete)) //5295
            {
                packet.WriteByte(5); //{status}default value : 5 Len : 1
                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight); //{invWeight}default value : 2381 Len : 4
                for (int i = 0; i < 5; i += 1)
                {
                    var item = items[i];
                    if (item == null)
                    {
                        packet.WriteInt32(-1); //{itemId}default value : 31850 Len : 4
                        packet.WriteByte(0); //{invNum}default value : 2 Len : 1
                        packet.WriteInt16(-1); //{slot}default value : 1 Len : 2
                        packet.WriteInt32(0); //{amount}default value : 5 Len : 4
                        packet.WriteInt16(0); //{weight}default value : 60 Len : 2
                    }
                    else
                    {
                        packet.WriteInt32(item.ItemId); //{itemId}default value : 31850 Len : 4
                        packet.WriteByte((byte)item.InventoryType); //{invNum}default value : 2 Len : 1
                        packet.WriteInt16(item.Slot); //{slot}default value : 1 Len : 2
                        packet.WriteInt32(item.Amount); //{amount}default value : 5 Len : 4
                        packet.WriteInt16(item.Weight); //{weight}default value : 60 Len : 2
                    }
                }
                client.Send(packet, addEnd: false);
            }
        }

        public static void SendShopTradeCompleteResponse(IRealmClient client, Asda2Item[] items)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.ShopTradeComplete)) //5294
            {
                packet.WriteByte(5); //{status}default value : 5 Len : 1
                packet.WriteInt32(client.ActiveCharacter.Asda2Inventory.Weight); //{invWeight}default value : 1394 Len : 4
                for (int i = 0; i < 5; i++)
                {
                    var item = items[i];

                    packet.WriteInt32(item == null ? 0 : item.ItemId); //{itemID}default value : 28516 Len : 4
                    packet.WriteByte((byte)(item == null ? 0 : item.InventoryType)); //{bagNum}default value : 1 Len : 1
                    packet.WriteInt16(item == null ? -1 : item.Slot); //{cellNum}default value : 0 Len : 4
                    packet.WriteInt16(item == null ? -1 : item.IsDeleted ? -1 : 0);
                    packet.WriteByte(item == null ? -1 : item.Durability); //{durability}default value : 100 Len : 1
                    packet.WriteInt16(item == null ? -1 : item.Weight); //{weight}default value : 677 Len : 2
                    packet.WriteInt32(0);
                    packet.WriteByte(item == null ? -1 : item.Enchant); //{enchant}default value : 0 Len : 1
                    packet.WriteByte(0); //value name : stab31 default value : stab31Len : 3
                    packet.WriteByte(0);
                    packet.WriteByte(0);
                    packet.WriteByte(item == null ? -1 : item.SealCount); //{sealCount}default value : 0 Len : 1
                    packet.WriteInt16(item == null ? -1 : (short)item.Parametr1Type);
                    packet.WriteInt16(item == null ? -1 : item.Parametr1Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item == null ? -1 : (short)item.Parametr2Type);
                    packet.WriteInt16(item == null ? -1 : item.Parametr2Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item == null ? -1 : (short)item.Parametr3Type); //{stat1Type}default value : 1 Len : 2
                    packet.WriteInt16(item == null ? -1 : item.Parametr3Value); //{stat1Value}default value : 9 Len : 2
                    packet.WriteInt16(item == null ? -1 : (short)item.Parametr4Type); //{stat1Type}default value : 1 Len : 2
                    if (item != null && item.Template.IsAvatar)
                    {
                        var t1 = Items.Asda2ItemMgr.GetTemplate(item.Soul2Id);
                        var t2 = Items.Asda2ItemMgr.GetTemplate(item.Soul3Id);
                        packet.WriteInt16(t1 == null ? -1 : t1.SowelBonusValue); //{stat1Value}default value : 9 Len : 2
                        packet.WriteInt16(-1); //{stat1Type}default value : 1 Len : 2
                        packet.WriteInt16(t2 == null ? -1 : t2.SowelBonusValue);
                    }
                    else
                    {
                        packet.WriteInt16(item == null ? -1 : item.Parametr4Value); //{stat1Value}default value : 9 Len : 2
                        packet.WriteInt16(item == null ? -1 : (short)item.Parametr5Type); //{stat1Type}default value : 1 Len : 2
                        packet.WriteInt16(item == null ? -1 : item.Parametr5Value); //{stat1Value}default value : 9 Len : 2
                    }
                    packet.WriteByte(0); //value name : unk15 default value : 0Len : 1
                    packet.WriteItemAmount(item);
                    packet.WriteInt16(item == null ? -1 : item.Soul1Id); //{soul1Id}default value : 7576 Len : 2
                    packet.WriteInt16(item == null ? -1 : item.Soul2Id); //{soul2Id}default value : -1 Len : 2
                    packet.WriteInt16(item == null ? -1 : item.Soul3Id); //{soul3Id}default value : -1 Len : 2
                    packet.WriteInt16(item == null ? -1 : item.Soul4Id); //{soul4Id}default value : -1 Len : 2
                }
                client.Send(packet, addEnd: false);
            }
        }
    }
}
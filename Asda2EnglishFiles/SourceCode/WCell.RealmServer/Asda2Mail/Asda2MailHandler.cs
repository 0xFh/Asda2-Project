using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Asda2_Items;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Logs;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Asda2Mail
{
    public static class Asda2MailHandler
    {
        [PacketHandler(RealmServerOpCode.OpenMailBox)]
        public static void OpenMailBoxRequest(IRealmClient client, RealmPacketIn packet)
        {
            int num = (client.ActiveCharacter.MailMessages.Count + 9) / 10;
            for (int index = 0; index < num; ++index)
            {
                List<Asda2MailMessage> list = client.ActiveCharacter.MailMessages.Values
                    .Skip<Asda2MailMessage>(index * 10).Take<Asda2MailMessage>(10).ToList<Asda2MailMessage>();
                Asda2MailHandler.SendMailsListResponse(client, (IEnumerable<Asda2MailMessage>) list);
            }
        }

        public static void SendYouHaveNewMailResponse(IRealmClient client, int amount)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.YouHaveNewMail))
            {
                packet.WriteInt32(amount);
                client.Send(packet, true);
            }
        }

        public static void SendMailsListResponse(IRealmClient client, IEnumerable<Asda2MailMessage> mailMsgs)
        {
            Asda2MailMessage[] asda2MailMessageArray = new Asda2MailMessage[10];
            int index1 = 0;
            foreach (Asda2MailMessage mailMsg in mailMsgs)
            {
                if (mailMsg.DeleteTime < DateTime.Now)
                {
                    client.ActiveCharacter.MailMessages.Remove(mailMsg.Guid);
                    mailMsg.DeleteLater();
                }
                else
                    asda2MailMessageArray[index1] = mailMsg;

                ++index1;
            }

            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MailsList))
            {
                packet.WriteByte(0);
                for (int index2 = 0; index2 < 10; ++index2)
                {
                    Asda2MailMessage asda2MailMessage = asda2MailMessageArray[index2];
                    packet.WriteInt32(asda2MailMessage == null ? -1 : (int) asda2MailMessage.Guid);
                    packet.WriteByte(asda2MailMessage == null ? 0 : (asda2MailMessage.IsReaded ? 1 : 0));
                    packet.WriteInt32(asda2MailMessage == null
                        ? 0
                        : (int) (asda2MailMessage.DeleteTime - DateTime.Now).TotalMinutes);
                    packet.WriteFixedAsciiString(asda2MailMessage == null ? "" : asda2MailMessage.SenderName, 20,
                        Locale.Start);
                    packet.WriteFixedAsciiString(asda2MailMessage == null ? "" : asda2MailMessage.Subject, 32,
                        Locale.Start);
                }

                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.SendMailMessage)]
        public static void SendMailMessageRequest(IRealmClient client, RealmPacketIn packet)
        {
            packet.ReadInt32();
            short slotInq = packet.ReadInt16();
            Asda2InventoryType asda2InventoryType = (Asda2InventoryType) packet.ReadByte();
            ushort num = packet.ReadUInt16();
            uint sendedGold = packet.ReadUInt32();
            string str = packet.ReadAsdaString(20, Locale.Start);
            string subject = packet.ReadAsdaString(32, Locale.Start);
            string message = packet.ReadAsdaString(126, Locale.Start);
            if (!Asda2EncodingHelper.IsPrueEnglish(message))
            {
                client.ActiveCharacter.SendOnlyEnglishCharactersAllowed("message");
                Asda2MailHandler.SendMailMessageSendedResponse(client, MailMessageSendedStatus.WrongInformation,
                    (Asda2Item) null);
            }
            else if (!Asda2EncodingHelper.IsPrueEnglish(subject))
            {
                client.ActiveCharacter.SendOnlyEnglishCharactersAllowed("subject");
                Asda2MailHandler.SendMailMessageSendedResponse(client, MailMessageSendedStatus.WrongInformation,
                    (Asda2Item) null);
            }
            else
            {
                Asda2Item asda2Item = asda2InventoryType == Asda2InventoryType.Shop
                    ? client.ActiveCharacter.Asda2Inventory.GetShopShopItem(slotInq)
                    : client.ActiveCharacter.Asda2Inventory.GetRegularItem(slotInq);
                if (client.ActiveCharacter.Money < 1001U || client.ActiveCharacter.Money - 1000U < sendedGold)
                {
                    Asda2MailHandler.SendMailMessageSendedResponse(client, MailMessageSendedStatus.IncifitientGold,
                        (Asda2Item) null);
                }
                else
                {
                    Character chr = World.GetCharacter(str, false);
                    CharacterRecord chrRec = chr == null ? CharacterRecord.GetRecordByName(str) : chr.Record;
                    if (chrRec == null)
                    {
                        Asda2MailHandler.SendMailMessageSendedResponse(client,
                            MailMessageSendedStatus.RecipentNotFounded, (Asda2Item) null);
                    }
                    else
                    {
                        Asda2Item sendItem = (Asda2Item) null;
                        if (asda2Item != null)
                        {
                            if (asda2Item.IsSoulbound)
                            {
                                Asda2MailHandler.SendMailMessageSendedResponse(client,
                                    MailMessageSendedStatus.YouCantSendThisItem, (Asda2Item) null);
                                return;
                            }

                            if (asda2Item.Amount < (int) num)
                            {
                                client.ActiveCharacter.YouAreFuckingCheater("Tying to mail wrong item amount", 50);
                                Asda2MailHandler.SendMailMessageSendedResponse(client,
                                    MailMessageSendedStatus.WrongInformation, (Asda2Item) null);
                                return;
                            }

                            asda2Item.ModAmount(-(num == (ushort) 0 ? 1 : (int) num));
                            sendItem = Asda2Item.CreateItem(asda2Item.Template, (Character) null,
                                num == (ushort) 0 ? 1 : (int) num);
                            sendItem.Record.SaveLater();
                        }

                        client.ActiveCharacter.SubtractMoney(sendedGold + 1000U);
                        Log.Create(Log.Types.ItemOperations, LogSourceType.Character, client.ActiveCharacter.EntryId)
                            .AddAttribute("source", 0.0, "send_mail").AddItemAttributes(sendItem, "sent")
                            .AddItemAttributes(asda2Item, "source").AddAttribute("gold", (double) sendedGold, "")
                            .AddAttribute("receiver", (double) chrRec.EntityLowId, str).Write();
                        ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
                        {
                            Asda2MailMessage record = new Asda2MailMessage(subject, message, sendItem, sendedGold,
                                chrRec.EntityLowId, client.ActiveCharacter.Name);
                            record.CreateLater();
                            if (chr == null)
                                return;
                            chr.MailMessages.Add(record.Guid, record);
                            chr.SendMailMsg(string.Format("You recieve new message from {0}. Subject {1}.",
                                (object) client.ActiveCharacter.Name, (object) record.Subject));
                            Asda2MailHandler.SendYouHaveNewMailResponse(chr.Client, 1);
                        }));
                        Asda2MailHandler.SendMailMessageSendedResponse(client, MailMessageSendedStatus.Ok, asda2Item);
                    }
                }
            }
        }

        public static void SendMailMessageSendedResponse(IRealmClient client, MailMessageSendedStatus status,
            Asda2Item item = null)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MailMessageSended))
            {
                packet.WriteByte((byte) status);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, item, true);
                packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                packet.WriteInt32(client.ActiveCharacter.Money);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.ShowMailMsg)]
        public static void ShowMailMsgRequest(IRealmClient client, RealmPacketIn packet)
        {
            int num = packet.ReadInt32();
            if (!client.ActiveCharacter.MailMessages.ContainsKey((long) num))
            {
                client.ActiveCharacter.YouAreFuckingCheater("Try to view not existing mail message.", 30);
                Asda2MailHandler.SendMailMessageInfoResponse(client, ShowMailMessageStatus.Fail,
                    (Asda2MailMessage) null);
            }
            else
            {
                Asda2MailMessage mailMessage = client.ActiveCharacter.MailMessages[(long) num];
                if (mailMessage != null)
                    mailMessage.IsReaded = true;
                Asda2MailHandler.SendMailMessageInfoResponse(client, ShowMailMessageStatus.Ok, mailMessage);
            }
        }

        public static void SendMailMessageInfoResponse(IRealmClient client, ShowMailMessageStatus status,
            Asda2MailMessage msg)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MailMessageInfo))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt32(msg == null ? 0 : (int) msg.Guid);
                packet.WriteInt32(msg == null ? 0U : msg.Gold);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, msg == null ? (Asda2Item) null : msg.Item, true);
                packet.WriteFixedAsciiString(msg == null ? "" : msg.Body, 128, Locale.Start);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.TakeMailItem)]
        public static void TakeMailItemRequest(IRealmClient client, RealmPacketIn packet)
        {
            int num1 = packet.ReadInt32();
            if (!client.ActiveCharacter.MailMessages.ContainsKey((long) num1))
            {
                client.ActiveCharacter.YouAreFuckingCheater("Try to take not existing mail message.", 50);
                Asda2MailHandler.SendMailItemTakedResponse(client, Asda2MailItemTakedStatus.WrongInfo,
                    (Asda2Item) null);
            }
            else
            {
                Asda2MailMessage mailMessage = client.ActiveCharacter.MailMessages[(long) num1];
                if (mailMessage == null)
                {
                    Asda2MailHandler.SendMailItemTakedResponse(client, Asda2MailItemTakedStatus.WrongInfo,
                        (Asda2Item) null);
                }
                else
                {
                    Asda2Item itemToCopyStats = mailMessage.Item;
                    Asda2Item asda2Item = (Asda2Item) null;
                    if (itemToCopyStats != null)
                    {
                        int num2 = (int) client.ActiveCharacter.Asda2Inventory.TryAdd(itemToCopyStats.ItemId,
                            itemToCopyStats.Amount, true, ref asda2Item, new Asda2InventoryType?(), itemToCopyStats);
                        Log.Create(Log.Types.ItemOperations, LogSourceType.Character, client.ActiveCharacter.EntryId)
                            .AddAttribute("source", 0.0, "taked_from_mail")
                            .AddAttribute("message_id", (double) mailMessage.Guid, "").AddItemAttributes(asda2Item, "")
                            .Write();
                        mailMessage.ItemGuid = 0L;
                        mailMessage.Item = (Asda2Item) null;
                        itemToCopyStats.Destroy();
                    }

                    client.ActiveCharacter.AddMoney(mailMessage.Gold);
                    mailMessage.Gold = 0U;
                    mailMessage.UpdateLater();
                    Asda2MailHandler.SendMailItemTakedResponse(client, Asda2MailItemTakedStatus.Ok, asda2Item);
                    client.ActiveCharacter.SendMoneyUpdate();
                }
            }
        }

        public static void SendMailItemTakedResponse(IRealmClient client, Asda2MailItemTakedStatus status,
            Asda2Item item)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MailItemTaked))
            {
                packet.WriteByte((byte) status);
                packet.WriteInt16(client.ActiveCharacter.Asda2Inventory.Weight);
                packet.WriteInt32(client.ActiveCharacter.Money);
                Asda2InventoryHandler.WriteItemInfoToPacket(packet, item, false);
                client.Send(packet, true);
            }
        }

        [PacketHandler(RealmServerOpCode.DeleteMailMessage)]
        public static void DeleteMailMessageRequest(IRealmClient client, RealmPacketIn packet)
        {
            int msgGuid = packet.ReadInt32();
            if (client.ActiveCharacter.MailMessages.ContainsKey((long) msgGuid))
            {
                Asda2MailMessage mailMessage = client.ActiveCharacter.MailMessages[(long) msgGuid];
                if (mailMessage != null)
                {
                    client.ActiveCharacter.MailMessages.Remove((long) msgGuid);
                    mailMessage.DeleteLater();
                }
            }

            Asda2MailHandler.SendMailMessageDeletedResponse(client, msgGuid);
        }

        public static void SendMailMessageDeletedResponse(IRealmClient client, int msgGuid)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.MailMessageDeleted))
            {
                packet.WriteByte(1);
                packet.WriteInt32(msgGuid);
                client.Send(packet, false);
            }
        }
    }
}
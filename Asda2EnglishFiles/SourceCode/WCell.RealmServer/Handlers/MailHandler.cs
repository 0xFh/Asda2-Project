using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.Constants.Items;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items.Enchanting;
using WCell.RealmServer.Mail;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class MailHandler
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static void HandleListMail(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            EntityId id = packet.ReadEntityId();
            if (!MailHandler.CheckMailBox(activeCharacter, activeCharacter.Map.GetObject(id) as GameObject))
                return;
            activeCharacter.MailAccount.SendMailList();
        }

        public static void HandleTakeMoney(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            EntityId id = packet.ReadEntityId();
            uint messageId = packet.ReadUInt32();
            if (!MailHandler.CheckMailBox(activeCharacter, activeCharacter.Map.GetObject(id) as GameObject))
                return;
            activeCharacter.MailAccount.TakeMoney(messageId);
        }

        public static void HandleMarkAsRead(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            EntityId id = packet.ReadEntityId();
            uint messageId = packet.ReadUInt32();
            if (!MailHandler.CheckMailBox(activeCharacter, activeCharacter.Map.GetObject(id) as GameObject))
                return;
            activeCharacter.MailAccount.MarkAsRead(messageId);
        }

        public static void HandleReturnToSender(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            EntityId id = packet.ReadEntityId();
            uint messageId = packet.ReadUInt32();
            if (!MailHandler.CheckMailBox(activeCharacter, activeCharacter.Map.GetObject(id) as GameObject))
                return;
            int sender = (int) activeCharacter.MailAccount.ReturnToSender(messageId);
        }

        public static void HandleDelete(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            EntityId id = packet.ReadEntityId();
            uint messageId = packet.ReadUInt32();
            if (!MailHandler.CheckMailBox(activeCharacter, activeCharacter.Map.GetObject(id) as GameObject))
                return;
            int num = (int) activeCharacter.MailAccount.DeleteMail(messageId);
        }

        public static void HandleNextTime(IRealmClient client, RealmPacketIn packet)
        {
            client.ActiveCharacter.MailAccount.GetNextMailTime();
        }

        public static void HandleItemTextQuery(IRealmClient client, RealmPacketIn packet)
        {
            Character activeCharacter = client.ActiveCharacter;
            uint itemTextId = packet.ReadUInt32();
            uint mailOrItemId = packet.ReadUInt32();
            int num = (int) packet.ReadUInt32();
            activeCharacter.MailAccount.SendItemText(itemTextId, mailOrItemId);
        }

        private static bool CheckMailBox(Character chr, GameObject mailbox)
        {
            if (chr.GodMode)
                return true;
            if (mailbox != null && mailbox.GOType == GameObjectType.Mailbox)
                return mailbox.Handler.CanBeUsedBy(chr);
            return false;
        }

        /// <summary>
        /// Sends a responce detailing the results of the client's mail request.
        /// </summary>
        public static void SendResult(IPacketReceiver client, uint mailId, MailResult result, MailError err)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SEND_MAIL_RESULT, 12))
            {
                packet.Write(mailId);
                packet.Write((uint) result);
                packet.Write((uint) err);
                client.Send(packet, false);
            }
        }

        /// <summary>
        /// Sends a responce detailing the results of the client's mail request.
        /// </summary>
        public static void SendResult(IPacketReceiver client, uint mailId, MailResult result, MailError err,
            InventoryError invErr)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SEND_MAIL_RESULT, 16))
            {
                packet.Write(mailId);
                packet.Write((uint) result);
                packet.Write((uint) err);
                packet.Write((uint) invErr);
                client.Send(packet, false);
            }
        }

        /// <summary>
        /// Sends a responce detailing the results of the client's mail request.
        /// </summary>
        public static void SendResult(IPacketReceiver client, uint mailId, MailResult result, MailError err,
            uint itemId, int itemCount)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_SEND_MAIL_RESULT, 20))
            {
                packet.Write(mailId);
                packet.Write((uint) result);
                packet.Write((uint) err);
                packet.Write(itemId);
                packet.Write(itemCount);
                client.Send(packet, false);
            }
        }

        /// <summary>Sends a list of mail messages to the client.</summary>
        public static void SendMailList(IPacketReceiver client, IList<MailMessage> messages)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_MAIL_LIST_RESULT,
                128 * messages.Count))
            {
                packet.Write(messages.Count);
                int num1 = Math.Min(messages.Count, (int) byte.MaxValue);
                packet.Write((byte) num1);
                for (int index1 = 0; index1 < num1; ++index1)
                {
                    MailMessage message = messages[index1];
                    if (!message.IsDeleted)
                    {
                        long position1 = packet.Position;
                        packet.Position = position1 + 2L;
                        packet.Write((uint) message.Guid);
                        packet.Write((byte) message.MessageType);
                        switch (message.MessageType)
                        {
                            case MailType.Normal:
                                packet.Write((ulong) message.SenderEntityId);
                                break;
                            case MailType.Auction:
                                packet.Write(message.SenderEntityId.Low);
                                break;
                            case MailType.Creature:
                                packet.Write(message.SenderEntityId.Low);
                                break;
                            case MailType.GameObject:
                                packet.Write(message.SenderEntityId.Low);
                                break;
                            case MailType.Item:
                                packet.WriteUInt(0);
                                break;
                        }

                        packet.Write(message.CashOnDelivery);
                        packet.Write(0U);
                        packet.Write((uint) message.MessageStationary);
                        packet.Write(message.IncludedMoney);
                        MailListFlags mailListFlags =
                            message.ReadTime.HasValue ? MailListFlags.Read : MailListFlags.NotRead;
                        switch (message.MessageType)
                        {
                            case MailType.Normal:
                                mailListFlags |= MailListFlags.Delete;
                                if (message.IncludedItemCount > 0)
                                {
                                    mailListFlags |= MailListFlags.Return;
                                    break;
                                }

                                break;
                            case MailType.Auction:
                                mailListFlags |= MailListFlags.Auction;
                                break;
                        }

                        packet.Write((uint) mailListFlags);
                        packet.Write((float) ((message.ExpireTime - DateTime.Now).TotalMilliseconds / 86400000.0));
                        packet.Write(0U);
                        packet.Write(message.Subject);
                        packet.Write(message.Body);
                        if (message.IncludedItemCount == 0)
                        {
                            packet.Write((byte) 0);
                        }
                        else
                        {
                            ICollection<ItemRecord> includedItems = message.IncludedItems;
                            packet.Write((byte) includedItems.Count);
                            byte num2 = 0;
                            foreach (ItemRecord itemRecord in (IEnumerable<ItemRecord>) includedItems)
                            {
                                packet.Write(num2++);
                                if (itemRecord != null)
                                {
                                    packet.Write(itemRecord.EntityLowId);
                                    packet.Write(itemRecord.EntryId);
                                    if (itemRecord.EnchantIds != null)
                                    {
                                        for (int index2 = 0; index2 < 7; ++index2)
                                        {
                                            int enchantId = itemRecord.EnchantIds[index2];
                                            if (enchantId != 0 &&
                                                EnchantMgr.GetEnchantmentEntry((uint) enchantId) != null)
                                            {
                                                packet.Write(0);
                                                if (index2 == 1)
                                                    packet.Write(itemRecord.EnchantTempTime);
                                                else
                                                    packet.Write(0);
                                                packet.Write(enchantId);
                                            }
                                            else
                                            {
                                                packet.Write(0);
                                                packet.Write(0);
                                                packet.Write(0);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int index2 = 0; index2 < 7; ++index2)
                                        {
                                            packet.Write(0U);
                                            packet.Write(0);
                                            packet.Write(0U);
                                        }
                                    }

                                    packet.Write(itemRecord.RandomProperty);
                                    packet.Write(itemRecord.RandomSuffix);
                                    packet.Write(itemRecord.Amount);
                                    packet.Write((uint) itemRecord.Charges);
                                    packet.Write(itemRecord.Template.MaxDurability);
                                    packet.Write(itemRecord.Durability);
                                    packet.Write((byte) 0);
                                }
                                else
                                {
                                    packet.Write(0U);
                                    packet.Write(0U);
                                    for (byte index2 = 0; index2 < (byte) 7; ++index2)
                                    {
                                        packet.Write(0U);
                                        packet.Write(0U);
                                        packet.Write(0U);
                                    }

                                    packet.Write(0);
                                    packet.Write(0);
                                    packet.Write(0);
                                    packet.Write(0);
                                    packet.Write(0);
                                    packet.Write(0);
                                    packet.Write((byte) 0);
                                }
                            }
                        }

                        long position2 = packet.Position;
                        packet.Position = position1;
                        packet.Write((ushort) (position2 - position1));
                        packet.Position = position2;
                    }
                }

                client.Send(packet, false);
            }
        }

        /// <summary>Notifies the Client that there is new mail</summary>
        public static void SendNotify(IRealmClient client)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.SMSG_RECEIVED_MAIL, 4))
            {
                packet.Write(0);
                client.Send(packet, false);
            }
        }

        public static void SendNextMailTime(IPacketReceiver client, ICollection<MailMessage> mail)
        {
            using (RealmPacketOut packet = new RealmPacketOut((PacketId) RealmServerOpCode.MSG_QUERY_NEXT_MAIL_TIME, 8))
            {
                if (mail.Count <= 0)
                {
                    packet.Write(3349725184U);
                    packet.Write(0U);
                    client.Send(packet, false);
                }
                else
                {
                    packet.Write(0U);
                    packet.Write((uint) mail.Count);
                    foreach (MailMessage mailMessage in (IEnumerable<MailMessage>) mail)
                    {
                        packet.Write((ulong) mailMessage.SenderEntityId);
                        if (mailMessage.MessageType == MailType.Auction)
                        {
                            packet.Write(2U);
                            packet.Write(2U);
                        }
                        else
                        {
                            packet.Write(0U);
                            packet.Write(0U);
                        }

                        packet.Write((uint) mailMessage.MessageStationary);
                        packet.Write(3321888768U);
                    }

                    client.Send(packet, false);
                }
            }
        }

        public static void SendItemTextQueryResponce(IPacketReceiver client, uint id, string text)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_ITEM_TEXT_QUERY_RESPONSE))
            {
                packet.Write(id);
                packet.Write(text);
                client.Send(packet, false);
            }
        }
    }
}
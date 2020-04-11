using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Factions;
using WCell.Core;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Network;
using WCell.Util.Threading;

namespace WCell.RealmServer.Mail
{
    /// <summary>Represents the ingame Mail-Account of this Character</summary>
    public class MailAccount
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private bool firstCheckSinceLogin = true;
        private Character m_chr;
        private GameObject m_mailBox;

        /// <summary>
        /// All Mail that is associated with this character (undelivered, read or unread)
        /// </summary>
        public Dictionary<uint, MailMessage> AllMail;

        public MailAccount(Character chr)
        {
            this.m_chr = chr;
            this.AllMail = new Dictionary<uint, MailMessage>((int) MailMgr.MaxMailCount);
        }

        public Character Owner
        {
            get { return this.m_chr; }
            internal set { this.m_chr = value; }
        }

        /// <summary>Loads stored mail from DB</summary>
        internal void Load()
        {
            foreach (MailMessage mailMessage in MailMessage.FindAllMessagesFor(this.m_chr.EntityId.Low))
            {
                if (!this.AllMail.ContainsKey((uint) mailMessage.Guid))
                    this.AllMail.Add((uint) mailMessage.Guid, mailMessage);
            }
        }

        /// <summary>The currently used MailBox (or null)</summary>
        public GameObject MailBox
        {
            get { return this.m_mailBox; }
            set
            {
                if (this.m_mailBox == value)
                    return;
                this.m_mailBox = value;
            }
        }

        public MailError SendMail(string recipientName, string subject, string body, MailStationary stationary,
            ICollection<Item> items, uint money, uint cod)
        {
            string name = recipientName;
            Character character = World.GetCharacter(name, false);
            FactionGroup factionGroup;
            int num;
            CharacterRecord recipient;
            if (character != null)
            {
                factionGroup = character.Faction.Group;
                num = character.MailAccount.AllMail.Count;
                recipient = character.Record;
            }
            else
            {
                CharacterRecord recordByName = CharacterRecord.GetRecordByName(name);
                if (recordByName == null)
                {
                    MailHandler.SendResult((IPacketReceiver) this.m_chr.Client, 0U, MailResult.MailSent,
                        MailError.RECIPIENT_NOT_FOUND);
                    return MailError.RECIPIENT_NOT_FOUND;
                }

                factionGroup = FactionMgr.GetFactionGroup(recordByName.Race);
                num = recordByName.MailCount;
                recipient = recordByName;
            }

            if (!this.m_chr.GodMode)
            {
                if (stationary == MailStationary.GM)
                {
                    MailHandler.SendResult((IPacketReceiver) this.m_chr.Client, 0U, MailResult.MailSent,
                        MailError.INTERNAL_ERROR);
                    return MailError.INTERNAL_ERROR;
                }

                if (RelationMgr.IsIgnoring(recipient.EntityLowId, this.m_chr.EntityId.Low))
                {
                    MailHandler.SendResult((IPacketReceiver) this.m_chr.Client, 0U, MailResult.MailSent,
                        MailError.RECIPIENT_NOT_FOUND);
                    return MailError.RECIPIENT_NOT_FOUND;
                }

                if (!MailMgr.AllowInterFactionMail && !this.m_chr.GodMode && factionGroup != this.m_chr.Faction.Group)
                {
                    MailHandler.SendResult((IPacketReceiver) this.m_chr, 0U, MailResult.MailSent,
                        MailError.NOT_YOUR_ALLIANCE);
                    return MailError.NOT_YOUR_ALLIANCE;
                }

                if ((long) num > (long) MailMgr.MaxMailCount)
                {
                    MailHandler.SendResult((IPacketReceiver) this.m_chr, 0U, MailResult.MailSent,
                        MailError.RECIPIENT_CAP_REACHED);
                    return MailError.RECIPIENT_CAP_REACHED;
                }
            }

            return this.SendMail(recipient, subject, body, stationary, items, money, cod);
        }

        /// <summary>
        /// Creates and sends a new Mail with the given parameters
        /// </summary>
        public MailError SendMail(CharacterRecord recipient, string subject, string body, MailStationary stationary,
            ICollection<Item> items, uint money, uint cod)
        {
            if (subject.Length > 128 || body.Length > 512)
                return MailError.INTERNAL_ERROR;
            if ((int) recipient.EntityLowId == (int) this.m_chr.EntityId.Low)
            {
                MailHandler.SendResult((IPacketReceiver) this.m_chr.Client, 0U, MailResult.MailSent,
                    MailError.CANNOT_SEND_TO_SELF);
                return MailError.CANNOT_SEND_TO_SELF;
            }

            uint amount = money;
            if (MailMgr.ChargePostage && !this.m_chr.GodMode)
            {
                amount += MailMgr.PostagePrice;
                uint num = items == null ? 0U : (uint) items.Count;
                if (num > 0U)
                    amount += (num - 1U) * MailMgr.PostagePrice;
            }

            if (amount > this.m_chr.Money)
            {
                MailHandler.SendResult((IPacketReceiver) this.m_chr.Client, 0U, MailResult.MailSent,
                    MailError.NOT_ENOUGH_MONEY);
                return MailError.NOT_ENOUGH_MONEY;
            }

            this.m_chr.SubtractMoney(amount);
            MailHandler.SendResult((IPacketReceiver) this.m_chr.Client, 0U, MailResult.MailSent, MailError.OK);
            this.m_chr.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.GoldSpentForMail, amount,
                0U, (Unit) null);
            uint num1 = 0;
            if (!this.m_chr.GodMode && (money > 0U || items != null && items.Count > 0) &&
                this.m_chr.Account.AccountId != recipient.AccountId)
                num1 = MailMgr.MaxPacketDeliveryDelay;
            MailMessage letter = new MailMessage(subject, body)
            {
                LastModifiedOn = new DateTime?(),
                SenderId = this.m_chr.EntityId.Low,
                ReceiverId = recipient.EntityLowId,
                MessageStationary = stationary,
                MessageType = MailType.Normal,
                CashOnDelivery = cod,
                IncludedMoney = money,
                SendTime = DateTime.Now,
                DeliveryTime = DateTime.Now.AddSeconds((double) num1)
            };
            if (items != null && items.Count > 0)
                letter.SetItems(items);
            MailMgr.SendMail(letter);
            return MailError.OK;
        }

        /// <summary>
        /// Returns the corresponding Item, if it can be mailed, else will send error message
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public void MarkAsRead(uint messageId)
        {
            MailMessage mailMessage;
            if (!this.AllMail.TryGetValue(messageId, out mailMessage))
                return;
            mailMessage.ReadTime = new DateTime?(DateTime.Now);
        }

        public void SendMailList()
        {
            if (this.firstCheckSinceLogin)
            {
                ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((IMessage) new Message((Action) (() =>
                {
                    this.Load();
                    IContextHandler contextHandler = this.m_chr.ContextHandler;
                    if (contextHandler == null)
                        return;
                    contextHandler.AddMessage((Action) (() =>
                    {
                        if (this.m_chr == null || !this.m_chr.IsInWorld)
                            return;
                        this.SendMailList();
                    }));
                })));
                this.firstCheckSinceLogin = false;
            }
            else
                MailHandler.SendMailList((IPacketReceiver) this.m_chr.Client, (IList<MailMessage>) this.CollectMail());
        }

        /// <summary>IMPORTANT: Required IO-Queue Context</summary>
        /// <returns></returns>
        public List<MailMessage> GetMail()
        {
            if (this.firstCheckSinceLogin)
            {
                this.Load();
                this.firstCheckSinceLogin = false;
            }

            return this.CollectMail();
        }

        private List<MailMessage> CollectMail()
        {
            List<MailMessage> mailMessageList1 = new List<MailMessage>((int) MailMgr.MaxMailCount);
            uint num1 = 0;
            uint num2 = 0;
            List<MailMessage> mailMessageList2 = new List<MailMessage>(10);
            foreach (MailMessage mailMessage in this.AllMail.Values)
            {
                if (!(mailMessage.DeliveryTime >= DateTime.Now))
                {
                    if (mailMessage.ExpireTime <= DateTime.Now)
                        mailMessageList2.Add(mailMessage);
                    else if (!mailMessage.IsDeleted)
                    {
                        if (num1 <= MailMgr.MaxMailCount)
                        {
                            if (num2 <= MailMgr.MaxStoredItems)
                            {
                                ++num1;
                                num2 += (uint) mailMessage.IncludedItemCount;
                                mailMessageList1.Add(mailMessage);
                            }
                            else
                                break;
                        }
                        else
                            break;
                    }
                }
            }

            foreach (MailMessage letter in mailMessageList2)
                this.DeleteOrReturn(letter);
            return mailMessageList1;
        }

        public void TakeMoney(uint messageId)
        {
            MailMessage mailMessage;
            if (!this.AllMail.TryGetValue(messageId, out mailMessage) || mailMessage.IsDeleted ||
                mailMessage.DeliveryTime > DateTime.Now)
            {
                MailHandler.SendResult((IPacketReceiver) this.m_chr.Client, messageId, MailResult.MoneyTaken,
                    MailError.INTERNAL_ERROR);
            }
            else
            {
                this.m_chr.AddMoney(mailMessage.IncludedMoney);
                mailMessage.IncludedMoney = 0U;
                MailHandler.SendResult((IPacketReceiver) this.m_chr.Client, messageId, MailResult.MoneyTaken,
                    MailError.OK);
            }
        }

        public MailError ReturnToSender(uint messageId)
        {
            MailMessage mailMessage;
            if (!this.AllMail.TryGetValue(messageId, out mailMessage) || mailMessage.IsDeleted ||
                mailMessage.DeliveryTime > DateTime.Now)
            {
                MailHandler.SendResult((IPacketReceiver) this.m_chr.Client, messageId, MailResult.ReturnedToSender,
                    MailError.INTERNAL_ERROR);
                return MailError.INTERNAL_ERROR;
            }

            mailMessage.ReturnToSender();
            MailHandler.SendResult((IPacketReceiver) this.m_chr.Client, messageId, MailResult.ReturnedToSender,
                MailError.OK);
            return MailError.OK;
        }

        public MailError DeleteMail(uint messageId)
        {
            MailMessage letter;
            if (!this.AllMail.TryGetValue(messageId, out letter) || letter.IsDeleted ||
                letter.DeliveryTime > DateTime.Now)
            {
                MailHandler.SendResult((IPacketReceiver) this.m_chr.Client, messageId, MailResult.Deleted,
                    MailError.INTERNAL_ERROR);
                return MailError.INTERNAL_ERROR;
            }

            this.DeleteOrReturn(letter);
            MailHandler.SendResult((IPacketReceiver) this.m_chr.Client, messageId, MailResult.Deleted, MailError.OK);
            return MailError.OK;
        }

        public void GetNextMailTime()
        {
            uint num = 0;
            List<MailMessage> mailMessageList = new List<MailMessage>(2);
            foreach (MailMessage mailMessage in this.AllMail.Values)
            {
                if (!mailMessage.WasRead && !(mailMessage.DeliveryTime > DateTime.Now))
                {
                    ++num;
                    if (num <= 2U)
                        mailMessageList.Add(mailMessage);
                    else
                        break;
                }
            }

            MailHandler.SendNextMailTime((IPacketReceiver) this.m_chr.Client,
                (ICollection<MailMessage>) mailMessageList);
        }

        public void SendItemText(uint itemTextId, uint mailOrItemId)
        {
        }

        private void DeleteOrReturn(MailMessage letter)
        {
            this.AllMail.Remove((uint) letter.Guid);
            if (!letter.IsDeleted && (letter.IncludedItemCount > 0 || letter.IncludedMoney > 0U))
            {
                letter.ReturnToSender();
            }
            else
            {
                letter.DeletedTime = new DateTime?(DateTime.Now);
                ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage(
                    (IMessage) new Message(new Action(letter.Destroy)));
            }
        }
    }
}
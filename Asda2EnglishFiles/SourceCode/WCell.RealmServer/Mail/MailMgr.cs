using Castle.ActiveRecord.Queries;
using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Database;
using WCell.Core.Initialization;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Network;
using WCell.Util.Variables;

namespace WCell.RealmServer.Mail
{
    public class MailMgr : Manager<MailMgr>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Min delivery delay of mail with items or Gold in seconds (Default: 30 minutes)
        /// </summary>
        public static uint MinPacketDeliveryDelay = 1800;

        /// <summary>
        /// Max delivery delay of mail with items or Gold in seconds (Default: 60 minutes)
        /// </summary>
        public static uint MaxPacketDeliveryDelay = 3600;

        /// <summary>
        /// Max number of messages each player can store in their mailbox.
        /// </summary>
        public static uint MaxMailCount = 100;

        /// <summary>
        /// Max number of items each player can store in their mailbox.
        /// </summary>
        public static uint MaxStoredItems = 256;

        /// <summary>
        /// Max number of days to store regular mail in the mailbox.
        /// </summary>
        public static uint MailExpiryDelay = 30;

        /// <summary>
        /// Max number of days to store Cash-On_Delivery mail in the mailbox.
        /// </summary>
        public static uint MaxCODExpiryDelay = 3;

        /// <summary>
        /// The index of the item template to use for creating permanent mail storage.
        /// </summary>
        [Variable("ChargeMailPostage")] public static bool ChargePostage = true;

        /// <summary>
        /// The amount of postage to charge per message sent, in copper.
        /// </summary>
        public static uint PostagePrice = 30;

        /// <summary>
        /// Max number of items each player can store in their mailbox.
        /// </summary>
        public const int MaxItemsPerMail = 12;

        public const int MaxMailSubjectLength = 128;
        public const int MaxMailBodyLength = 512;

        /// <summary>
        /// Whether or not to delivery mail instantly for any type of mail.
        /// </summary>
        public static bool DeliverMailInstantly;

        /// <summary>
        /// Whether to allow characters to send mail to characters on the opposite Team.
        /// </summary>
        public static bool AllowInterFactionMail;

        internal static NHIdGenerator TextIdGenerator;

        [WCell.Core.Initialization.Initialization(InitializationPass.Sixth)]
        public static void Initialize()
        {
            try
            {
                MailMgr.CreateIdGenerators();
            }
            catch (Exception ex)
            {
                RealmDBMgr.OnDBError(ex);
                MailMgr.CreateIdGenerators();
            }
        }

        private static void CreateIdGenerators()
        {
            NHIdGenerator nhIdGenerator1 = new NHIdGenerator(typeof(MailMessage), "_TextId", 1L);
            NHIdGenerator nhIdGenerator2 = new NHIdGenerator(typeof(ItemRecord), "m_ItemTextId", 1L);
            if (nhIdGenerator1.LastId > nhIdGenerator2.LastId)
                MailMgr.TextIdGenerator = nhIdGenerator1;
            else
                MailMgr.TextIdGenerator = nhIdGenerator2;
        }

        protected MailMgr()
        {
        }

        public static bool SendMail(string recipientName, string subject, string body)
        {
            uint idByName = CharacterRecord.GetIdByName(recipientName);
            if (idByName <= 0U)
                return false;
            MailMgr.SendMail(idByName, subject, body);
            return true;
        }

        public static void SendMail(uint recipientLowId, string subject, string body)
        {
            DateTime now = DateTime.Now;
            new MailMessage(subject, body)
            {
                LastModifiedOn = new DateTime?(),
                SenderId = 0U,
                ReceiverId = recipientLowId,
                MessageStationary = MailStationary.Normal,
                MessageType = MailType.Normal,
                SendTime = now,
                DeliveryTime = now
            }.Send();
        }

        public static MailError SendMail(string recipientName, string subject, string body, MailStationary stationary,
            ICollection<ItemRecord> items, uint money, uint cod, IPacketReceiver sender)
        {
            uint idByName = CharacterRecord.GetIdByName(recipientName);
            if (idByName <= 0U)
                return MailError.RECIPIENT_NOT_FOUND;
            return MailMgr.SendMail(idByName, subject, body, stationary, items, money, cod, sender);
        }

        public static MailError SendMail(uint recipientLowId, string subject, string body, MailStationary stationary,
            ICollection<ItemRecord> items, uint money, uint cod, IPacketReceiver sender)
        {
            if (sender != null)
                MailHandler.SendResult(sender, 0U, MailResult.MailSent, MailError.OK);
            uint num = 0;
            MailMessage letter = new MailMessage(subject, body)
            {
                LastModifiedOn = new DateTime?(),
                SenderId = sender is IEntity ? ((IEntity) sender).EntityId.Low : 0U,
                ReceiverId = recipientLowId,
                MessageStationary = stationary,
                MessageType = MailType.Normal,
                CashOnDelivery = cod,
                IncludedMoney = money,
                SendTime = DateTime.Now,
                DeliveryTime = DateTime.Now.AddSeconds((double) num)
            };
            if (items != null && items.Count > 0)
                letter.SetItems(items);
            MailMgr.SendMail(letter);
            return MailError.OK;
        }

        public static void SendMail(MailMessage letter)
        {
            letter.ExpireTime = letter.DeliveryTime.AddDays(letter.CashOnDelivery > 0U
                ? (double) MailMgr.MaxCODExpiryDelay
                : (double) MailMgr.MailExpiryDelay);
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.ExecuteInContext((Action) (() =>
            {
                letter.Save();
                Character recipient = World.GetCharacter(letter.ReceiverId);
                if (recipient == null)
                    return;
                if (letter.DeliveryTime < DateTime.Now)
                    recipient.ExecuteInContext((Action) (() =>
                    {
                        letter.Recipient = recipient.Record;
                        recipient.MailAccount.AllMail.Add((uint) letter.Guid, letter);
                        MailHandler.SendNotify(recipient.Client);
                    }));
                else
                    recipient.CallDelayed(letter.RemainingDeliveryMillis,
                        (Action<WorldObject>) (chr => MailHandler.SendNotify(recipient.Client)));
            }));
        }

        /// <summary>
        /// Returns all value mail that was sent to the Character with the given Id to their original sender
        /// </summary>
        public static void ReturnValueMailFor(uint charId)
        {
            new ScalarQuery<long>(typeof(CharacterRecord), QueryLanguage.Sql,
                string.Format("UPDATE {0} SET {1} = {2}, {3} = 0 WHERE {4} = {5} AND ({6} > 0 OR {7} > 0 OR {8} > 0)",
                    (object) DatabaseUtil.Dialect.QuoteForTableName(typeof(MailMessage).Name),
                    (object) DatabaseUtil.Dialect.QuoteForColumnName("ReceiverId"),
                    (object) DatabaseUtil.Dialect.QuoteForColumnName("SenderId"),
                    (object) DatabaseUtil.Dialect.QuoteForColumnName("CashOnDelivery"),
                    (object) DatabaseUtil.Dialect.QuoteForColumnName("SenderId"), (object) charId,
                    (object) DatabaseUtil.Dialect.QuoteForColumnName("CashOnDelivery"),
                    (object) DatabaseUtil.Dialect.QuoteForColumnName("IncludedMoney"),
                    (object) DatabaseUtil.Dialect.QuoteForColumnName("IncludedItemCount"))).Execute();
        }
    }
}
using Castle.ActiveRecord;
using NLog;
using System;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.Util.NLog;

namespace WCell.RealmServer.Asda2Mail
{
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property)]
    public class Asda2MailMessage : ActiveRecordBase<Asda2MailMessage>
    {
        private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

        private static readonly NHIdGenerator s_idGenerator =
            new NHIdGenerator(typeof(Asda2MailMessage), nameof(Guid), 1L);

        [Property] public string Subject { get; set; }

        [Property] public string Body { get; set; }

        public Asda2Item Item { get; set; }

        [Property] public uint Gold { get; set; }

        [Property] public uint RecieverId { get; set; }

        [Property] public bool IsReaded { get; set; }

        [Property] public long ItemGuid { get; set; }

        /// <summary>Returns the next unique Id for a new Item</summary>
        public static long NextId()
        {
            return Asda2MailMessage.s_idGenerator.Next();
        }

        /// <summary>Create an exisiting MailMessage</summary>
        public Asda2MailMessage()
        {
        }

        /// <summary>Create a new MailMessage</summary>
        public Asda2MailMessage(string subject, string body, Asda2Item item, uint gold, uint recieverId,
            string senderName)
        {
            this.Subject = subject;
            this.Body = body;
            this.Item = item;
            this.Gold = gold;
            this.RecieverId = recieverId;
            this.ItemGuid = item == null ? -1L : item.Record.Guid;
            this.SenderName = senderName;
            this.DeleteTime = DateTime.Now.AddDays(10.0);
            this.Guid = Asda2MailMessage.NextId();
        }

        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public long Guid { get; set; }

        [Property] public DateTime DeleteTime { get; set; }

        [Property] public string SenderName { get; set; }

        public static Asda2MailMessage[] LoadAll(Character chr)
        {
            Asda2MailMessage[] allByProperty =
                ActiveRecordBase<Asda2MailMessage>.FindAllByProperty("RecieverId", (object) chr.EntityId.Low);
            foreach (Asda2MailMessage asda2MailMessage in allByProperty)
                asda2MailMessage.Init();
            return allByProperty;
        }

        private void Init()
        {
            if (this.ItemGuid <= 0L)
                return;
            Asda2ItemRecord record;
            try
            {
                record = ActiveRecordBase<Asda2ItemRecord>.Find((object) this.ItemGuid);
            }
            catch (NotFoundException ex)
            {
                LogUtil.WarnException(
                    string.Format("Mail message {0} failed to load cause item {1} not founded. Mail message deleted.",
                        (object) this.Guid, (object) this.ItemGuid), new object[0]);
                this.ItemGuid = -1L;
                this.SaveLater();
                return;
            }

            this.Item = Asda2Item.CreateItem(record, (Character) null);
        }
    }
}
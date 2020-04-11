using Castle.ActiveRecord;
using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Core;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Mail
{
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property)]
    public class MailMessage : ActiveRecordBase<MailMessage>
    {
        private static readonly Logger s_log = LogManager.GetCurrentClassLogger();
        private static readonly NHIdGenerator s_idGenerator = new NHIdGenerator(typeof(MailMessage), nameof(Guid), 1L);

        [Field("ReceiverId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _receiverId;

        [Field("SenderId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _senderId;

        [Field("IncludedMoney", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private long _includedMoney;

        [Field("CashOnDelivery", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private long _cashOnDelivery;

        private ICollection<ItemRecord> _items;

        /// <summary>Returns the next unique Id for a new Item</summary>
        public static long NextId()
        {
            return MailMessage.s_idGenerator.Next();
        }

        /// <summary>Create an exisiting MailMessage</summary>
        public MailMessage()
        {
        }

        /// <summary>Create a new MailMessage</summary>
        public MailMessage(string subject, string body)
        {
            this.Guid = MailMessage.NextId();
            this.TextId = (uint) MailMgr.TextIdGenerator.Next();
            this.Subject = subject;
            this.Body = body;
        }

        public CharacterRecord Recipient { get; set; }

        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public long Guid { get; set; }

        [Version(UnsavedValue = "null")] public DateTime? LastModifiedOn { get; set; }

        [Property(NotNull = true)] public MailType MessageType { get; set; }

        [Property(NotNull = true)] public MailStationary MessageStationary { get; set; }

        public uint ReceiverId
        {
            get { return (uint) this._receiverId; }
            set { this._receiverId = (int) value; }
        }

        public uint SenderId
        {
            get { return (uint) this._senderId; }
            set { this._senderId = (int) value; }
        }

        public EntityId ReceiverEntityId
        {
            get { return EntityId.GetPlayerId((uint) this._receiverId); }
            set { this._receiverId = (int) value.Low; }
        }

        public EntityId SenderEntityId
        {
            get { return EntityId.GetPlayerId((uint) this._senderId); }
            set { this._senderId = (int) value.Low; }
        }

        [Property(Length = 512, NotNull = true)]
        public string Subject { get; set; }

        /// <summary>The body of the message</summary>
        [Property(Length = 8192, NotNull = true)]
        public string Body { get; set; }

        public uint TextId
        {
            get { return (uint) this._TextId; }
            set { this._TextId = (int) value; }
        }

        [Property("TextId", NotNull = true)] public int _TextId { get; set; }

        [Property(NotNull = true)] public DateTime SendTime { get; set; }

        [Property(NotNull = true)] public DateTime DeliveryTime { get; set; }

        public int RemainingDeliveryMillis
        {
            get { return (int) (this.DeliveryTime - DateTime.Now).TotalMilliseconds; }
        }

        public bool WasRead
        {
            get { return this.ReadTime.HasValue; }
        }

        [Property] public DateTime? ReadTime { get; set; }

        [Property(NotNull = true)] public DateTime ExpireTime { get; set; }

        [Property] public DateTime? DeletedTime { get; set; }

        public uint IncludedMoney
        {
            get { return (uint) this._includedMoney; }
            set { this._includedMoney = (long) value; }
        }

        public uint CashOnDelivery
        {
            get { return (uint) this._cashOnDelivery; }
            set { this._cashOnDelivery = (long) value; }
        }

        [Property(NotNull = true)] public bool CopiedToItem { get; internal set; }

        [Property] public int IncludedItemCount { get; private set; }

        /// <summary>
        /// The list of included items.
        /// Returns null if no items are added.
        /// </summary>
        public ICollection<ItemRecord> IncludedItems
        {
            get { return (ICollection<ItemRecord>) null; }
        }

        public ItemRecord AddItem(Asda2ItemId item)
        {
            ItemTemplate template = ItemMgr.GetTemplate(item);
            if (template == null)
                return (ItemRecord) null;
            return this.AddItem(template);
        }

        public ItemRecord AddItem(ItemTemplate template)
        {
            ItemRecord record = ItemRecord.CreateRecord(template);
            this.AddItem(record);
            return record;
        }

        public void AddItem(ItemRecord record)
        {
            if (this._items == null)
                this._items = (ICollection<ItemRecord>) new List<ItemRecord>(3);
            record.MailId = this.Guid;
            this._items.Add(record);
            ++this.IncludedItemCount;
        }

        public ItemRecord RemoveItem(uint lowId)
        {
            ICollection<ItemRecord> includedItems = this.IncludedItems;
            if (includedItems != null)
            {
                foreach (ItemRecord itemRecord in (IEnumerable<ItemRecord>) includedItems)
                {
                    if ((int) itemRecord.EntityLowId == (int) lowId)
                    {
                        includedItems.Remove(itemRecord);
                        this.IncludedItemCount = includedItems.Count;
                        return itemRecord;
                    }
                }
            }

            return (ItemRecord) null;
        }

        public void SetItems(ICollection<Item> items)
        {
            if (this._items != null)
                throw new InvalidOperationException("Tried to set Items after Items were already set: " +
                                                    (object) this);
            this._items = (ICollection<ItemRecord>) new List<ItemRecord>(items.Count);
            foreach (Item obj in (IEnumerable<Item>) items)
            {
                obj.Remove(true);
                obj.Record.MailId = this.Guid;
                this._items.Add(obj.Record);
            }

            this.IncludedItemCount = this._items.Count;
        }

        public void SetItems(ICollection<ItemRecord> items)
        {
            if (this._items != null)
                throw new InvalidOperationException("Tried to set Items after Items were already set: " +
                                                    (object) this);
            this._items = (ICollection<ItemRecord>) new List<ItemRecord>(items.Count);
            foreach (ItemRecord itemRecord in (IEnumerable<ItemRecord>) items)
            {
                itemRecord.MailId = this.Guid;
                this._items.Add(itemRecord);
            }

            this.IncludedItemCount = this._items.Count;
        }

        internal void PutBack(ItemRecord item)
        {
            this._items.Add(item);
            ++this.IncludedItemCount;
        }

        public bool IsDeleted
        {
            get
            {
                if (this.DeletedTime.HasValue)
                    return this.DeletedTime.Value < DateTime.Now;
                return false;
            }
        }

        public override void Create()
        {
            if (this._items != null)
            {
                foreach (ActiveRecordBase activeRecordBase in (IEnumerable<ItemRecord>) this._items)
                    activeRecordBase.Save();
            }

            base.Create();
        }

        public override void Update()
        {
            if (this._items != null)
            {
                foreach (ActiveRecordBase activeRecordBase in (IEnumerable<ItemRecord>) this._items)
                    activeRecordBase.Save();
            }

            base.Update();
        }

        public override void Save()
        {
            if (this._items != null)
            {
                foreach (ActiveRecordBase activeRecordBase in (IEnumerable<ItemRecord>) this._items)
                    activeRecordBase.Save();
            }

            base.Save();
        }

        /// <summary>Delete letter and all containing Items</summary>
        public void Destroy()
        {
            if (this.IncludedItemCount > 0)
            {
                ActiveRecordBase<ItemRecord>.DeleteAll("MailId = " + (object) this.Guid);
                this._items = (ICollection<ItemRecord>) null;
            }

            this.Delete();
        }

        /// <summary>
        /// Returns to sender or Destroys the mail (if sender doesn't exist)
        /// </summary>
        public void ReturnToSender()
        {
            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.ExecuteInContext((Action) (() =>
            {
                if (!CharacterRecord.Exists(this.SenderId))
                {
                    this.Destroy();
                }
                else
                {
                    this.SenderId = this.ReceiverId;
                    this.ReceiverId = this.SenderId;
                    this.Subject += " [Returned]";
                    this.Send();
                }
            }));
        }

        public void Send()
        {
            MailMgr.SendMail(this);
        }

        public static IEnumerable<MailMessage> FindAllMessagesFor(uint charLowId)
        {
            return (IEnumerable<MailMessage>) ActiveRecordBase<MailMessage>.FindAllByProperty("_receiverId",
                (object) (int) charLowId);
        }
    }
}
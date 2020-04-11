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
  [ActiveRecord(Access = PropertyAccess.Property)]
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
      return s_idGenerator.Next();
    }

    /// <summary>Create an exisiting MailMessage</summary>
    public MailMessage()
    {
    }

    /// <summary>Create a new MailMessage</summary>
    public MailMessage(string subject, string body)
    {
      Guid = NextId();
      TextId = (uint) MailMgr.TextIdGenerator.Next();
      Subject = subject;
      Body = body;
    }

    public CharacterRecord Recipient { get; set; }

    [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
    public long Guid { get; set; }

    [Version(UnsavedValue = "null")]
    public DateTime? LastModifiedOn { get; set; }

    [Property(NotNull = true)]
    public MailType MessageType { get; set; }

    [Property(NotNull = true)]
    public MailStationary MessageStationary { get; set; }

    public uint ReceiverId
    {
      get { return (uint) _receiverId; }
      set { _receiverId = (int) value; }
    }

    public uint SenderId
    {
      get { return (uint) _senderId; }
      set { _senderId = (int) value; }
    }

    public EntityId ReceiverEntityId
    {
      get { return EntityId.GetPlayerId((uint) _receiverId); }
      set { _receiverId = (int) value.Low; }
    }

    public EntityId SenderEntityId
    {
      get { return EntityId.GetPlayerId((uint) _senderId); }
      set { _senderId = (int) value.Low; }
    }

    [Property(Length = 512, NotNull = true)]
    public string Subject { get; set; }

    /// <summary>The body of the message</summary>
    [Property(Length = 8192, NotNull = true)]
    public string Body { get; set; }

    public uint TextId
    {
      get { return (uint) _TextId; }
      set { _TextId = (int) value; }
    }

    [Property("TextId", NotNull = true)]
    public int _TextId { get; set; }

    [Property(NotNull = true)]
    public DateTime SendTime { get; set; }

    [Property(NotNull = true)]
    public DateTime DeliveryTime { get; set; }

    public int RemainingDeliveryMillis
    {
      get { return (int) (DeliveryTime - DateTime.Now).TotalMilliseconds; }
    }

    public bool WasRead
    {
      get { return ReadTime.HasValue; }
    }

    [Property]
    public DateTime? ReadTime { get; set; }

    [Property(NotNull = true)]
    public DateTime ExpireTime { get; set; }

    [Property]
    public DateTime? DeletedTime { get; set; }

    public uint IncludedMoney
    {
      get { return (uint) _includedMoney; }
      set { _includedMoney = value; }
    }

    public uint CashOnDelivery
    {
      get { return (uint) _cashOnDelivery; }
      set { _cashOnDelivery = value; }
    }

    [Property(NotNull = true)]
    public bool CopiedToItem { get; internal set; }

    [Property]
    public int IncludedItemCount { get; private set; }

    /// <summary>
    /// The list of included items.
    /// Returns null if no items are added.
    /// </summary>
    public ICollection<ItemRecord> IncludedItems
    {
      get { return null; }
    }

    public ItemRecord AddItem(Asda2ItemId item)
    {
      ItemTemplate template = ItemMgr.GetTemplate(item);
      if(template == null)
        return null;
      return AddItem(template);
    }

    public ItemRecord AddItem(ItemTemplate template)
    {
      ItemRecord record = ItemRecord.CreateRecord(template);
      AddItem(record);
      return record;
    }

    public void AddItem(ItemRecord record)
    {
      if(_items == null)
        _items = new List<ItemRecord>(3);
      record.MailId = Guid;
      _items.Add(record);
      ++IncludedItemCount;
    }

    public ItemRecord RemoveItem(uint lowId)
    {
      ICollection<ItemRecord> includedItems = IncludedItems;
      if(includedItems != null)
      {
        foreach(ItemRecord itemRecord in includedItems)
        {
          if((int) itemRecord.EntityLowId == (int) lowId)
          {
            includedItems.Remove(itemRecord);
            IncludedItemCount = includedItems.Count;
            return itemRecord;
          }
        }
      }

      return null;
    }

    public void SetItems(ICollection<Item> items)
    {
      if(_items != null)
        throw new InvalidOperationException("Tried to set Items after Items were already set: " +
                                            this);
      _items = new List<ItemRecord>(items.Count);
      foreach(Item obj in items)
      {
        obj.Remove(true);
        obj.Record.MailId = Guid;
        _items.Add(obj.Record);
      }

      IncludedItemCount = _items.Count;
    }

    public void SetItems(ICollection<ItemRecord> items)
    {
      if(_items != null)
        throw new InvalidOperationException("Tried to set Items after Items were already set: " +
                                            this);
      _items = new List<ItemRecord>(items.Count);
      foreach(ItemRecord itemRecord in items)
      {
        itemRecord.MailId = Guid;
        _items.Add(itemRecord);
      }

      IncludedItemCount = _items.Count;
    }

    internal void PutBack(ItemRecord item)
    {
      _items.Add(item);
      ++IncludedItemCount;
    }

    public bool IsDeleted
    {
      get
      {
        if(DeletedTime.HasValue)
          return DeletedTime.Value < DateTime.Now;
        return false;
      }
    }

    public override void Create()
    {
      if(_items != null)
      {
        foreach(ActiveRecordBase activeRecordBase in _items)
          activeRecordBase.Save();
      }

      base.Create();
    }

    public override void Update()
    {
      if(_items != null)
      {
        foreach(ActiveRecordBase activeRecordBase in _items)
          activeRecordBase.Save();
      }

      base.Update();
    }

    public override void Save()
    {
      if(_items != null)
      {
        foreach(ActiveRecordBase activeRecordBase in _items)
          activeRecordBase.Save();
      }

      base.Save();
    }

    /// <summary>Delete letter and all containing Items</summary>
    public void Destroy()
    {
      if(IncludedItemCount > 0)
      {
        ActiveRecordBase<ItemRecord>.DeleteAll("MailId = " + Guid);
        _items = null;
      }

      Delete();
    }

    /// <summary>
    /// Returns to sender or Destroys the mail (if sender doesn't exist)
    /// </summary>
    public void ReturnToSender()
    {
      ServerApp<RealmServer>.IOQueue.ExecuteInContext(() =>
      {
        if(!CharacterRecord.Exists(SenderId))
        {
          Destroy();
        }
        else
        {
          SenderId = ReceiverId;
          ReceiverId = SenderId;
          Subject += " [Returned]";
          Send();
        }
      });
    }

    public void Send()
    {
      MailMgr.SendMail(this);
    }

    public static IEnumerable<MailMessage> FindAllMessagesFor(uint charLowId)
    {
      return FindAllByProperty("_receiverId",
        (int) charLowId);
    }
  }
}
using System;
using System.Collections.Generic;
using Castle.ActiveRecord;
using NHibernate;
using NLog;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Core;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.Logs;
using WCell.RealmServer.Mail;
using WCell.Util.NLog;

namespace WCell.RealmServer.Asda2Mail
{
	[ActiveRecord(Access = PropertyAccess.Property)]
    public class Asda2MailMessage : ActiveRecordBase<Asda2MailMessage>
	{
        [Property]
	    public string Subject { get; set; }
        [Property]
        public string Body { get; set; }
        public Asda2Item Item { get; set; }
        [Property]
        public uint Gold { get; set; }
        [Property]
	    public uint RecieverId { get; set; }
        [Property]
        public bool IsReaded { get; set; }
	    [Property]
        public long ItemGuid { get; set; }
	    private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

        private static readonly NHIdGenerator s_idGenerator = new NHIdGenerator(typeof(Asda2MailMessage), "Guid");

		/// <summary>
		/// Returns the next unique Id for a new Item
		/// </summary>
		public static long NextId()
		{
			return s_idGenerator.Next();
		}

		
		/// <summary>
		/// Create an exisiting MailMessage
		/// </summary>
		public Asda2MailMessage()
		{
		}

		/// <summary>
		/// Create a new MailMessage
		/// </summary>
        public Asda2MailMessage(string subject, string body, Asda2Item item, uint gold, uint recieverId, string senderName)
		{
		    Subject = subject;
		    Body = body;
		    Item = item;
		    Gold = gold;
		    RecieverId = recieverId;
		    ItemGuid = item ==null?-1: item.Record.Guid;
		    SenderName = senderName;
		    DeleteTime = DateTime.Now.AddDays(10);
		    Guid = NextId();
		}

	    [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
		public long Guid
		{
			get;
			set;
		}
        [Property]
	    public DateTime DeleteTime { get; set; }
        [Property]
	    public string SenderName { get; set; }

	    public static Asda2MailMessage[] LoadAll(Character chr)
		{
		    var msgs = FindAllByProperty("RecieverId", chr.EntityId.Low);
		    foreach (var asda2MailMessage in msgs)
		    {
		        asda2MailMessage.Init();
		    }
		    return msgs;
		}

	    private void Init()
	    {
	        if (ItemGuid <= 0) return;
	        Asda2ItemRecord itemRec;
	        try
	        {
	            itemRec = Asda2ItemRecord.Find(ItemGuid);
	        }
	        catch (NotFoundException)
	        {
	            LogUtil.WarnException(string.Format("Mail message {0} failed to load cause item {1} not founded. Mail message deleted.",Guid,ItemGuid));
	            ItemGuid = -1;
	            this.SaveLater();
	            return;
	        }
	        Item = Asda2Item.CreateItem(itemRec,(Character) null);
	    }
	}
}
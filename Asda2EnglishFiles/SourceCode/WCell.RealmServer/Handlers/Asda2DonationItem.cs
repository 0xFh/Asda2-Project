using Castle.ActiveRecord;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Handlers
{
    [Castle.ActiveRecord.ActiveRecord(Access = PropertyAccess.Property)]
    public class Asda2DonationItem : ActiveRecordBase<Asda2DonationItem>
    {
        private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

        private static readonly NHIdGenerator s_idGenerator =
            new NHIdGenerator(typeof(Asda2DonationItem), nameof(Guid), 1L);

        [Property] public int ItemId { get; set; }

        [Property] public int Amount { get; set; }

        [Property] public uint RecieverId { get; set; }

        [Property] public string Creator { get; set; }

        [Property] public bool IsSoulBound { get; set; }

        /// <summary>Returns the next unique Id for a new Item</summary>
        public static long NextId()
        {
            return Asda2DonationItem.s_idGenerator.Next();
        }

        /// <summary>Create an exisiting MailMessage</summary>
        public Asda2DonationItem()
        {
        }

        /// <summary>Create a new Donation item</summary>
        public Asda2DonationItem(uint recieverId, int itemId, int amount, string name, bool isSoulBound)
        {
            this.RecieverId = recieverId;
            this.ItemId = itemId;
            this.Amount = amount;
            this.Created = DateTime.Now;
            this.Guid = (int) Asda2DonationItem.NextId();
            this.Creator = name;
            this.IsSoulBound = isSoulBound;
        }

        [PrimaryKey(PrimaryKeyType.Assigned, "Guid")]
        public int Guid { get; set; }

        [Property] public DateTime Created { get; set; }

        [Property] public bool Recived { get; set; }

        public static Asda2DonationItem[] LoadAll(Character chr)
        {
            Asda2DonationItem[] array =
                ((IEnumerable<Asda2DonationItem>) ActiveRecordBase<Asda2DonationItem>.FindAllByProperty("RecieverId",
                    (object) chr.EntryId)).Where<Asda2DonationItem>((Func<Asda2DonationItem, bool>) (d => !d.Recived))
                .ToArray<Asda2DonationItem>();
            foreach (Asda2DonationItem asda2DonationItem in array)
                asda2DonationItem.Init();
            return array;
        }

        private void Init()
        {
        }
    }
}
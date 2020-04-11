using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.RealmServer.Database;
using WCell.RealmServer.Mail;

namespace WCell.RealmServer.NPCs.Auctioneer
{
    [Castle.ActiveRecord.ActiveRecord]
    public class Auction : ActiveRecordBase<Auction>
    {
        /// <summary>
        /// TODO: NPC entityid must not be saved to DB
        /// TODO: Don't save uints as long - int is fine
        /// </summary>
        [Field("AuctionId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _auctionId;

        [Field("ItemTemplateId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _itemTemplateId;

        [Field("OwnerEntityLowId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _ownerEntityLowId;

        [Field("BidderEntityLowId", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _bidderEntityLowId;

        [Field("CurrentBid", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _currentBid;

        [Field("BuyoutPrice", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _buyoutPrice;

        [Field("Deposit", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _deposit;

        [Field("AuctionHouseFaction", Access = PropertyAccess.FieldCamelcase, NotNull = true)]
        private int _houseFaction;

        private bool _isNew;

        [Property(NotNull = true)] public DateTime TimeEnds { get; set; }

        [PrimaryKey(PrimaryKeyType.Assigned, "ItemEntityLowId")]
        private int _ItemEntityLowId { get; set; }

        public AuctionHouseFaction HouseFaction
        {
            get { return (AuctionHouseFaction) Enum.ToObject(typeof(AuctionHouseFaction), this._houseFaction); }
            set { this._houseFaction = (int) value; }
        }

        public uint AuctionId
        {
            get { return (uint) this._auctionId; }
        }

        public uint ItemTemplateId
        {
            get { return (uint) this._itemTemplateId; }
            set { this._itemTemplateId = (int) value; }
        }

        public uint ItemLowId
        {
            get { return (uint) this._ItemEntityLowId; }
            set
            {
                this._auctionId = (int) value;
                this._ItemEntityLowId = (int) value;
            }
        }

        public uint OwnerLowId
        {
            get { return (uint) this._ownerEntityLowId; }
            set { this._ownerEntityLowId = (int) value; }
        }

        public uint BidderLowId
        {
            get { return (uint) this._bidderEntityLowId; }
            set { this._bidderEntityLowId = (int) value; }
        }

        public uint CurrentBid
        {
            get { return (uint) this._currentBid; }
            set { this._currentBid = (int) value; }
        }

        public uint BuyoutPrice
        {
            get { return (uint) this._buyoutPrice; }
            set { this._buyoutPrice = (int) value; }
        }

        public uint Deposit
        {
            get { return (uint) this._deposit; }
            set { this._deposit = (int) value; }
        }

        public bool IsNew
        {
            get { return this._isNew; }
            set { this._isNew = value; }
        }

        public void SendMail(MailAuctionAnswers response, uint money)
        {
            this.SendMail(string.Format("{0}:0:{1}", (object) this.ItemTemplateId, (object) response), money,
                (ItemRecord) null, "");
        }

        public void SendMail(MailAuctionAnswers response, uint money, string body)
        {
            this.SendMail(string.Format("{0}:0:{1}", (object) this.ItemTemplateId, (object) response), money,
                (ItemRecord) null, body);
        }

        public void SendMail(MailAuctionAnswers response, ItemRecord item)
        {
            this.SendMail(string.Format("{0}:0:{1}", (object) this.ItemTemplateId, (object) response), 0U, item, "");
        }

        public void SendMail(MailAuctionAnswers response, uint money, ItemRecord item, string body)
        {
            this.SendMail(string.Format("{0}:0:{1}", (object) this.ItemTemplateId, (object) response), money, item,
                body);
        }

        public void SendMail(string subject, uint money, ItemRecord item, string body)
        {
            MailMessage mailMessage = new MailMessage(subject, body)
            {
                SenderId = (uint) this.HouseFaction,
                ReceiverId = this.OwnerLowId,
                MessageStationary = MailStationary.Auction,
                MessageType = MailType.Auction,
                IncludedMoney = money,
                LastModifiedOn = new DateTime?(),
                SendTime = DateTime.Now,
                DeliveryTime = DateTime.Now
            };
            if (item != null)
                mailMessage.AddItem(item);
            mailMessage.Send();
        }

        public static IEnumerable<Auction> GetAffiliatedAuctions(AuctionHouseFaction houseFaction)
        {
            return (IEnumerable<Auction>) ActiveRecordBase<Auction>.FindAllByProperty("_houseFaction",
                (object) (int) houseFaction);
        }

        public static IEnumerable<Auction> GetAuctionsForCharacter(uint charLowId)
        {
            return (IEnumerable<Auction>) ActiveRecordBase<Auction>.FindAllByProperty("_ownerEntityLowId",
                (object) (int) charLowId);
        }

        public static IEnumerable<Auction> GetBidderAuctionsForCharacter(uint charLowId)
        {
            return (IEnumerable<Auction>) ActiveRecordBase<Auction>.FindAllByProperty("_bidderEntityLowId",
                (object) (int) charLowId);
        }
    }
}
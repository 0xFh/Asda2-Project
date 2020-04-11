using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.RealmServer.Database;
using WCell.RealmServer.Mail;

namespace WCell.RealmServer.NPCs.Auctioneer
{
  [ActiveRecord]
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

    [Property(NotNull = true)]
    public DateTime TimeEnds { get; set; }

    [PrimaryKey(PrimaryKeyType.Assigned, "ItemEntityLowId")]
    private int _ItemEntityLowId { get; set; }

    public AuctionHouseFaction HouseFaction
    {
      get { return (AuctionHouseFaction) Enum.ToObject(typeof(AuctionHouseFaction), _houseFaction); }
      set { _houseFaction = (int) value; }
    }

    public uint AuctionId
    {
      get { return (uint) _auctionId; }
    }

    public uint ItemTemplateId
    {
      get { return (uint) _itemTemplateId; }
      set { _itemTemplateId = (int) value; }
    }

    public uint ItemLowId
    {
      get { return (uint) _ItemEntityLowId; }
      set
      {
        _auctionId = (int) value;
        _ItemEntityLowId = (int) value;
      }
    }

    public uint OwnerLowId
    {
      get { return (uint) _ownerEntityLowId; }
      set { _ownerEntityLowId = (int) value; }
    }

    public uint BidderLowId
    {
      get { return (uint) _bidderEntityLowId; }
      set { _bidderEntityLowId = (int) value; }
    }

    public uint CurrentBid
    {
      get { return (uint) _currentBid; }
      set { _currentBid = (int) value; }
    }

    public uint BuyoutPrice
    {
      get { return (uint) _buyoutPrice; }
      set { _buyoutPrice = (int) value; }
    }

    public uint Deposit
    {
      get { return (uint) _deposit; }
      set { _deposit = (int) value; }
    }

    public bool IsNew
    {
      get { return _isNew; }
      set { _isNew = value; }
    }

    public void SendMail(MailAuctionAnswers response, uint money)
    {
      SendMail(string.Format("{0}:0:{1}", ItemTemplateId, response), money,
        null, "");
    }

    public void SendMail(MailAuctionAnswers response, uint money, string body)
    {
      SendMail(string.Format("{0}:0:{1}", ItemTemplateId, response), money,
        null, body);
    }

    public void SendMail(MailAuctionAnswers response, ItemRecord item)
    {
      SendMail(string.Format("{0}:0:{1}", ItemTemplateId, response), 0U, item, "");
    }

    public void SendMail(MailAuctionAnswers response, uint money, ItemRecord item, string body)
    {
      SendMail(string.Format("{0}:0:{1}", ItemTemplateId, response), money, item,
        body);
    }

    public void SendMail(string subject, uint money, ItemRecord item, string body)
    {
      MailMessage mailMessage = new MailMessage(subject, body)
      {
        SenderId = (uint) HouseFaction,
        ReceiverId = OwnerLowId,
        MessageStationary = MailStationary.Auction,
        MessageType = MailType.Auction,
        IncludedMoney = money,
        LastModifiedOn = new DateTime?(),
        SendTime = DateTime.Now,
        DeliveryTime = DateTime.Now
      };
      if(item != null)
        mailMessage.AddItem(item);
      mailMessage.Send();
    }

    public static IEnumerable<Auction> GetAffiliatedAuctions(AuctionHouseFaction houseFaction)
    {
      return FindAllByProperty("_houseFaction",
        (int) houseFaction);
    }

    public static IEnumerable<Auction> GetAuctionsForCharacter(uint charLowId)
    {
      return FindAllByProperty("_ownerEntityLowId",
        (int) charLowId);
    }

    public static IEnumerable<Auction> GetBidderAuctionsForCharacter(uint charLowId)
    {
      return FindAllByProperty("_bidderEntityLowId",
        (int) charLowId);
    }
  }
}
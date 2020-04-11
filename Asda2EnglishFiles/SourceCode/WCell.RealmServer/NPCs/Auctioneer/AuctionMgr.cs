using System;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Network;
using WCell.Util.Collections;

namespace WCell.RealmServer.NPCs.Auctioneer
{
    public class AuctionMgr : Manager<AuctionMgr>
    {
        /// <summary>
        /// The percent of the auctioned item's value used to calculate the required auctionhouse deposit.
        /// This value is used for Alliance/Horde auctionhouses only.
        /// </summary>
        public static uint FactionHouseDepositPercent = 15;

        /// <summary>
        /// The percent of the auctioned item's value used to calculate the required auctionhouse deposit.
        /// This value is used for Neutral auctionhouses only.
        /// </summary>
        public static uint NeutralHouseDepositPercent = 75;

        /// <summary>
        /// The multiplier used to calculate the cut the auctionhouse takes from your auction winnings.
        /// Default = 1.00
        /// </summary>
        public static float AuctionHouseCutRate = 1f;

        /// <summary>
        /// Whether Alliance characters can list on Horde Auctionhouses and vice-versa.
        /// </summary>
        public static bool AllowInterFactionAuctions;

        /// <summary>All auctions listed in the Alliance Auctionhouses.</summary>
        public AuctionHouse AllianceAuctions;

        /// <summary>All auctions listed in the Horde Auctionhouses.</summary>
        public AuctionHouse HordeAuctions;

        /// <summary>All auctions listed in the Neutral auctionhouses.</summary>
        public AuctionHouse NeutralAuctions;

        private SynchronizedDictionary<uint, ItemRecord> _auctionedItems;
        private bool _hasItemLoaded;

        [WCell.Core.Initialization.Initialization(InitializationPass.Fifth, "Initialize Auctions")]
        public static void Initialize()
        {
            Singleton<AuctionMgr>.Instance.Start();
        }

        protected bool Start()
        {
            this._auctionedItems = new SynchronizedDictionary<uint, ItemRecord>(10000);
            if (AuctionMgr.AllowInterFactionAuctions)
            {
                this.NeutralAuctions = new AuctionHouse();
                this.AllianceAuctions = this.NeutralAuctions;
                this.HordeAuctions = this.NeutralAuctions;
            }
            else
            {
                this.AllianceAuctions = new AuctionHouse();
                this.HordeAuctions = new AuctionHouse();
                this.NeutralAuctions = new AuctionHouse();
            }

            this.FetchAuctions();
            return true;
        }

        protected AuctionMgr()
        {
        }

        private void FetchAuctions()
        {
            if (AuctionMgr.AllowInterFactionAuctions)
            {
                foreach (Auction affiliatedAuction in Auction.GetAffiliatedAuctions(AuctionHouseFaction.Neutral))
                    this.NeutralAuctions.AddAuction(affiliatedAuction);
            }
            else
            {
                foreach (Auction affiliatedAuction in Auction.GetAffiliatedAuctions(AuctionHouseFaction.Alliance))
                    this.AllianceAuctions.AddAuction(affiliatedAuction);
                foreach (Auction affiliatedAuction in Auction.GetAffiliatedAuctions(AuctionHouseFaction.Horde))
                    this.HordeAuctions.AddAuction(affiliatedAuction);
                foreach (Auction affiliatedAuction in Auction.GetAffiliatedAuctions(AuctionHouseFaction.Neutral))
                    this.NeutralAuctions.AddAuction(affiliatedAuction);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void LoadItems()
        {
            if (this._hasItemLoaded)
                return;
            try
            {
                foreach (ItemRecord loadAuctionedItem in ItemRecord.LoadAuctionedItems())
                    this._auctionedItems.Add(loadAuctionedItem.EntityLowId, loadAuctionedItem);
                this._hasItemLoaded = true;
            }
            catch (Exception ex)
            {
                RealmDBMgr.OnDBError(ex);
                foreach (ItemRecord loadAuctionedItem in ItemRecord.LoadAuctionedItems())
                    this._auctionedItems.Add(loadAuctionedItem.EntityLowId, loadAuctionedItem);
                this._hasItemLoaded = true;
            }
        }

        public SynchronizedDictionary<uint, ItemRecord> AuctionItems
        {
            get { return this._auctionedItems; }
        }

        public bool HasItem(uint itemid)
        {
            return this._auctionedItems.ContainsKey(itemid);
        }

        public bool HasItemLoaded
        {
            get { return this._hasItemLoaded; }
            set { this._hasItemLoaded = value; }
        }

        public void RemoveAuction(Auction auction)
        {
            this.AllianceAuctions.RemoveAuction(auction);
            this.HordeAuctions.RemoveAuction(auction);
            this.NeutralAuctions.RemoveAuction(auction);
        }

        public void AuctionHello(Character chr, NPC auctioneer)
        {
            if (!AuctionMgr.DoAuctioneerInteraction(chr, auctioneer))
                return;
            AuctionHandler.SendAuctionHello((IPacketReceiver) chr.Client, auctioneer);
        }

        public void AuctionSellItem(Character chr, NPC auctioneer, EntityId itemId, uint bid, uint buyout, uint time,
            uint stackSize)
        {
        }

        public void AuctionPlaceBid(Character chr, NPC auctioneer, uint auctionId, uint bid)
        {
            if (!AuctionMgr.DoAuctioneerInteraction(chr, auctioneer))
                return;
            Auction auction = (Auction) null;
            if (!auctioneer.AuctioneerEntry.Auctions.TryGetAuction(auctionId, out auction))
            {
                AuctionHandler.SendAuctionCommandResult((IPacketReceiver) chr.Client, (Auction) null,
                    AuctionAction.PlaceBid, AuctionError.InternalError);
            }
            else
            {
                AuctionError error = this.AuctionBidChecks(auction, chr, bid);
                if (error != AuctionError.Ok)
                    AuctionHandler.SendAuctionCommandResult((IPacketReceiver) chr.Client, auction,
                        AuctionAction.PlaceBid, error);
                else if (bid < auction.BuyoutPrice || auction.BuyoutPrice == 0U)
                {
                    if ((long) auction.BidderLowId == (long) (ulong) chr.EntityId)
                    {
                        chr.SubtractMoney(bid - auction.CurrentBid);
                    }
                    else
                    {
                        chr.SubtractMoney(bid);
                        AuctionMgr.SendOutbidMail(auction, bid);
                    }

                    auction.BidderLowId = chr.EntityId.Low;
                    auction.CurrentBid = bid;
                    AuctionHandler.SendAuctionCommandResult((IPacketReceiver) chr.Client, auction,
                        AuctionAction.PlaceBid, AuctionError.Ok);
                }
                else
                {
                    if ((long) auction.BidderLowId == (long) (ulong) chr.EntityId)
                    {
                        chr.SubtractMoney(auction.BuyoutPrice - auction.CurrentBid);
                    }
                    else
                    {
                        chr.SubtractMoney(auction.BuyoutPrice);
                        if ((int) auction.BidderLowId != (int) auction.OwnerLowId)
                            AuctionMgr.SendOutbidMail(auction, auction.BuyoutPrice);
                    }

                    auction.BidderLowId = chr.EntityId.Low;
                    auction.CurrentBid = auction.BuyoutPrice;
                    AuctionMgr.SendAuctionSuccessfullMail(auction);
                    AuctionMgr.SendAuctionWonMail(auction);
                    AuctionHandler.SendAuctionCommandResult((IPacketReceiver) chr.Client, auction,
                        AuctionAction.PlaceBid, AuctionError.Ok);
                    auctioneer.AuctioneerEntry.Auctions.RemoveAuction(auction);
                }
            }
        }

        public void CancelAuction(Character chr, NPC auctioneer, uint auctionId)
        {
            if (!AuctionMgr.DoAuctioneerInteraction(chr, auctioneer))
                return;
            Auction auction;
            if (auctioneer.AuctioneerEntry.Auctions.TryGetAuction(auctionId, out auction))
            {
                if ((int) auction.OwnerLowId == (int) chr.EntityId.Low)
                {
                    ItemRecord recordById = ItemRecord.GetRecordByID(auction.ItemLowId);
                    if (recordById != null)
                    {
                        if ((int) auction.BidderLowId != (int) auction.OwnerLowId)
                        {
                            uint amount = AuctionMgr.CalcAuctionCut(auction.HouseFaction, auction.CurrentBid);
                            if (chr.Money < amount)
                            {
                                AuctionHandler.SendAuctionCommandResult((IPacketReceiver) chr.Client, auction,
                                    AuctionAction.CancelAuction, AuctionError.NotEnoughMoney);
                                return;
                            }

                            auction.SendMail(MailAuctionAnswers.CancelledToBidder, auction.CurrentBid);
                            chr.SubtractMoney(amount);
                        }

                        auction.SendMail(MailAuctionAnswers.Cancelled, recordById);
                        auctioneer.AuctioneerEntry.Auctions.RemoveAuction(auction);
                        AuctionHandler.SendAuctionCommandResult((IPacketReceiver) chr.Client, auction,
                            AuctionAction.CancelAuction, AuctionError.Ok);
                        auctioneer.AuctioneerEntry.Auctions.RemoveAuction(auction);
                    }
                    else
                        AuctionHandler.SendAuctionCommandResult((IPacketReceiver) chr.Client, auction,
                            AuctionAction.CancelAuction, AuctionError.ItemNotFound);
                }
                else
                    AuctionHandler.SendAuctionCommandResult((IPacketReceiver) chr.Client, auction,
                        AuctionAction.CancelAuction, AuctionError.ItemNotFound);
            }
            else
                AuctionHandler.SendAuctionCommandResult((IPacketReceiver) chr.Client, auction,
                    AuctionAction.CancelAuction, AuctionError.ItemNotFound);
        }

        public void AuctionListOwnerItems(Character chr, NPC auctioneer)
        {
            if (!AuctionMgr.DoAuctioneerInteraction(chr, auctioneer))
                return;
            Auction[] array = Auction.GetAuctionsForCharacter(chr.EntityId.Low).ToArray<Auction>();
            AuctionHandler.SendAuctionListOwnerItems((IPacketReceiver) chr.Client, array);
        }

        public void AuctionListBidderItems(Character chr, NPC auctioneer)
        {
            if (!AuctionMgr.DoAuctioneerInteraction(chr, auctioneer))
                return;
            Auction[] array = Auction.GetBidderAuctionsForCharacter(chr.EntityId.Low).ToArray<Auction>();
            AuctionHandler.SendAuctionListBidderItems((IPacketReceiver) chr.Client, array);
        }

        public void AuctionListItems(Character chr, NPC auctioneer, AuctionSearch searcher)
        {
            if (!AuctionMgr.DoAuctioneerInteraction(chr, auctioneer))
                return;
            Auction[] array = searcher.RetrieveMatchedAuctions(auctioneer.AuctioneerEntry.Auctions).ToArray<Auction>();
            AuctionHandler.SendAuctionListItems((IPacketReceiver) chr.Client, array);
        }

        private static void SendOutbidMail(Auction auction, uint newBid)
        {
            if (auction == null)
                return;
            Character character = World.GetCharacter(auction.BidderLowId);
            if (character != null)
                AuctionHandler.SendAuctionOutbidNotification((IPacketReceiver) character.Client, auction, newBid,
                    AuctionMgr.GetMinimumNewBidIncrement(auction));
            auction.SendMail(MailAuctionAnswers.Outbid, auction.CurrentBid);
        }

        private static void SendAuctionSuccessfullMail(Auction auction)
        {
            if (auction == null)
                return;
            uint num = AuctionMgr.CalcAuctionCut(auction.HouseFaction, auction.CurrentBid);
            string body = string.Format("{0,16:X}:{1,16:D}:0:{2,16:D}:{3,16:D}", (object) auction.BidderLowId,
                (object) auction.CurrentBid, (object) auction.Deposit, (object) num);
            uint money = auction.CurrentBid + auction.Deposit - num;
            auction.SendMail(MailAuctionAnswers.Successful, money, body);
        }

        private static void SendAuctionWonMail(Auction auction)
        {
            if (auction == null)
                return;
            string body = string.Format("{0,16:X}:{1,16:D}:{2,16:D}", (object) auction.OwnerLowId,
                (object) auction.CurrentBid, (object) auction.BuyoutPrice);
            ItemRecord recordById = ItemRecord.GetRecordByID(auction.ItemLowId);
            if (recordById == null)
                return;
            auction.SendMail(MailAuctionAnswers.Won, 0U, recordById, body);
        }

        private static bool DoAuctioneerInteraction(Character chr, NPC auctioneer)
        {
            if (!auctioneer.IsAuctioneer || !auctioneer.CheckVendorInteraction(chr))
                return false;
            chr.Auras.RemoveByFlag(AuraInterruptFlags.OnStartAttack);
            return true;
        }

        private AuctionError AuctionCheatChecks(NPC auctioneer, Item item, uint bid, uint time)
        {
            if (bid == 0U || time == 0U)
                return AuctionError.InternalError;
            if (item == null)
                return AuctionError.ItemNotFound;
            return this.IsAlreadyAuctioned(auctioneer, (ILootable) item) ||
                   item.IsContainer && !((Container) item).BaseInventory.IsEmpty ||
                   (!item.CanBeTraded || item.Duration > 0U || item.IsConjured)
                ? AuctionError.InternalError
                : AuctionError.Ok;
        }

        private AuctionError AuctionBidChecks(Auction auction, Character chr, uint bid)
        {
            if ((long) auction.OwnerLowId == (long) (ulong) chr.EntityId ||
                !chr.GodMode && chr.Account.GetCharacterRecord(auction.OwnerLowId) != null)
                return AuctionError.CannotBidOnOwnAuction;
            return bid < AuctionMgr.GetMinimumNewBid(auction) || chr.Money < bid
                ? AuctionError.InternalError
                : AuctionError.Ok;
        }

        private bool IsAlreadyAuctioned(NPC auctioneer, ILootable item)
        {
            if (item == null)
                return true;
            return this.AuctionItems.ContainsKey(item.EntityId.Low);
        }

        private static uint GetAuctionDeposit(Item item, AuctionHouseFaction houseFaction, uint timeInMin)
        {
            if (item == null)
                return 0;
            uint houseDepositPercent = AuctionMgr.FactionHouseDepositPercent;
            if (houseFaction == AuctionHouseFaction.Neutral && !AuctionMgr.AllowInterFactionAuctions)
                houseDepositPercent = AuctionMgr.NeutralHouseDepositPercent;
            return item.Template.SellPrice * (uint) item.Amount * houseDepositPercent / 100U * (timeInMin / 720U);
        }

        public static uint GetMinimumNewBid(Auction auction)
        {
            return auction.CurrentBid + AuctionMgr.GetMinimumNewBidIncrement(auction);
        }

        public static uint GetMinimumNewBidIncrement(Auction auction)
        {
            uint num = 0;
            if (auction.CurrentBid > 0U)
                num = Math.Max(1U, auction.CurrentBid / 100U * 5U);
            return num;
        }

        private static uint CalcAuctionCut(AuctionHouseFaction houseFaction, uint bid)
        {
            if (houseFaction == AuctionHouseFaction.Neutral && !AuctionMgr.AllowInterFactionAuctions)
                return (uint) (0.150000005960464 * (double) bid * (double) AuctionMgr.AuctionHouseCutRate);
            return (uint) (0.0500000007450581 * (double) bid * (double) AuctionMgr.AuctionHouseCutRate);
        }
    }
}
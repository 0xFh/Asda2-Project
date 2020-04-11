using System;
using WCell.Core;
using WCell.RealmServer.Database;
using WCell.Util.Collections;

namespace WCell.RealmServer.NPCs.Auctioneer
{
    public class AuctionHouse
    {
        private readonly SynchronizedDictionary<uint, Auction> auctions;
        private readonly SynchronizedList<uint> items;

        public SynchronizedDictionary<uint, Auction> Auctions
        {
            get { return this.auctions; }
        }

        public AuctionHouse()
        {
            this.auctions = new SynchronizedDictionary<uint, Auction>(10000);
            this.items = new SynchronizedList<uint>(10000);
        }

        public void AddAuction(Auction newAuction)
        {
            if (newAuction == null)
                return;
            this.auctions.Add(newAuction.ItemLowId, newAuction);
            this.items.Add(newAuction.ItemLowId);
            if (!newAuction.IsNew)
                return;
            newAuction.Create();
        }

        public void RemoveAuction(Auction auction)
        {
            if (auction == null || !this.auctions.ContainsKey(auction.AuctionId))
                return;
            this.auctions.Remove(auction.ItemLowId);
            this.items.Remove(auction.ItemLowId);
            AuctionMgr instance = Singleton<AuctionMgr>.Instance;
            ItemRecord record = (ItemRecord) null;
            if (instance.AuctionItems.ContainsKey(auction.ItemLowId))
            {
                record = instance.AuctionItems[auction.ItemLowId];
                instance.AuctionItems.Remove(auction.ItemLowId);
            }

            ServerApp<WCell.RealmServer.RealmServer>.IOQueue.AddMessage((Action) (() =>
            {
                if (record != null)
                {
                    record.IsAuctioned = false;
                    record.Save();
                }

                auction.Delete();
            }));
        }

        public void RemoveAuctionById(uint auctionId)
        {
            Auction auction;
            if (!this.TryGetAuction(auctionId, out auction))
                return;
            this.RemoveAuction(auction);
        }

        public bool HasItemById(uint itemEntityLowId)
        {
            return Singleton<AuctionMgr>.Instance.AuctionItems.ContainsKey(itemEntityLowId);
        }

        public bool TryGetAuction(uint auctionId, out Auction auction)
        {
            return this.auctions.TryGetValue(auctionId, out auction);
        }
    }
}
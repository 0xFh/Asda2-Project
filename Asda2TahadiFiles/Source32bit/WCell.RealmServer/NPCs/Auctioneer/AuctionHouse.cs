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
      get { return auctions; }
    }

    public AuctionHouse()
    {
      auctions = new SynchronizedDictionary<uint, Auction>(10000);
      items = new SynchronizedList<uint>(10000);
    }

    public void AddAuction(Auction newAuction)
    {
      if(newAuction == null)
        return;
      auctions.Add(newAuction.ItemLowId, newAuction);
      items.Add(newAuction.ItemLowId);
      if(!newAuction.IsNew)
        return;
      newAuction.Create();
    }

    public void RemoveAuction(Auction auction)
    {
      if(auction == null || !auctions.ContainsKey(auction.AuctionId))
        return;
      auctions.Remove(auction.ItemLowId);
      items.Remove(auction.ItemLowId);
      AuctionMgr instance = Singleton<AuctionMgr>.Instance;
      ItemRecord record = null;
      if(instance.AuctionItems.ContainsKey(auction.ItemLowId))
      {
        record = instance.AuctionItems[auction.ItemLowId];
        instance.AuctionItems.Remove(auction.ItemLowId);
      }

      ServerApp<RealmServer>.IOQueue.AddMessage(() =>
      {
        if(record != null)
        {
          record.IsAuctioned = false;
          record.Save();
        }

        auction.Delete();
      });
    }

    public void RemoveAuctionById(uint auctionId)
    {
      Auction auction;
      if(!TryGetAuction(auctionId, out auction))
        return;
      RemoveAuction(auction);
    }

    public bool HasItemById(uint itemEntityLowId)
    {
      return Singleton<AuctionMgr>.Instance.AuctionItems.ContainsKey(itemEntityLowId);
    }

    public bool TryGetAuction(uint auctionId, out Auction auction)
    {
      return auctions.TryGetValue(auctionId, out auction);
    }
  }
}
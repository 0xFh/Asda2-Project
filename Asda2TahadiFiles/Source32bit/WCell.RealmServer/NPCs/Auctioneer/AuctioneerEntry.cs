using WCell.Core;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.NPCs.Auctioneer
{
  public class AuctioneerEntry
  {
    public AuctionHouse Auctions;
    public AuctionHouseFaction LinkedHouseFaction;
    private readonly NPC npc;

    public AuctioneerEntry(NPC npc)
    {
      this.npc = npc;
      LinkAuctionSetter();
    }

    private void LinkAuctionSetter()
    {
      if(AuctionMgr.AllowInterFactionAuctions)
      {
        Auctions = Singleton<AuctionMgr>.Instance.NeutralAuctions;
        LinkedHouseFaction = AuctionHouseFaction.Neutral;
      }
      else if(npc.Faction.IsAlliance)
      {
        Auctions = Singleton<AuctionMgr>.Instance.AllianceAuctions;
        LinkedHouseFaction = AuctionHouseFaction.Alliance;
      }
      else if(npc.Faction.IsHorde)
      {
        Auctions = Singleton<AuctionMgr>.Instance.HordeAuctions;
        LinkedHouseFaction = AuctionHouseFaction.Horde;
      }
      else
      {
        Auctions = Singleton<AuctionMgr>.Instance.NeutralAuctions;
        LinkedHouseFaction = AuctionHouseFaction.Neutral;
      }
    }
  }
}
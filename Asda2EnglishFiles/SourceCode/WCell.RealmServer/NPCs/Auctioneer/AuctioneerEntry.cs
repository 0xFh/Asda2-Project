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
            this.LinkAuctionSetter();
        }

        private void LinkAuctionSetter()
        {
            if (AuctionMgr.AllowInterFactionAuctions)
            {
                this.Auctions = Singleton<AuctionMgr>.Instance.NeutralAuctions;
                this.LinkedHouseFaction = AuctionHouseFaction.Neutral;
            }
            else if (this.npc.Faction.IsAlliance)
            {
                this.Auctions = Singleton<AuctionMgr>.Instance.AllianceAuctions;
                this.LinkedHouseFaction = AuctionHouseFaction.Alliance;
            }
            else if (this.npc.Faction.IsHorde)
            {
                this.Auctions = Singleton<AuctionMgr>.Instance.HordeAuctions;
                this.LinkedHouseFaction = AuctionHouseFaction.Horde;
            }
            else
            {
                this.Auctions = Singleton<AuctionMgr>.Instance.NeutralAuctions;
                this.LinkedHouseFaction = AuctionHouseFaction.Neutral;
            }
        }
    }
}
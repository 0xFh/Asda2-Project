using System.Collections.Generic;
using WCell.Constants.Items;
using WCell.Core;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.NPCs.Auctioneer
{
    public class AuctionSearch
    {
        private uint m_maxResultCount;

        public uint MaxResultCount
        {
            get { return this.m_maxResultCount; }
            set { this.m_maxResultCount = value; }
        }

        public uint StartIndex { get; set; }

        public string Name { get; set; }

        public uint LevelRange1 { get; set; }

        public uint LevelRange2 { get; set; }

        public InventorySlotType InventoryType { get; set; }

        public ItemClass ItemClass { get; set; }

        public ItemSubClass ItemSubClass { get; set; }

        public int Quality { get; set; }

        public bool IsUsable { get; set; }

        public ICollection<Auction> RetrieveMatchedAuctions(AuctionHouse auctionHouse)
        {
            List<Auction> auctionList = new List<Auction>();
            foreach (Auction auction in auctionHouse.Auctions.Values)
            {
                ItemRecord auctionItem = Singleton<AuctionMgr>.Instance.AuctionItems[auction.ItemLowId];
                if (auctionItem != null && (string.IsNullOrEmpty(this.Name) ||
                                            auctionItem.Template.DefaultName.ToLower().Contains(this.Name.ToLower())))
                {
                    auctionList.Add(auction);
                    ++this.MaxResultCount;
                }
            }

            return (ICollection<Auction>) auctionList;
        }
    }
}
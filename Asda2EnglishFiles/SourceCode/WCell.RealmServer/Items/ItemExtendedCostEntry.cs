using System.Collections.Generic;
using WCell.Constants.Items;

namespace WCell.RealmServer.Items
{
    public class ItemExtendedCostEntry
    {
        public static ItemExtendedCostEntry NullEntry = new ItemExtendedCostEntry()
        {
            Id = 0,
            HonorCost = 0,
            ArenaPointCost = 0,
            RequiredItems = new List<ItemExtendedCostEntry.RequiredItem>(5)
            {
                new ItemExtendedCostEntry.RequiredItem()
                {
                    Id = Asda2ItemId.None,
                    Cost = 0
                },
                new ItemExtendedCostEntry.RequiredItem()
                {
                    Id = Asda2ItemId.None,
                    Cost = 0
                },
                new ItemExtendedCostEntry.RequiredItem()
                {
                    Id = Asda2ItemId.None,
                    Cost = 0
                },
                new ItemExtendedCostEntry.RequiredItem()
                {
                    Id = Asda2ItemId.None,
                    Cost = 0
                },
                new ItemExtendedCostEntry.RequiredItem()
                {
                    Id = Asda2ItemId.None,
                    Cost = 0
                }
            },
            ReqArenaRating = 0
        };

        public List<ItemExtendedCostEntry.RequiredItem> RequiredItems = new List<ItemExtendedCostEntry.RequiredItem>(5);
        public uint Id;
        public uint HonorCost;
        public uint ArenaPointCost;
        public uint ReqArenaRating;
        public uint Unk_322;

        public Asda2ItemId ReqItem1
        {
            get { return this.RequiredItems[0].Id; }
        }

        public Asda2ItemId ReqItem2
        {
            get { return this.RequiredItems[1].Id; }
        }

        public Asda2ItemId ReqItem3
        {
            get { return this.RequiredItems[2].Id; }
        }

        public Asda2ItemId ReqItem4
        {
            get { return this.RequiredItems[3].Id; }
        }

        public Asda2ItemId ReqItem5
        {
            get { return this.RequiredItems[4].Id; }
        }

        public int ReqItemAmt1
        {
            get { return this.RequiredItems[0].Cost; }
        }

        public int ReqItemAmt2
        {
            get { return this.RequiredItems[1].Cost; }
        }

        public int ReqItemAmt3
        {
            get { return this.RequiredItems[2].Cost; }
        }

        public int ReqItemAmt4
        {
            get { return this.RequiredItems[3].Cost; }
        }

        public int ReqItemAmt5
        {
            get { return this.RequiredItems[4].Cost; }
        }

        public struct RequiredItem
        {
            public Asda2ItemId Id;
            public int Cost;

            public ItemTemplate Template
            {
                get { return ItemMgr.GetTemplate(this.Id); }
            }
        }
    }
}
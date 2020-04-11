﻿using System;
using System.Collections.Generic;
using WCell.Util.Data;

namespace WCell.RealmServer.Items
{
    [DataHolder]
    public class DecompositionDrop : IDataHolder
    {
        public int Id { get; set; }

        public int FromItemId { get; set; }

        public int ItemId { get; set; }

        public float Chance { get; set; }

        public int MinAmount { get; set; }

        public int MaxAmount { get; set; }

        public void FinalizeDataHolder()
        {
            if (!Asda2ItemMgr.DecompositionDrops.ContainsKey(this.FromItemId))
                Asda2ItemMgr.DecompositionDrops.Add(this.FromItemId, new List<DecompositionDrop>());
            Asda2ItemMgr.DecompositionDrops[this.FromItemId].Add(this);
            if (Asda2ItemMgr.DecompositionDrops[this.FromItemId].Count <= 1)
                return;
            Asda2ItemMgr.DecompositionDrops[this.FromItemId]
                .Sort(new Comparison<DecompositionDrop>(this.SortBoosterItems));
        }

        public int SortBoosterItems(DecompositionDrop a, DecompositionDrop b)
        {
            return a.Chance.CompareTo(b.Chance);
        }
    }
}
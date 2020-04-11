using System;
using System.Collections.Generic;
using WCell.Util.Data;

namespace WCell.RealmServer.Items
{
    [DataHolder]
    public class BoosterDrop : IDataHolder
    {
        public int Id;
        public int BoosterId;
        public int ItemId;
        public float Chance;

        public void FinalizeDataHolder()
        {
            if (!Asda2ItemMgr.BoosterDrops.ContainsKey(this.BoosterId))
                Asda2ItemMgr.BoosterDrops.Add(this.BoosterId, new List<BoosterDrop>());
            Asda2ItemMgr.BoosterDrops[this.BoosterId].Add(this);
            if (Asda2ItemMgr.BoosterDrops[this.BoosterId].Count <= 1)
                return;
            Asda2ItemMgr.BoosterDrops[this.BoosterId].Sort(new Comparison<BoosterDrop>(this.SortBoosterItems));
        }

        public int SortBoosterItems(BoosterDrop a, BoosterDrop b)
        {
            return a.Chance.CompareTo(b.Chance);
        }
    }
}
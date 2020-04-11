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
            if (!Asda2ItemMgr.BoosterDrops.ContainsKey(BoosterId))
                Asda2ItemMgr.BoosterDrops.Add(BoosterId, new List<BoosterDrop>());
            Asda2ItemMgr.BoosterDrops[BoosterId].Add(this);
            if (Asda2ItemMgr.BoosterDrops[BoosterId].Count > 1)
                Asda2ItemMgr.BoosterDrops[BoosterId].Sort(SortBoosterItems);
        }
        public int SortBoosterItems(BoosterDrop a, BoosterDrop b)
        {
            return a.Chance.CompareTo(b.Chance);
        }
    }
}
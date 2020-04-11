using System.Collections.Generic;
using WCell.RealmServer.Items;
using WCell.Util.Data;

namespace WCell.RealmServer.Asda2_Items
{
    [DataHolder]
    public class PackageDrop : IDataHolder
    {
        public int Id;
        public int PackageId;
        public int ItemId;
        public int Amount;

        public void FinalizeDataHolder()
        {
            if (!Asda2ItemMgr.PackageDrops.ContainsKey(this.PackageId))
                Asda2ItemMgr.PackageDrops.Add(this.PackageId, new List<PackageDrop>());
            Asda2ItemMgr.PackageDrops[this.PackageId].Add(this);
        }
    }
}
using WCell.RealmServer.Items;
using WCell.Util.Data;

namespace WCell.RealmServer.Asda2_Items
{
    [DataHolder]
    public class RegularShopRecord : IDataHolder
    {
        public int Id { get; set; }

        public int ItemId { get; set; }

        public int NpcId { get; set; }

        public void FinalizeDataHolder()
        {
            if (Asda2ItemMgr.AvalibleRegularShopItems.ContainsKey(this.ItemId))
                return;
            Asda2ItemMgr.AvalibleRegularShopItems.Add(this.ItemId, this);
        }
    }
}
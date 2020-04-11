using WCell.RealmServer.Items;
using WCell.Util.Data;

namespace WCell.RealmServer.Asda2_Items
{
    [DataHolder]
    public class WarShopDataRecord : IDataHolder
    {
        public int Location { get; set; }

        public int Id { get; set; }

        public int ItemId { get; set; }

        public int Amount { get; set; }

        public int Money1Type { get; set; }

        public int Money2Type { get; set; }

        public int Cost1 { get; set; }

        public int Cost2 { get; set; }

        public long Page { get; set; }

        public void FinalizeDataHolder()
        {
            Asda2ItemMgr.WarShopDataRecords[this.Id] = this;
        }
    }
}
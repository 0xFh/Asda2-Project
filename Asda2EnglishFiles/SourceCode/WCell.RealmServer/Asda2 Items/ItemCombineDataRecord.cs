using WCell.Util.Data;

namespace WCell.RealmServer.Items
{
    [DataHolder]
    public class ItemCombineDataRecord : IDataHolder
    {
        public int Id { get; set; }

        [Persistent(5)] public int[] RequiredItems { get; set; }

        [Persistent(5)] public int[] Amounts { get; set; }

        public int ResultItem { get; set; }

        public void FinalizeDataHolder()
        {
            Asda2ItemMgr.ItemCombineRecords.SetValue((object) this, this.Id);
        }
    }
}
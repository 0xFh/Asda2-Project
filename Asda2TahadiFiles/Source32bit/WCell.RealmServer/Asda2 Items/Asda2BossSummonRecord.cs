using WCell.Constants.NPCs;
using WCell.Constants.World;
using WCell.Util.Data;

namespace WCell.RealmServer.Items
{
    [DataHolder]
    public class Asda2BossSummonRecord : IDataHolder
    {
        public int Id { get; set; }

        public byte Amount { get; set; }

        public NPCId MobId { get; set; }

        public MapId MapId { get; set; }

        public short X { get; set; }

        public short Y { get; set; }

        public void FinalizeDataHolder()
        {
            Asda2ItemMgr.SummonRecords.SetValue((object) this, this.Id);
        }
    }
}
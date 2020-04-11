using Castle.ActiveRecord;
using WCell.Util.Data;

namespace WCell.RealmServer.Asda2Fishing
{
    [DataHolder]
    public class FishingFishInfoRecord : IDataHolder
    {
        public int Id { get; set; }

        public int FishId { get; set; }

        public int FishingTime { get; set; }

        [Persistent(Length = 6)] [Property] public int[] BaitIds { get; set; }

        public void FinalizeDataHolder()
        {
            Asda2FishingMgr.FishRecords.Add(this.FishId, this);
        }
    }
}
using WCell.Util.Data;

namespace WCell.RealmServer.Asda2Fishing
{
    [DataHolder]
    public class FishingBaseInfoRecord : IDataHolder
    {
        public int Id { get; set; }

        [Persistent(Length = 20)] public int[] ItemIds { get; set; }

        [Persistent(Length = 20)] public int[] MaxFishLenghts { get; set; }

        [Persistent(Length = 20)] public int[] MinFishLengths { get; set; }

        [Persistent(Length = 20)] public int[] Chances { get; set; }

        public int Key { get; set; }

        public int IsPremium { get; set; }

        public void FinalizeDataHolder()
        {
            if (this.IsPremium == 1)
                Asda2FishingMgr.PremiumFishingBaseInfos.Add(this.Key, this);
            else
                Asda2FishingMgr.FishingBaseInfos.Add(this.Key, this);
        }
    }
}
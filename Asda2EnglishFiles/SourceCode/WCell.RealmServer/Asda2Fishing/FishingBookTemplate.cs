using System.Collections.Generic;
using WCell.Util.Data;

namespace WCell.RealmServer.Asda2Fishing
{
    [DataHolder]
    public class FishingBookTemplate : IDataHolder
    {
        public Dictionary<int, byte> FishIndexes = new Dictionary<int, byte>();

        public int Id { get; set; }

        public int BookId { get; set; }

        [Persistent(Length = 30)] public int[] RequiredFishes { get; set; }

        [Persistent(Length = 30)] public int[] RequiredFishesAmounts { get; set; }

        [Persistent(Length = 4)] public int[] Rewards { get; set; }

        [Persistent(Length = 4)] public int[] RewardAmounts { get; set; }

        public void FinalizeDataHolder()
        {
            if (Asda2FishingMgr.FishingBookTemplates.ContainsKey(this.BookId))
                return;
            Asda2FishingMgr.FishingBookTemplates.Add(this.BookId, this);
            for (byte index = 0; (int) index < this.RequiredFishes.Length; ++index)
            {
                if (this.RequiredFishes[(int) index] != -1)
                    this.FishIndexes.Add(this.RequiredFishes[(int) index], index);
            }
        }
    }
}
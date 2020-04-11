using System.Collections.Generic;
using WCell.Util.Data;

namespace WCell.RealmServer.Asda2PetSystem
{
    [DataHolder]
    public class PetExpTableRecord : IDataHolder
    {
        public int Id { get; set; }

        public int Rank { get; set; }

        public int Rarity { get; set; }

        [Persistent(Length = 10)] public int[] Exps { get; set; }

        public void FinalizeDataHolder()
        {
            if (!Asda2PetMgr.ExpTable.ContainsKey(this.Rank))
                Asda2PetMgr.ExpTable.Add(this.Rank, new Dictionary<int, int[]>());
            Asda2PetMgr.ExpTable[this.Rank].Add(this.Rarity, this.Exps);
        }
    }
}
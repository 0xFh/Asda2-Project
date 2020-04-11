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
        [Persistent (Length = 10)]
        public int[] Exps { get; set; }
        #region Implementation of IDataHolder

        public void FinalizeDataHolder()
        {
            if(!Asda2PetMgr.ExpTable.ContainsKey(Rank))
                Asda2PetMgr.ExpTable.Add(Rank,new Dictionary<int, int[]>());
            Asda2PetMgr.ExpTable[Rank].Add(Rarity,Exps);
        }

        #endregion
    }
}
using System.Collections.Generic;
using WCell.Util.Data;

namespace WCell.RealmServer.Asda2PetSystem
{
    [DataHolder]
    public class PetOptionValueRecord : IDataHolder
    {
        public int Id { get; set; }
        public int StatType { get; set; }
        public int PetRank { get; set; }
        public int PetRarity { get; set; }
        public int PetLevel { get; set; }
        public int Value { get; set; }
        #region Implementation of IDataHolder

        public void FinalizeDataHolder()
        {
            if(!Asda2PetMgr.PetOptionValues.ContainsKey(StatType))
                Asda2PetMgr.PetOptionValues.Add(StatType,new Dictionary<int, Dictionary<int, Dictionary<int, int>>>());
            if(!Asda2PetMgr.PetOptionValues[StatType].ContainsKey(PetRank))
                Asda2PetMgr.PetOptionValues[StatType].Add(PetRank,new Dictionary<int, Dictionary<int, int>>());
            if(!Asda2PetMgr.PetOptionValues[StatType][PetRank].ContainsKey(PetRarity))
                Asda2PetMgr.PetOptionValues[StatType][PetRank].Add(PetRarity,new Dictionary<int, int>());
            Asda2PetMgr.PetOptionValues[StatType][PetRank][PetRarity].Add(PetLevel,Value);
        }

        #endregion
    }
}
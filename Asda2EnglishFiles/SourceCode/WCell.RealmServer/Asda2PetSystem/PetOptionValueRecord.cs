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

        public void FinalizeDataHolder()
        {
            if (!Asda2PetMgr.PetOptionValues.ContainsKey(this.StatType))
                Asda2PetMgr.PetOptionValues.Add(this.StatType,
                    new Dictionary<int, Dictionary<int, Dictionary<int, int>>>());
            if (!Asda2PetMgr.PetOptionValues[this.StatType].ContainsKey(this.PetRank))
                Asda2PetMgr.PetOptionValues[this.StatType]
                    .Add(this.PetRank, new Dictionary<int, Dictionary<int, int>>());
            if (!Asda2PetMgr.PetOptionValues[this.StatType][this.PetRank].ContainsKey(this.PetRarity))
                Asda2PetMgr.PetOptionValues[this.StatType][this.PetRank]
                    .Add(this.PetRarity, new Dictionary<int, int>());
            Asda2PetMgr.PetOptionValues[this.StatType][this.PetRank][this.PetRarity].Add(this.PetLevel, this.Value);
        }
    }
}
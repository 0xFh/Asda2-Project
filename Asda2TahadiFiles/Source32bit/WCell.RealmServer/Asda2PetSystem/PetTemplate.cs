using Castle.ActiveRecord;
using System;
using System.Collections.Generic;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.Asda2PetSystem
{
    [DataHolder]
    public class PetTemplate : IDataHolder
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int EvolutionStep { get; set; }

        public int Rank { get; set; }

        public int Rarity { get; set; }

        public int MaxLevel { get; set; }

        public int Bonus1Type { get; set; }

        public int Bonus2Type { get; set; }

        public int Bonus3Type { get; set; }

        [Persistent(Length = 5)] [Property] public int[] WhiteByRankPetIds { get; set; }

        [Property] [Persistent(Length = 5)] public int[] YellowByRankPetIds { get; set; }

        [Persistent(Length = 5)] [Property] public int[] PurpleByRankPetIds { get; set; }

        [Persistent(Length = 5)] [Property] public int[] GreenByRankPetIds { get; set; }

        public int MinimumUsingLevel
        {
            get
            {
                switch (this.Rank)
                {
                    case 1:
                        return 20;
                    case 2:
                        return 40;
                    case 3:
                        return 60;
                    case 4:
                        return 80;
                    case 5:
                        return 100;
                    default:
                        return 1;
                }
            }
        }

        public void FinalizeDataHolder()
        {
            ArrayUtil.SetValue((Array) Asda2PetMgr.PetTemplates, this.Id, (object) this);
            if (!Asda2PetMgr.PetTemplatesByRankAndRarity.ContainsKey(this.Rank))
                Asda2PetMgr.PetTemplatesByRankAndRarity.Add(this.Rank,
                    new Dictionary<int, Dictionary<string, PetTemplate>>());
            if (!Asda2PetMgr.PetTemplatesByRankAndRarity[this.Rank].ContainsKey(this.Rarity))
                Asda2PetMgr.PetTemplatesByRankAndRarity[this.Rank]
                    .Add(this.Rarity, new Dictionary<string, PetTemplate>());
            if (Asda2PetMgr.PetTemplatesByRankAndRarity[this.Rank][this.Rarity].ContainsKey(this.Name))
                return;
            Asda2PetMgr.PetTemplatesByRankAndRarity[this.Rank][this.Rarity].Add(this.Name, this);
        }

        public PetTemplate GetEvolutionTemplate(int rarity, int rank)
        {
            switch (rarity)
            {
                case 1:
                    return Asda2PetMgr.PetTemplates.Get<PetTemplate>(this.YellowByRankPetIds[rank]);
                case 2:
                    return Asda2PetMgr.PetTemplates.Get<PetTemplate>(this.PurpleByRankPetIds[rank]);
                case 3:
                    return Asda2PetMgr.PetTemplates.Get<PetTemplate>(this.GreenByRankPetIds[rank]);
                default:
                    return Asda2PetMgr.PetTemplates.Get<PetTemplate>(this.WhiteByRankPetIds[rank]);
            }
        }
    }
}
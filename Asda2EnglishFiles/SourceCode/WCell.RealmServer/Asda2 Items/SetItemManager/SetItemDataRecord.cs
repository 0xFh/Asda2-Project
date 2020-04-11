using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Util.Data;

namespace WCell.RealmServer.Entities
{
    [DataHolder]
    public class SetItemDataRecord : IDataHolder
    {
        public List<Asda2SetBonus> SetBonuses = new List<Asda2SetBonus>();

        public string Name { get; set; }

        public int Id { get; set; }

        [Persistent(Length = 10)] public int[] ItemIds { get; set; }

        public int Stat1Type { get; set; }

        public int Stat1Value { get; set; }

        public int Stat2Type { get; set; }

        public int Stat2Value { get; set; }

        public int Stat3Type { get; set; }

        public int Stat3Value { get; set; }

        public int MaxItemsCount { get; set; }

        public int Steps { get; set; }

        public void FinalizeDataHolder()
        {
            foreach (int key in ((IEnumerable<int>) this.ItemIds).Where<int>((Func<int, bool>) (itemId => itemId != -1))
                .Where<int>((Func<int, bool>) (itemId => !SetItemManager.ItemSetsRecordsByItemIds.ContainsKey(itemId))))
                SetItemManager.ItemSetsRecordsByItemIds.Add(key, this);
            this.MaxItemsCount = ((IEnumerable<int>) this.ItemIds).Count<int>((Func<int, bool>) (i => i > 0));
            if (this.Stat1Type > 0)
                this.SetBonuses.Add(new Asda2SetBonus()
                {
                    Type = this.Stat1Type,
                    Value = this.Stat1Value
                });
            if (this.Stat2Type > 0)
                this.SetBonuses.Add(new Asda2SetBonus()
                {
                    Type = this.Stat2Type,
                    Value = this.Stat2Value
                });
            if (this.Stat3Type > 0)
                this.SetBonuses.Add(new Asda2SetBonus()
                {
                    Type = this.Stat3Type,
                    Value = this.Stat3Value
                });
            foreach (Asda2SetBonus setBonuse in this.SetBonuses)
            {
                if (setBonuse.Type == 10)
                    setBonuse.Value = (int) (((double) setBonuse.Value + 0.5) / 5.0);
            }
        }

        public Asda2SetBonus GetBonus(byte itemsAppliedCount)
        {
            int index = this.SetBonuses.Count - 1 - (this.MaxItemsCount - (int) itemsAppliedCount);
            if (index >= 0 && index < this.SetBonuses.Count)
                return this.SetBonuses[index];
            return (Asda2SetBonus) null;
        }
    }
}
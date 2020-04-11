using System.Collections.Generic;
using WCell.Constants.Looting;
using WCell.Util.Data;

namespace WCell.RealmServer.Asda2Looting
{
    public abstract class Asda2LootItemEntry : Asda2LootEntity, IDataHolder
    {
        public Asda2LootEntryType LootType;
        public uint GroupId;
        public int Guid;

        public object GetId()
        {
            return (object) this.MonstrId;
        }

        public void FinalizeDataHolder()
        {
            if (this.MinAmount < 1)
                this.MinAmount = 1;
            if (this.MinAmount > this.MaxAmount)
                this.MaxAmount = this.MinAmount;
            if ((double) this.DropChance < 0.0)
                this.DropChance = -this.DropChance;
            Asda2LootMgr.AddEntry(this);
        }

        protected static void AddItems<T>(Asda2LootEntryType t, List<T> all) where T : Asda2LootItemEntry
        {
            foreach (List<Asda2LootItemEntry> entry in Asda2LootMgr.GetEntries(t))
            {
                if (entry != null)
                {
                    foreach (Asda2LootItemEntry asda2LootItemEntry in entry)
                        all.Add((T) asda2LootItemEntry);
                }
            }
        }

        public override string ToString()
        {
            return this.ItemTemplate.ToString() + " (" + (object) this.DropChance + "%)";
        }
    }
}
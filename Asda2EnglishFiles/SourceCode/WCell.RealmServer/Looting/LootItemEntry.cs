using System.Collections.Generic;
using WCell.Constants.Looting;
using WCell.Util.Data;

namespace WCell.RealmServer.Looting
{
    public abstract class LootItemEntry : LootEntity, IDataHolder
    {
        public LootEntryType LootType;
        public uint GroupId;
        public int MinAmountOrRef;
        public uint ReferencedEntryId;

        public object GetId()
        {
            return (object) this.EntryId;
        }

        public void FinalizeDataHolder()
        {
            if (this.MinAmountOrRef < 0)
                this.ReferencedEntryId = (uint) -this.MinAmountOrRef;
            else
                this.MinAmount = this.MinAmountOrRef;
            if (this.MinAmount < 1)
                this.MinAmount = 1;
            if (this.MinAmount > this.MaxAmount)
                this.MaxAmount = this.MinAmount;
            if ((double) this.DropChance < 0.0)
                this.DropChance = -this.DropChance;
            LootMgr.AddEntry(this);
        }

        protected static void AddItems<T>(LootEntryType t, List<T> all) where T : LootItemEntry
        {
            foreach (ResolvedLootItemList entry in LootMgr.GetEntries(t))
            {
                if (entry != null)
                {
                    foreach (LootEntity lootEntity in (List<LootEntity>) entry)
                        all.Add((T) lootEntity);
                }
            }
        }

        public override string ToString()
        {
            return this.ItemTemplate.ToString() + " (" + (object) this.DropChance + "%)";
        }
    }
}
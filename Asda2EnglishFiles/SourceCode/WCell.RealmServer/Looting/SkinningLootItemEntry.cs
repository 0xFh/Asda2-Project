using System.Collections.Generic;
using WCell.Constants.Looting;

namespace WCell.RealmServer.Looting
{
    public class SkinningLootItemEntry : LootItemEntry
    {
        public static IEnumerable<SkinningLootItemEntry> GetAllDataHolders()
        {
            List<SkinningLootItemEntry> all = new List<SkinningLootItemEntry>(10000);
            LootItemEntry.AddItems<SkinningLootItemEntry>(LootEntryType.Skinning, all);
            return (IEnumerable<SkinningLootItemEntry>) all;
        }
    }
}
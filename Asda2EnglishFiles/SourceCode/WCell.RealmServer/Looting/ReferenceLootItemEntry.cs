using System.Collections.Generic;
using WCell.Constants.Looting;

namespace WCell.RealmServer.Looting
{
    public class ReferenceLootItemEntry : LootItemEntry
    {
        public static IEnumerable<ReferenceLootItemEntry> GetAllDataHolders()
        {
            List<ReferenceLootItemEntry> all = new List<ReferenceLootItemEntry>(10000);
            LootItemEntry.AddItems<ReferenceLootItemEntry>(LootEntryType.Reference, all);
            return (IEnumerable<ReferenceLootItemEntry>) all;
        }
    }
}
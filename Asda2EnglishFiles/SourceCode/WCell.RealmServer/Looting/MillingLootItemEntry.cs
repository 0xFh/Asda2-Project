using System.Collections.Generic;
using WCell.Constants.Looting;

namespace WCell.RealmServer.Looting
{
    public class MillingLootItemEntry : LootItemEntry
    {
        public static IEnumerable<MillingLootItemEntry> GetAllDataHolders()
        {
            List<MillingLootItemEntry> all = new List<MillingLootItemEntry>(10000);
            LootItemEntry.AddItems<MillingLootItemEntry>(LootEntryType.Milling, all);
            return (IEnumerable<MillingLootItemEntry>) all;
        }
    }
}
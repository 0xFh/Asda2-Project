using System.Collections.Generic;
using WCell.Constants.Looting;

namespace WCell.RealmServer.Looting
{
    public class FishingLootItemEntry : LootItemEntry
    {
        public static IEnumerable<FishingLootItemEntry> GetAllDataHolders()
        {
            List<FishingLootItemEntry> all = new List<FishingLootItemEntry>(400);
            LootItemEntry.AddItems<FishingLootItemEntry>(LootEntryType.Fishing, all);
            return (IEnumerable<FishingLootItemEntry>) all;
        }
    }
}
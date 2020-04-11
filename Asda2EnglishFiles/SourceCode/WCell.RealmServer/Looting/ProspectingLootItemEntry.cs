using System.Collections.Generic;
using WCell.Constants.Looting;

namespace WCell.RealmServer.Looting
{
    public class ProspectingLootItemEntry : LootItemEntry
    {
        public static IEnumerable<ProspectingLootItemEntry> GetAllDataHolders()
        {
            List<ProspectingLootItemEntry> all = new List<ProspectingLootItemEntry>(10000);
            LootItemEntry.AddItems<ProspectingLootItemEntry>(LootEntryType.Prospecting, all);
            return (IEnumerable<ProspectingLootItemEntry>) all;
        }
    }
}
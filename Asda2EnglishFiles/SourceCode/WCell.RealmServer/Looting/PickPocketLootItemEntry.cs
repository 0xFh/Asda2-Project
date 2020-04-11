using System.Collections.Generic;
using WCell.Constants.Looting;

namespace WCell.RealmServer.Looting
{
    public class PickPocketLootItemEntry : LootItemEntry
    {
        public static IEnumerable<PickPocketLootItemEntry> GetAllDataHolders()
        {
            List<PickPocketLootItemEntry> all = new List<PickPocketLootItemEntry>(10000);
            LootItemEntry.AddItems<PickPocketLootItemEntry>(LootEntryType.PickPocketing, all);
            return (IEnumerable<PickPocketLootItemEntry>) all;
        }
    }
}
using System.Collections.Generic;
using WCell.Constants.Looting;

namespace WCell.RealmServer.Looting
{
    public class ItemLootItemEntry : LootItemEntry
    {
        public static IEnumerable<ItemLootItemEntry> GetAllDataHolders()
        {
            List<ItemLootItemEntry> all = new List<ItemLootItemEntry>(20000);
            LootItemEntry.AddItems<ItemLootItemEntry>(LootEntryType.Item, all);
            LootItemEntry.AddItems<ItemLootItemEntry>(LootEntryType.Disenchanting, all);
            LootItemEntry.AddItems<ItemLootItemEntry>(LootEntryType.Prospecting, all);
            LootItemEntry.AddItems<ItemLootItemEntry>(LootEntryType.Milling, all);
            return (IEnumerable<ItemLootItemEntry>) all;
        }
    }
}
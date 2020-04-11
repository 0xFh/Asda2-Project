using System.Collections.Generic;
using WCell.Constants.Looting;

namespace WCell.RealmServer.Looting
{
    public class GOLootItemEntry : LootItemEntry
    {
        public static IEnumerable<GOLootItemEntry> GetAllDataHolders()
        {
            List<GOLootItemEntry> all = new List<GOLootItemEntry>(20000);
            LootItemEntry.AddItems<GOLootItemEntry>(LootEntryType.GameObject, all);
            return (IEnumerable<GOLootItemEntry>) all;
        }
    }
}
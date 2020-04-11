using System.Collections.Generic;
using WCell.Constants.Looting;

namespace WCell.RealmServer.Looting
{
    public class NPCLootItemEntry : LootItemEntry
    {
        public static IEnumerable<NPCLootItemEntry> GetAllDataHolders()
        {
            List<NPCLootItemEntry> all = new List<NPCLootItemEntry>(500000);
            LootItemEntry.AddItems<NPCLootItemEntry>(LootEntryType.NPCCorpse, all);
            LootItemEntry.AddItems<NPCLootItemEntry>(LootEntryType.Skinning, all);
            LootItemEntry.AddItems<NPCLootItemEntry>(LootEntryType.PickPocketing, all);
            return (IEnumerable<NPCLootItemEntry>) all;
        }
    }
}
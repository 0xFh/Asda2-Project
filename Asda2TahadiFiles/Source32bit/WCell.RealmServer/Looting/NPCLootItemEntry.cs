using System.Collections.Generic;
using WCell.Constants.Looting;

namespace WCell.RealmServer.Looting
{
  public class NPCLootItemEntry : LootItemEntry
  {
    public static IEnumerable<NPCLootItemEntry> GetAllDataHolders()
    {
      List<NPCLootItemEntry> all = new List<NPCLootItemEntry>(500000);
      AddItems(LootEntryType.NPCCorpse, all);
      AddItems(LootEntryType.Skinning, all);
      AddItems(LootEntryType.PickPocketing, all);
      return all;
    }
  }
}
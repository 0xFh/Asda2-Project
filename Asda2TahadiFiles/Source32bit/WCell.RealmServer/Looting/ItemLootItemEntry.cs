using System.Collections.Generic;
using WCell.Constants.Looting;

namespace WCell.RealmServer.Looting
{
  public class ItemLootItemEntry : LootItemEntry
  {
    public static IEnumerable<ItemLootItemEntry> GetAllDataHolders()
    {
      List<ItemLootItemEntry> all = new List<ItemLootItemEntry>(20000);
      AddItems(LootEntryType.Item, all);
      AddItems(LootEntryType.Disenchanting, all);
      AddItems(LootEntryType.Prospecting, all);
      AddItems(LootEntryType.Milling, all);
      return all;
    }
  }
}
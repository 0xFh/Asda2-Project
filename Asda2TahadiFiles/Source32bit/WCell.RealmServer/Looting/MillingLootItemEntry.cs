using System.Collections.Generic;
using WCell.Constants.Looting;

namespace WCell.RealmServer.Looting
{
  public class MillingLootItemEntry : LootItemEntry
  {
    public static IEnumerable<MillingLootItemEntry> GetAllDataHolders()
    {
      List<MillingLootItemEntry> all = new List<MillingLootItemEntry>(10000);
      AddItems(LootEntryType.Milling, all);
      return all;
    }
  }
}
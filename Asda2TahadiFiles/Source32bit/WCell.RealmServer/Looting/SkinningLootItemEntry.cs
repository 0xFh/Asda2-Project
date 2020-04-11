using System.Collections.Generic;
using WCell.Constants.Looting;

namespace WCell.RealmServer.Looting
{
  public class SkinningLootItemEntry : LootItemEntry
  {
    public static IEnumerable<SkinningLootItemEntry> GetAllDataHolders()
    {
      List<SkinningLootItemEntry> all = new List<SkinningLootItemEntry>(10000);
      AddItems(LootEntryType.Skinning, all);
      return all;
    }
  }
}
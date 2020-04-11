using System.Collections.Generic;
using WCell.RealmServer.Content;
using WCell.RealmServer.Items.Enchanting;
using WCell.Util;

namespace WCell.RealmServer.Items
{
  public class ItemRandomEnchantEntry : BaseItemRandomPropertyInfo
  {
    public override void FinalizeDataHolder()
    {
      if(PropertiesId > 30000U)
      {
        ContentMgr.OnInvalidDBData("RandomEnchantEntry has invalid PropertiesId: {0} (Enchant: {2})",
          (object) PropertiesId, (object) EnchantId);
      }
      else
      {
        List<ItemRandomEnchantEntry> randomEnchantEntryList =
          EnchantMgr.RandomEnchantEntries.Get(PropertiesId);
        if(randomEnchantEntryList == null)
          ArrayUtil.Set(ref EnchantMgr.RandomEnchantEntries, PropertiesId,
            randomEnchantEntryList = new List<ItemRandomEnchantEntry>());
        randomEnchantEntryList.Add(this);
      }
    }
  }
}
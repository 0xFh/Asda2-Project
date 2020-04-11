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
            if (this.PropertiesId > 30000U)
            {
                ContentMgr.OnInvalidDBData("RandomEnchantEntry has invalid PropertiesId: {0} (Enchant: {2})",
                    (object) this.PropertiesId, (object) this.EnchantId);
            }
            else
            {
                List<ItemRandomEnchantEntry> randomEnchantEntryList =
                    EnchantMgr.RandomEnchantEntries.Get<List<ItemRandomEnchantEntry>>(this.PropertiesId);
                if (randomEnchantEntryList == null)
                    ArrayUtil.Set<List<ItemRandomEnchantEntry>>(ref EnchantMgr.RandomEnchantEntries, this.PropertiesId,
                        randomEnchantEntryList = new List<ItemRandomEnchantEntry>());
                randomEnchantEntryList.Add(this);
            }
        }
    }
}
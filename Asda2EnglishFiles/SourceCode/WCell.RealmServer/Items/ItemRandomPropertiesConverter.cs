using WCell.Core.DBC;
using WCell.RealmServer.Items.Enchanting;

namespace WCell.RealmServer.Items
{
    public class ItemRandomPropertiesConverter : AdvancedDBCRecordConverter<ItemRandomPropertyEntry>
    {
        public override ItemRandomPropertyEntry ConvertTo(byte[] rawData, ref int id)
        {
            int num4;
            ItemRandomPropertyEntry entry = new ItemRandomPropertyEntry();
            int num = 0;
            id = num4 = DBCRecordConverter.GetInt32(rawData, num++);
            entry.Id = (uint) num4;
            num++;
            for (int i = 0; i < entry.Enchants.Length; i++)
            {
                uint num3 = DBCRecordConverter.GetUInt32(rawData, num++);
                entry.Enchants[i] = EnchantMgr.GetEnchantmentEntry(num3);
            }

            return entry;
        }
    }
}
using WCell.Constants.Items;
using WCell.Core.DBC;
using WCell.RealmServer.Items.Enchanting;

namespace WCell.RealmServer.Items
{
    public class GemPropertiesConverter : AdvancedDBCRecordConverter<GemProperties>
    {
        public override GemProperties ConvertTo(byte[] rawData, ref int id)
        {
            GemProperties gemProperties = new GemProperties();
            gemProperties.Id = (uint) (id = DBCRecordConverter.GetInt32(rawData, 0));
            uint uint32 = DBCRecordConverter.GetUInt32(rawData, 1);
            gemProperties.Enchantment = EnchantMgr.GetEnchantmentEntry(uint32);
            gemProperties.Color = (SocketColor) DBCRecordConverter.GetUInt32(rawData, 4);
            return gemProperties;
        }
    }
}
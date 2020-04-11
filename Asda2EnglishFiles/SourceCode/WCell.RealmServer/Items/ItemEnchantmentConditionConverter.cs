using WCell.Core.DBC;
using WCell.RealmServer.Items.Enchanting;

namespace WCell.RealmServer.Items
{
    public class ItemEnchantmentConditionConverter : AdvancedDBCRecordConverter<ItemEnchantmentCondition>
    {
        public override ItemEnchantmentCondition ConvertTo(byte[] rawData, ref int id)
        {
            int num2;
            ItemEnchantmentCondition condition = new ItemEnchantmentCondition();
            int num = 0;
            id = num2 = DBCRecordConverter.GetInt32(rawData, num++);
            condition.Id = (uint) num2;
            return condition;
        }
    }
}
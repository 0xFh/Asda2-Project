using WCell.Constants.Items;
using WCell.Constants.Skills;
using WCell.Core.DBC;
using WCell.RealmServer.Items.Enchanting;
using WCell.Util;

namespace WCell.RealmServer.Items
{
    public class ItemEnchantmentConverter : AdvancedDBCRecordConverter<ItemEnchantmentEntry>
    {
        public override ItemEnchantmentEntry ConvertTo(byte[] rawData, ref int id)
        {
            ItemEnchantmentEntry enchantmentEntry1 = new ItemEnchantmentEntry();
            enchantmentEntry1.Id = (uint) (id = DBCRecordConverter.GetInt32(rawData, 0));
            enchantmentEntry1.Charges = DBCRecordConverter.GetUInt32(rawData, 1);
            enchantmentEntry1.Description = this.GetString(rawData, 14);
            enchantmentEntry1.Effects = new ItemEnchantmentEffect[3];
            for (int index = 0; index < 3; ++index)
            {
                ItemEnchantmentType uint32 = (ItemEnchantmentType) DBCRecordConverter.GetUInt32(rawData, 2 + index);
                if (uint32 != ItemEnchantmentType.None)
                {
                    ItemEnchantmentEffect enchantmentEffect = new ItemEnchantmentEffect();
                    enchantmentEntry1.Effects[index] = enchantmentEffect;
                    enchantmentEffect.Type = uint32;
                    enchantmentEffect.MinAmount = DBCRecordConverter.GetInt32(rawData, 5 + index);
                    enchantmentEffect.MaxAmount = DBCRecordConverter.GetInt32(rawData, 8 + index);
                    enchantmentEffect.Misc = DBCRecordConverter.GetUInt32(rawData, 11 + index);
                }
            }

            ArrayUtil.Prune<ItemEnchantmentEffect>(ref enchantmentEntry1.Effects);
            int num1 = 31;
            ItemEnchantmentEntry enchantmentEntry2 = enchantmentEntry1;
            byte[] data1 = rawData;
            int field1 = num1;
            int num2 = field1 + 1;
            int uint32_1 = (int) DBCRecordConverter.GetUInt32(data1, field1);
            enchantmentEntry2.Visual = (uint) uint32_1;
            ItemEnchantmentEntry enchantmentEntry3 = enchantmentEntry1;
            byte[] data2 = rawData;
            int field2 = num2;
            int num3 = field2 + 1;
            int uint32_2 = (int) DBCRecordConverter.GetUInt32(data2, field2);
            enchantmentEntry3.Flags = (uint) uint32_2;
            ItemEnchantmentEntry enchantmentEntry4 = enchantmentEntry1;
            byte[] data3 = rawData;
            int field3 = num3;
            int num4 = field3 + 1;
            int uint32_3 = (int) DBCRecordConverter.GetUInt32(data3, field3);
            enchantmentEntry4.SourceItemId = (uint) uint32_3;
            byte[] data4 = rawData;
            int field4 = num4;
            int num5 = field4 + 1;
            uint uint32_4 = DBCRecordConverter.GetUInt32(data4, field4);
            if (uint32_4 > 0U)
                enchantmentEntry1.Condition = EnchantMgr.GetEnchantmentCondition(uint32_4);
            ItemEnchantmentEntry enchantmentEntry5 = enchantmentEntry1;
            byte[] data5 = rawData;
            int field5 = num5;
            int field6 = field5 + 1;
            int uint32_5 = (int) DBCRecordConverter.GetUInt32(data5, field5);
            enchantmentEntry5.RequiredSkillId = (SkillId) uint32_5;
            enchantmentEntry1.RequiredSkillAmount = DBCRecordConverter.GetInt32(rawData, field6);
            return enchantmentEntry1;
        }
    }
}
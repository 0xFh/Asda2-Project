using System;
using WCell.Core.DBC;
using WCell.RealmServer.Items.Enchanting;
using WCell.Util;

namespace WCell.RealmServer.Items
{
    public class ItemRandomSuffixConverter : AdvancedDBCRecordConverter<ItemRandomSuffixEntry>
    {
        public override ItemRandomSuffixEntry ConvertTo(byte[] rawData, ref int id)
        {
            ItemRandomSuffixEntry entry = new ItemRandomSuffixEntry();
            uint num = 5;
            int field = 0;
            entry.Id = id = DBCRecordConverter.GetInt32(rawData, field++);
            field += 0x12;
            entry.Enchants = new ItemEnchantmentEntry[num];
            entry.Values = new int[num];
            for (int i = 0; i < num; i++)
            {
                uint num4 = DBCRecordConverter.GetUInt32(rawData, field);
                if (num4 != 0)
                {
                    ItemEnchantmentEntry enchantmentEntry = EnchantMgr.GetEnchantmentEntry(num4);
                    if (enchantmentEntry != null)
                    {
                        entry.Enchants[i] = enchantmentEntry;
                        entry.Values[i] = DBCRecordConverter.GetInt32(rawData, field + ((int) num));
                    }
                }

                field++;
            }

            ArrayUtil.Trunc<ItemEnchantmentEntry>(ref entry.Enchants);
            ArrayUtil.TruncVals<int>(ref entry.Values);
            return entry;
        }
    }
}
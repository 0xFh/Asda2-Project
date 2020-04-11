using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core.DBC;

namespace WCell.RealmServer.Spells
{
    public class ShapeshiftEntryConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            ShapeshiftEntry shapeshiftEntry1 = new ShapeshiftEntry();
            int num1 = 0;
            ShapeshiftEntry shapeshiftEntry2 = shapeshiftEntry1;
            byte[] data1 = rawData;
            int field1 = num1;
            int num2 = field1 + 1;
            int int32_1 = DBCRecordConverter.GetInt32(data1, field1);
            shapeshiftEntry2.Id = (ShapeshiftForm) int32_1;
            ShapeshiftEntry shapeshiftEntry3 = shapeshiftEntry1;
            byte[] data2 = rawData;
            int field2 = num2;
            int offset = field2 + 1;
            int uint32_1 = (int) DBCRecordConverter.GetUInt32(data2, field2);
            shapeshiftEntry3.BarOrder = (uint) uint32_1;
            shapeshiftEntry1.Name = this.GetString(rawData, ref offset);
            ShapeshiftEntry shapeshiftEntry4 = shapeshiftEntry1;
            byte[] data3 = rawData;
            int field3 = offset;
            int num3 = field3 + 1;
            int uint32_2 = (int) DBCRecordConverter.GetUInt32(data3, field3);
            shapeshiftEntry4.Flags = (ShapeshiftInfoFlags) uint32_2;
            ShapeshiftEntry shapeshiftEntry5 = shapeshiftEntry1;
            byte[] data4 = rawData;
            int field4 = num3;
            int num4 = field4 + 1;
            int int32_2 = DBCRecordConverter.GetInt32(data4, field4);
            shapeshiftEntry5.CreatureType = (CreatureType) int32_2;
            int num5 = num4 + 1;
            ShapeshiftEntry shapeshiftEntry6 = shapeshiftEntry1;
            byte[] data5 = rawData;
            int field5 = num5;
            int num6 = field5 + 1;
            int int32_3 = DBCRecordConverter.GetInt32(data5, field5);
            shapeshiftEntry6.AttackTime = int32_3;
            ShapeshiftEntry shapeshiftEntry7 = shapeshiftEntry1;
            byte[] data6 = rawData;
            int field6 = num6;
            int num7 = field6 + 1;
            int uint32_3 = (int) DBCRecordConverter.GetUInt32(data6, field6);
            shapeshiftEntry7.ModelIdAlliance = (uint) uint32_3;
            ShapeshiftEntry shapeshiftEntry8 = shapeshiftEntry1;
            byte[] data7 = rawData;
            int field7 = num7;
            int num8 = field7 + 1;
            int uint32_4 = (int) DBCRecordConverter.GetUInt32(data7, field7);
            shapeshiftEntry8.ModelIdHorde = (uint) uint32_4;
            int num9 = num8 + 2;
            shapeshiftEntry1.DefaultActionBarSpells = new SpellId[8];
            for (int index = 0; index < 8; ++index)
                shapeshiftEntry1.DefaultActionBarSpells[index] = (SpellId) DBCRecordConverter.GetInt32(rawData, num9++);
            SpellHandler.ShapeshiftEntries[(int) shapeshiftEntry1.Id] = shapeshiftEntry1;
        }
    }
}
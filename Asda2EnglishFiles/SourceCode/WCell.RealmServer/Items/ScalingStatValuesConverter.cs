using WCell.Core.DBC;

namespace WCell.RealmServer.Items
{
    public class ScalingStatValuesConverter : AdvancedDBCRecordConverter<ScalingStatValues>
    {
        public override ScalingStatValues ConvertTo(byte[] rawData, ref int id)
        {
            int num5;
            ScalingStatValues values = new ScalingStatValues();
            int num = 0;
            id = num5 = DBCRecordConverter.GetInt32(rawData, num++);
            values.Id = (uint) num5;
            values.Level = DBCRecordConverter.GetUInt32(rawData, num++);
            int index = 0;
            while (index < 4)
            {
                values.SsdMultiplier[index] = DBCRecordConverter.GetUInt32(rawData, num++);
                index++;
            }

            int num3 = 0;
            while (num3 < 5)
            {
                values.ArmorMod[num3] = DBCRecordConverter.GetUInt32(rawData, num++);
                num3++;
            }

            for (int i = 0; i < 6; i++)
            {
                values.DpsMod[i] = DBCRecordConverter.GetUInt32(rawData, num++);
            }

            values.SpellBonus = DBCRecordConverter.GetUInt32(rawData, num++);
            while (index < 6)
            {
                values.SsdMultiplier[index] = DBCRecordConverter.GetUInt32(rawData, num++);
                index++;
            }

            while (num3 < 8)
            {
                values.ArmorMod[num3] = DBCRecordConverter.GetUInt32(rawData, num++);
                num3++;
            }

            return values;
        }
    }
}
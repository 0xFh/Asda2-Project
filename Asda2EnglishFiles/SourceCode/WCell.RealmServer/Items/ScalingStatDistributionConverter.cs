using WCell.Core.DBC;

namespace WCell.RealmServer.Items
{
    public class ScalingStatDistributionConverter : AdvancedDBCRecordConverter<ScalingStatDistributionEntry>
    {
        public override ScalingStatDistributionEntry ConvertTo(byte[] rawData, ref int id)
        {
            int num4;
            ScalingStatDistributionEntry entry = new ScalingStatDistributionEntry();
            int num = 0;
            id = num4 = DBCRecordConverter.GetInt32(rawData, num++);
            entry.Id = (uint) num4;
            for (int i = 0; i < 10; i++)
            {
                entry.StatMod[i] = DBCRecordConverter.GetInt32(rawData, num++);
            }

            for (int j = 0; j < 10; j++)
            {
                entry.Modifier[j] = DBCRecordConverter.GetUInt32(rawData, num++);
            }

            entry.MaxLevel = DBCRecordConverter.GetUInt32(rawData, num++);
            return entry;
        }
    }
}
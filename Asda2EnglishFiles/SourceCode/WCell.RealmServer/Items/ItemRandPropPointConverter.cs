using WCell.Core.DBC;

namespace WCell.RealmServer.Items
{
    public class ItemRandPropPointConverter : AdvancedDBCRecordConverter<ItemLevelInfo>
    {
        public override ItemLevelInfo ConvertTo(byte[] rawData, ref int id)
        {
            int num5;
            ItemLevelInfo info = new ItemLevelInfo();
            int num = 0;
            id = num5 = DBCRecordConverter.GetInt32(rawData, num++);
            info.Level = (uint) num5;
            for (int i = 0; i < 5; i++)
            {
                info.EpicPoints[i] = DBCRecordConverter.GetUInt32(rawData, num++);
            }

            for (int j = 0; j < 5; j++)
            {
                info.RarePoints[j] = DBCRecordConverter.GetUInt32(rawData, num++);
            }

            for (int k = 0; k < 5; k++)
            {
                info.UncommonPoints[k] = DBCRecordConverter.GetUInt32(rawData, num++);
            }

            return info;
        }
    }
}
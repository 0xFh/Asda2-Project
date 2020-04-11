namespace WCell.Core.DBC
{
    public class GameTableConverter : AdvancedDBCRecordConverter<float>
    {
        public override float ConvertTo(byte[] rawData, ref int id)
        {
            return DBCRecordConverter.GetFloat(rawData, 0);
        }
    }
}
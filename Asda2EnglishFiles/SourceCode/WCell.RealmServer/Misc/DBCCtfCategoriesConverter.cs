using WCell.Core.DBC;

namespace WCell.RealmServer.Misc
{
    public class DBCCtfCategoriesConverter : AdvancedDBCRecordConverter<string>
    {
        public override string ConvertTo(byte[] rawData, ref int id)
        {
            id = (int) DBCRecordConverter.GetUInt32(rawData, 0);
            return this.GetString(rawData, 4);
        }
    }
}
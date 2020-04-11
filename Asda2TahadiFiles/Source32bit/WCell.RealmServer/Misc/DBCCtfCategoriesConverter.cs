using WCell.Core.DBC;

namespace WCell.RealmServer.Misc
{
  public class DBCCtfCategoriesConverter : AdvancedDBCRecordConverter<string>
  {
    public override string ConvertTo(byte[] rawData, ref int id)
    {
      id = (int) GetUInt32(rawData, 0);
      return GetString(rawData, 4);
    }
  }
}
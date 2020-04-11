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
      id = num5 = GetInt32(rawData, num++);
      values.Id = (uint) num5;
      values.Level = GetUInt32(rawData, num++);
      int index = 0;
      while(index < 4)
      {
        values.SsdMultiplier[index] = GetUInt32(rawData, num++);
        index++;
      }

      int num3 = 0;
      while(num3 < 5)
      {
        values.ArmorMod[num3] = GetUInt32(rawData, num++);
        num3++;
      }

      for(int i = 0; i < 6; i++)
      {
        values.DpsMod[i] = GetUInt32(rawData, num++);
      }

      values.SpellBonus = GetUInt32(rawData, num++);
      while(index < 6)
      {
        values.SsdMultiplier[index] = GetUInt32(rawData, num++);
        index++;
      }

      while(num3 < 8)
      {
        values.ArmorMod[num3] = GetUInt32(rawData, num++);
        num3++;
      }

      return values;
    }
  }
}
using WCell.Constants;
using WCell.Core.DBC;

namespace WCell.RealmServer.NPCs
{
  public class BarberShopStyleConverter : AdvancedDBCRecordConverter<BarberShopStyleEntry>
  {
    public override BarberShopStyleEntry ConvertTo(byte[] rawData, ref int id)
    {
      return new BarberShopStyleEntry
      {
        Id = GetInt32(rawData, 0),
        Type = GetInt32(rawData, 1),
        Race = (RaceId) GetUInt32(rawData, 37),
        Gender = (GenderType) GetUInt32(rawData, 38),
        HairId = GetInt32(rawData, 39)
      };
    }
  }
}
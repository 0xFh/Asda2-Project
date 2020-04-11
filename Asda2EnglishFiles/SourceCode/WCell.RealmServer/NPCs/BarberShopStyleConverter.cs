using WCell.Constants;
using WCell.Core.DBC;

namespace WCell.RealmServer.NPCs
{
    public class BarberShopStyleConverter : AdvancedDBCRecordConverter<BarberShopStyleEntry>
    {
        public override BarberShopStyleEntry ConvertTo(byte[] rawData, ref int id)
        {
            return new BarberShopStyleEntry()
            {
                Id = DBCRecordConverter.GetInt32(rawData, 0),
                Type = DBCRecordConverter.GetInt32(rawData, 1),
                Race = (RaceId) DBCRecordConverter.GetUInt32(rawData, 37),
                Gender = (GenderType) DBCRecordConverter.GetUInt32(rawData, 38),
                HairId = DBCRecordConverter.GetInt32(rawData, 39)
            };
        }
    }
}
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Core.DBC;

namespace WCell.RealmServer.RacesClasses
{
    public class DBCRaceConverter : AdvancedDBCRecordConverter<BaseRace>
    {
        public override BaseRace ConvertTo(byte[] rawData, ref int id)
        {
            return new BaseRace()
            {
                Id = (RaceId) (id = (int) DBCRecordConverter.GetUInt32(rawData, 0)),
                FactionTemplateId = (FactionTemplateId) DBCRecordConverter.GetUInt32(rawData, 8),
                MaleDisplayId = DBCRecordConverter.GetUInt32(rawData, 4),
                FemaleDisplayId = DBCRecordConverter.GetUInt32(rawData, 5),
                Scale = DBCRecordConverter.GetFloat(rawData, 7),
                Name = this.GetString(rawData, 14),
                ClientId = (ClientId) DBCRecordConverter.GetUInt32(rawData, 68)
            };
        }
    }
}
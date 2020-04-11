using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Core.DBC;

namespace WCell.RealmServer.RacesClasses
{
  public class DBCRaceConverter : AdvancedDBCRecordConverter<BaseRace>
  {
    public override BaseRace ConvertTo(byte[] rawData, ref int id)
    {
      return new BaseRace
      {
        Id = (RaceId) (id = (int) GetUInt32(rawData, 0)),
        FactionTemplateId = (FactionTemplateId) GetUInt32(rawData, 8),
        MaleDisplayId = GetUInt32(rawData, 4),
        FemaleDisplayId = GetUInt32(rawData, 5),
        Scale = GetFloat(rawData, 7),
        Name = GetString(rawData, 14),
        ClientId = (ClientId) GetUInt32(rawData, 68)
      };
    }
  }
}
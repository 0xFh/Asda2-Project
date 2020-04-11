using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.World;
using WCell.Core.DBC;

namespace WCell.RealmServer.Global
{
  public class AreaTableConverter : AdvancedDBCRecordConverter<ZoneTemplate>
  {
    public override ZoneTemplate ConvertTo(byte[] rawData, ref int id)
    {
      id = GetInt32(rawData, 0);
      return new ZoneTemplate
      {
        Id = (ZoneId) GetUInt32(rawData, 0),
        m_MapId = (MapId) GetUInt32(rawData, 1),
        m_parentZoneId = (ZoneId) GetUInt32(rawData, 2),
        ExplorationBit = GetInt32(rawData, 3),
        Flags = (ZoneFlags) GetUInt32(rawData, 4),
        AreaLevel = GetInt32(rawData, 10),
        Name = GetString(rawData, 11),
        Ownership = (FactionGroupMask) GetUInt32(rawData, 28)
      };
    }
  }
}
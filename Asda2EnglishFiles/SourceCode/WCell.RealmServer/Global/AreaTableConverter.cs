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
            id = DBCRecordConverter.GetInt32(rawData, 0);
            return new ZoneTemplate()
            {
                Id = (ZoneId) DBCRecordConverter.GetUInt32(rawData, 0),
                m_MapId = (MapId) DBCRecordConverter.GetUInt32(rawData, 1),
                m_parentZoneId = (ZoneId) DBCRecordConverter.GetUInt32(rawData, 2),
                ExplorationBit = DBCRecordConverter.GetInt32(rawData, 3),
                Flags = (ZoneFlags) DBCRecordConverter.GetUInt32(rawData, 4),
                AreaLevel = DBCRecordConverter.GetInt32(rawData, 10),
                Name = this.GetString(rawData, 11),
                Ownership = (FactionGroupMask) DBCRecordConverter.GetUInt32(rawData, 28)
            };
        }
    }
}
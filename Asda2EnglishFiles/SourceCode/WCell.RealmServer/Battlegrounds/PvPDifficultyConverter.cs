using WCell.Constants.World;
using WCell.Core.DBC;

namespace WCell.RealmServer.Battlegrounds
{
    public class PvPDifficultyConverter : AdvancedDBCRecordConverter<PvPDifficultyEntry>
    {
        public override PvPDifficultyEntry ConvertTo(byte[] rawData, ref int id)
        {
            return new PvPDifficultyEntry()
            {
                Id = id = DBCRecordConverter.GetInt32(rawData, 0),
                mapId = (MapId) DBCRecordConverter.GetInt32(rawData, 1),
                bracketId = DBCRecordConverter.GetInt32(rawData, 2),
                minLevel = DBCRecordConverter.GetInt32(rawData, 3),
                maxLevel = DBCRecordConverter.GetInt32(rawData, 4)
            };
        }
    }
}
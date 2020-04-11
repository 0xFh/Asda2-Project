using WCell.Constants;
using WCell.Constants.World;
using WCell.Core.DBC;

namespace WCell.RealmServer.Battlegrounds
{
    public class BattlemasterConverter : AdvancedDBCRecordConverter<BattlemasterList>
    {
        public override BattlemasterList ConvertTo(byte[] rawData, ref int bgId)
        {
            bgId = DBCRecordConverter.GetInt32(rawData, 0);
            return new BattlemasterList()
            {
                BGId = (BattlegroundId) bgId,
                MapId = (MapId) DBCRecordConverter.GetUInt32(rawData, 1)
            };
        }
    }
}
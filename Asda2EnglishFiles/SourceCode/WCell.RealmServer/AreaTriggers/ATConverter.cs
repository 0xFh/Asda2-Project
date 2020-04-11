using Cell.Core;
using WCell.Constants.World;
using WCell.Core.DBC;

namespace WCell.RealmServer.AreaTriggers
{
    internal class ATConverter : AdvancedDBCRecordConverter<AreaTrigger>
    {
        public override AreaTrigger ConvertTo(byte[] rawData, ref int id)
        {
            return new AreaTrigger((uint) (id = rawData.GetInt32(0U)), (MapId) rawData.GetUInt32(1U),
                rawData.GetFloat(2U), rawData.GetFloat(3U), rawData.GetFloat(4U), rawData.GetFloat(5U),
                rawData.GetFloat(6U), rawData.GetFloat(7U), rawData.GetFloat(8U), rawData.GetFloat(9U));
        }
    }
}
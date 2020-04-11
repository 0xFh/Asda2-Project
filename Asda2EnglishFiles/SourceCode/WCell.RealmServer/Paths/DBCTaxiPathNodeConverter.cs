using Cell.Core;
using WCell.Constants.Pathing;
using WCell.Constants.World;
using WCell.Core.DBC;

namespace WCell.RealmServer.Paths
{
    public class DBCTaxiPathNodeConverter : AdvancedDBCRecordConverter<PathVertex>
    {
        public override PathVertex ConvertTo(byte[] rawData, ref int id)
        {
            uint index = 0;
            PathVertex vertex = new PathVertex();
            id = (int) (vertex.Id = rawData.GetUInt32(index++));
            vertex.PathId = rawData.GetUInt32(index++);
            vertex.NodeIndex = rawData.GetUInt32(index++);
            vertex.MapId = (MapId) rawData.GetUInt32(index++);
            vertex.Pos = rawData.GetLocation(index);
            index += 3;
            vertex.Flags = (TaxiPathNodeFlags) ((byte) rawData.GetUInt32(index++));
            vertex.Delay = rawData.GetUInt32(index++);
            vertex.ArrivalEventId = rawData.GetUInt32(index++);
            vertex.DepartureEventId = rawData.GetUInt32(index);
            return vertex;
        }
    }
}
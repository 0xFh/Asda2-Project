using WCell.Constants.NPCs;
using WCell.Constants.World;
using WCell.Core.DBC;

namespace WCell.RealmServer.Global
{
    public class DBCTaxiNodeConverter : AdvancedDBCRecordConverter<PathNode>
    {
        public override PathNode ConvertTo(byte[] rawData, ref int id)
        {
            PathNode node = new PathNode();
            int offset = 0;
            id = (int) (node.Id = DBCRecordConverter.GetUInt32(rawData, offset++));
            node.mapId = (MapId) DBCRecordConverter.GetUInt32(rawData, offset++);
            node.Position = rawData.GetLocation((uint) offset);
            offset += 3;
            node.Name = base.GetString(rawData, ref offset);
            node.HordeMountId = (NPCId) DBCRecordConverter.GetUInt32(rawData, offset++);
            node.AllianceMountId = (NPCId) DBCRecordConverter.GetUInt32(rawData, offset);
            return node;
        }
    }
}
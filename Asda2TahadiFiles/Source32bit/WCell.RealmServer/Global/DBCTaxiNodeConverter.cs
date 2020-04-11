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
      id = (int) (node.Id = GetUInt32(rawData, offset++));
      node.mapId = (MapId) GetUInt32(rawData, offset++);
      node.Position = rawData.GetLocation((uint) offset);
      offset += 3;
      node.Name = GetString(rawData, ref offset);
      node.HordeMountId = (NPCId) GetUInt32(rawData, offset++);
      node.AllianceMountId = (NPCId) GetUInt32(rawData, offset);
      return node;
    }
  }
}
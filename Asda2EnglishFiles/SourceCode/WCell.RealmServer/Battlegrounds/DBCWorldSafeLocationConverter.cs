using Cell.Core;
using WCell.Constants.World;
using WCell.Core.DBC;

namespace WCell.RealmServer.Battlegrounds
{
    public class DBCWorldSafeLocationConverter : AdvancedDBCRecordConverter<WorldSafeLocation>
    {
        public override WorldSafeLocation ConvertTo(byte[] rawData, ref int id)
        {
            WorldSafeLocation worldSafeLocation = new WorldSafeLocation();
            id = (int) (worldSafeLocation.Id = rawData.GetUInt32(0U));
            worldSafeLocation.MapId = (MapId) rawData.GetUInt32(1U);
            worldSafeLocation.X = rawData.GetFloat(2U);
            worldSafeLocation.Y = rawData.GetFloat(3U);
            worldSafeLocation.Z = rawData.GetFloat(4U);
            return worldSafeLocation;
        }
    }
}
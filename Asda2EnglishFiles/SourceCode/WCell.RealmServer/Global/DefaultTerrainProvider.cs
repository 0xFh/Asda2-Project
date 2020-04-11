using WCell.Constants.World;
using WCell.Core.Terrain;

namespace WCell.RealmServer.Global
{
    public class DefaultTerrainProvider : ITerrainProvider
    {
        public ITerrain CreateTerrain(MapId rgnId)
        {
            return (ITerrain) new EmptyTerrain();
        }
    }
}
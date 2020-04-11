using WCell.Constants.World;
using WCell.Core.Terrain;

namespace WCell.RealmServer.Global
{
    public static class TerrainMgr
    {
        public static readonly ITerrain[] Terrains = new ITerrain[1727];

        /// <summary>
        /// Use InitializationPass.First in Addon to set a custom provider
        /// </summary>
        public static ITerrainProvider Provider;

        /// <summary>Called by World</summary>
        internal static void InitTerrain()
        {
            if (TerrainMgr.Provider == null)
                TerrainMgr.Provider = (ITerrainProvider) new DefaultTerrainProvider();
            foreach (MapTemplate mapTemplate in WCell.RealmServer.Global.World.MapTemplates)
            {
                if (mapTemplate != null)
                    TerrainMgr.Terrains[(int) mapTemplate.Id] = TerrainMgr.Provider.CreateTerrain(mapTemplate.Id);
            }
        }

        public static ITerrain GetTerrain(MapId map)
        {
            return TerrainMgr.Terrains[(int) map];
        }
    }
}
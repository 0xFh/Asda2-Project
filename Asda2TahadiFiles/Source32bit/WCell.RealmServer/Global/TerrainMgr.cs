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
      if(Provider == null)
        Provider = new DefaultTerrainProvider();
      foreach(MapTemplate mapTemplate in World.MapTemplates)
      {
        if(mapTemplate != null)
          Terrains[(int) mapTemplate.Id] = Provider.CreateTerrain(mapTemplate.Id);
      }
    }

    public static ITerrain GetTerrain(MapId map)
    {
      return Terrains[(int) map];
    }
  }
}
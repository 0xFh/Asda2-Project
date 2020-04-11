using System.Collections.Generic;
using WCell.Constants.World;
using WCell.RealmServer.GameObjects.Spawns;

namespace WCell.RealmServer.Global
{
  public static class Asda2PortalMgr
  {
    private static readonly List<GOSpawnPoolTemplate> EmptyList = new List<GOSpawnPoolTemplate>();

    public static Dictionary<MapId, List<GOSpawnPoolTemplate>> Portals =
      new Dictionary<MapId, List<GOSpawnPoolTemplate>>();

    public static List<GOSpawnPoolTemplate> GetSpawnPoolTemplatesByMap(MapId mapId)
    {
      if(!Portals.ContainsKey(mapId))
        return EmptyList;
      return Portals[mapId];
    }
  }
}
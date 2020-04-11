using System.Collections.Generic;
using WCell.Constants.World;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Events.Asda2.Managers
{
  public static class DeffenceTownEventManager
  {
    internal static Dictionary<MapId, DeffenceTownEvent> DefenceTownEvents = new Dictionary<MapId, DeffenceTownEvent>();

    public static bool Start(Map map, int minLevel, int maxLevel, float amountMod, float healthMod,
      float otherStatsMod, float speedMod, float difficulty)
    {
      switch (map.Id)
      {
        case MapId.Alpia:
          if (DefenceTownEvents.ContainsKey(map.Id))
          {
            DefenceTownEvents[map.Id].Stop(false);
            DefenceTownEvents.Remove(map.Id);
          }
          var deffTownEvent = new DefenceTownEventAplia(map, minLevel, maxLevel, amountMod, healthMod, otherStatsMod,
            speedMod, difficulty);
          DefenceTownEvents.Add(map.Id, deffTownEvent);
          deffTownEvent.Start();
          return true;
        default:
          return false;
      }
    }

    public static bool Stop(Map map, bool success)
    {
      switch (map.Id)
      {
        case MapId.Alpia:
          if (DefenceTownEvents.ContainsKey(map.Id))
          {
            DefenceTownEvents[map.Id].Stop(success);
            DefenceTownEvents.Remove(map.Id);
          }
          return true;
        default:
          return false;
      }
    }
  }
}
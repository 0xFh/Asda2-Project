using WCell.Util;

namespace WCell.Constants.World
{
  public static class WorldStates
  {
    public static readonly WorldState[] EmptyStates = new WorldState[0];
    public static readonly WorldState[] AllStates = new WorldState[6000];
    public static WorldState[] GlobalStates = new WorldState[1];
    public static readonly WorldState[][] MapStates = new WorldState[727][];
    public static readonly WorldState[][] ZoneStates = new WorldState[5016][];

    static WorldStates()
    {
      CreateStates();
      foreach(WorldState allState in AllStates)
      {
        if(allState != null)
        {
          WorldState[] arr;
          if(allState.ZoneId != ZoneId.None)
          {
            arr = GetStates(allState.ZoneId) ?? new WorldState[1];
            int num = (int) ArrayUtil.AddOnlyOne(ref arr, allState);
            ZoneStates[(int) allState.MapId] = arr;
          }
          else if(allState.MapId != MapId.End)
          {
            arr = GetStates(allState.MapId) ?? new WorldState[1];
            int num = (int) ArrayUtil.AddOnlyOne(ref arr, allState);
            MapStates[(int) allState.MapId] = arr;
          }
          else
          {
            int num = (int) ArrayUtil.AddOnlyOne(ref GlobalStates, allState);
            arr = GlobalStates;
          }

          allState.Index = (uint) (arr.Length - 1);
        }
      }
    }

    private static void AddState(WorldState state)
    {
      AllStates[(int) state.Key] = state;
    }

    public static WorldState[] GetStates(MapId map)
    {
      return MapStates.Get((uint) map) ?? new WorldState[0];
    }

    public static WorldState[] GetStates(ZoneId zone)
    {
      return ZoneStates.Get((uint) zone) ?? new WorldState[0];
    }

    public static WorldState GetState(WorldStateId id)
    {
      return AllStates[(int) id];
    }

    private static void CreateStates()
    {
      AddState(new WorldState(2264U, 0));
      AddState(new WorldState(2263U, 0));
      AddState(new WorldState(2262U, 0));
      AddState(new WorldState(2261U, 0));
      AddState(new WorldState(2260U, 0));
      AddState(new WorldState(2259U, 0));
      AddState(new WorldState(3191U, 0));
      AddState(new WorldState(3901U, 1));
      AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGAllianceScore, 0));
      AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGHordeScore, 0));
      AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGAlliancePickupState, 0));
      AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGHordePickupState, 0));
      AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGUnknown, 2));
      AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGMaxScore, 3));
      AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGHordeFlagState, 1));
      AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGAllianceFlagState, 1));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABMaxResources, 1600));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABOccupiedBasesAlliance, 0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABOccupiedBasesHorde, 0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABResourcesAlliance, 0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABResourcesHorde, 0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowStableIcon, 1));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowStableIconAlliance, 0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowStableIconHorde, 0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowStableIconAllianceContested, 0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowStableIconHordeContested, 0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowGoldMineIcon, 1));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowGoldMineIconAlliance, 0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowGoldMineIconHorde, 0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowGoldMineIconAllianceContested,
        0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowGoldMineIconHordeContested, 0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowLumberMillIcon, 1));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowLumberMillIconAlliance, 0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowLumberMillIconHorde, 0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowLumberMillIconAllianceContested,
        0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowLumberMillIconHordeContested, 0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowFarmIcon, 1));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowFarmIconAlliance, 0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowFarmIconHorde, 0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowFarmIconAllianceContested, 0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowFarmIconHordeContested, 0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowBlacksmithIcon, 1));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowBlacksmithIconAlliance, 0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowBlacksmithIconHorde, 0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowBlacksmithIconAllianceContested,
        0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowBlacksmithIconHordeContested, 0));
      AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABNearVictoryWarning, 1400));
    }
  }
}
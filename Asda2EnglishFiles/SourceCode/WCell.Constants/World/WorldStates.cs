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
            WorldStates.CreateStates();
            foreach (WorldState allState in WorldStates.AllStates)
            {
                if (allState != null)
                {
                    WorldState[] arr;
                    if (allState.ZoneId != ZoneId.None)
                    {
                        arr = WorldStates.GetStates(allState.ZoneId) ?? new WorldState[1];
                        int num = (int) ArrayUtil.AddOnlyOne<WorldState>(ref arr, allState);
                        WorldStates.ZoneStates[(int) allState.MapId] = arr;
                    }
                    else if (allState.MapId != MapId.End)
                    {
                        arr = WorldStates.GetStates(allState.MapId) ?? new WorldState[1];
                        int num = (int) ArrayUtil.AddOnlyOne<WorldState>(ref arr, allState);
                        WorldStates.MapStates[(int) allState.MapId] = arr;
                    }
                    else
                    {
                        int num = (int) ArrayUtil.AddOnlyOne<WorldState>(ref WorldStates.GlobalStates, allState);
                        arr = WorldStates.GlobalStates;
                    }

                    allState.Index = (uint) (arr.Length - 1);
                }
            }
        }

        private static void AddState(WorldState state)
        {
            WorldStates.AllStates[(int) state.Key] = state;
        }

        public static WorldState[] GetStates(MapId map)
        {
            return WorldStates.MapStates.Get<WorldState[]>((uint) map) ?? new WorldState[0];
        }

        public static WorldState[] GetStates(ZoneId zone)
        {
            return WorldStates.ZoneStates.Get<WorldState[]>((uint) zone) ?? new WorldState[0];
        }

        public static WorldState GetState(WorldStateId id)
        {
            return WorldStates.AllStates[(int) id];
        }

        private static void CreateStates()
        {
            WorldStates.AddState(new WorldState(2264U, 0));
            WorldStates.AddState(new WorldState(2263U, 0));
            WorldStates.AddState(new WorldState(2262U, 0));
            WorldStates.AddState(new WorldState(2261U, 0));
            WorldStates.AddState(new WorldState(2260U, 0));
            WorldStates.AddState(new WorldState(2259U, 0));
            WorldStates.AddState(new WorldState(3191U, 0));
            WorldStates.AddState(new WorldState(3901U, 1));
            WorldStates.AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGAllianceScore, 0));
            WorldStates.AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGHordeScore, 0));
            WorldStates.AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGAlliancePickupState, 0));
            WorldStates.AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGHordePickupState, 0));
            WorldStates.AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGUnknown, 2));
            WorldStates.AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGMaxScore, 3));
            WorldStates.AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGHordeFlagState, 1));
            WorldStates.AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGAllianceFlagState, 1));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABMaxResources, 1600));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABOccupiedBasesAlliance, 0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABOccupiedBasesHorde, 0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABResourcesAlliance, 0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABResourcesHorde, 0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowStableIcon, 1));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowStableIconAlliance, 0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowStableIconHorde, 0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowStableIconAllianceContested, 0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowStableIconHordeContested, 0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowGoldMineIcon, 1));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowGoldMineIconAlliance, 0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowGoldMineIconHorde, 0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowGoldMineIconAllianceContested,
                0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowGoldMineIconHordeContested, 0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowLumberMillIcon, 1));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowLumberMillIconAlliance, 0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowLumberMillIconHorde, 0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowLumberMillIconAllianceContested,
                0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowLumberMillIconHordeContested, 0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowFarmIcon, 1));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowFarmIconAlliance, 0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowFarmIconHorde, 0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowFarmIconAllianceContested, 0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowFarmIconHordeContested, 0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowBlacksmithIcon, 1));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowBlacksmithIconAlliance, 0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowBlacksmithIconHorde, 0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowBlacksmithIconAllianceContested,
                0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABShowBlacksmithIconHordeContested, 0));
            WorldStates.AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABNearVictoryWarning, 1400));
        }
    }
}
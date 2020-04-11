using WCell.RealmServer.Global;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
    public static class LocationUtil
    {
        public static bool IsValid(this IWorldLocation location, Unit user)
        {
            if (location.Position.Equals(new Vector3()))
                return false;
            if (location.Map == null)
                return user.Map.Id == location.MapId;
            return true;
        }

        public static Zone GetZone(this IWorldZoneLocation loc)
        {
            return loc.Map.GetZone(loc.ZoneId);
        }
    }
}
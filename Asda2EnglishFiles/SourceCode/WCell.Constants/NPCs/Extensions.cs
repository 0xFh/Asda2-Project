namespace WCell.Constants.NPCs
{
    public static class Extensions
    {
        public static bool HasAnyFlag(this VehicleFlags flags, VehicleFlags otherFlags)
        {
            return (flags & otherFlags) != (VehicleFlags) 0;
        }

        public static bool HasAnyFlag(this VehicleSeatFlags flags, VehicleSeatFlags otherFlags)
        {
            return (flags & otherFlags) != VehicleSeatFlags.None;
        }

        public static bool HasAnyFlag(this VehicleSeatFlagsB flags, VehicleSeatFlagsB otherFlags)
        {
            return (flags & otherFlags) != VehicleSeatFlagsB.None;
        }
    }
}
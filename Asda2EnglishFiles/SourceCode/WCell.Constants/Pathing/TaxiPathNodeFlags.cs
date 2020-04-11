using System;

namespace WCell.Constants.Pathing
{
    [Flags]
    public enum TaxiPathNodeFlags : byte
    {
        IsTeleport = 1,
        ArrivalOrDeparture = 2,
    }
}
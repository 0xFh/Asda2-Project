using System;

namespace WCell.Constants
{
    /// <summary>Used in UNIT_FIELD_BYTES_2, 2nd byte</summary>
    [Flags]
    public enum PvPState
    {
        None = 0,
        PVP = 1,
        FFAPVP = 4,
        InPvPSanctuary = 8,
    }
}
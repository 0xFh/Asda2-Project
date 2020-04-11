using System;

namespace WCell.Constants
{
    /// <summary>Used in UNIT_FIELD_BYTES_1, 3rd byte</summary>
    [Flags]
    public enum StateFlag
    {
        None = 0,
        AlwaysStand = 1,
        Sneaking = 2,
        UnTrackable = 4,
    }
}
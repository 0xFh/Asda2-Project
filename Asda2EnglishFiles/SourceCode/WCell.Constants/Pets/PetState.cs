using System;

namespace WCell.Constants.Pets
{
    /// <summary>Used in UNIT_FIELD_BYTES_2, 3rd byte</summary>
    [Flags]
    public enum PetState
    {
        CanBeRenamed = 1,
        CanBeAbandoned = 2,
    }
}
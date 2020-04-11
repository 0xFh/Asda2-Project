namespace WCell.Constants
{
    /// <summary>Used in UNIT_FIELD_BYTES_2, 1st byte</summary>
    public enum SheathType : sbyte
    {
        Undetermined = -1,
        None = 0,
        Melee = 1,
        Ranged = 2,
        Shield = 4,
        Rod = 5,
        Light = 7,
    }
}
using System;

namespace WCell.Constants.Factions
{
    /// <summary>A mask of the values from FactionGroup.dbc</summary>
    [Flags]
    public enum FactionGroupMask
    {
        None = 0,
        Player = 1,
        Alliance = 2,
        Horde = 4,
        Monster = 8,
    }
}
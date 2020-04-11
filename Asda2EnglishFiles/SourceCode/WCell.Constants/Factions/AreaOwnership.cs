using System;

namespace WCell.Constants.Factions
{
    [Flags]
    public enum AreaOwnership : uint
    {
        Contested = 0,
        Alliance = 2,
        Horde = 4,
        Sanctuary = Horde | Alliance, // 0x00000006
    }
}
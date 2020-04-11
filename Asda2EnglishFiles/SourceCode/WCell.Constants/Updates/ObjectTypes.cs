using System;

namespace WCell.Constants.Updates
{
    [Flags]
    public enum ObjectTypes : uint
    {
        None = 0,
        Object = 1,
        Item = 2,
        Container = 4,
        Unit = 8,
        Player = 16, // 0x00000010
        GameObject = 32, // 0x00000020
        Attackable = GameObject | Unit, // 0x00000028
        DynamicObject = 64, // 0x00000040
        Corpse = 128, // 0x00000080
        AIGroup = 256, // 0x00000100
        AreaTrigger = 512, // 0x00000200
        All = 65535, // 0x0000FFFF
    }
}
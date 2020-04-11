using System;

namespace WCell.Constants.Updates
{
    /// <summary>
    /// Custom enum to enable further distinction between Units
    /// </summary>
    [Flags]
    public enum ObjectTypeCustom
    {
        None = 0,
        Object = 1,
        Item = 2,
        Container = 6,
        Unit = 8,
        Player = 16, // 0x00000010
        GameObject = 32, // 0x00000020
        Attackable = GameObject | Unit, // 0x00000028
        DynamicObject = 64, // 0x00000040
        Corpse = 128, // 0x00000080
        AIGroup = 256, // 0x00000100
        AreaTrigger = 512, // 0x00000200
        NPC = 4096, // 0x00001000
        Pet = 8192, // 0x00002000
        All = 65535, // 0x0000FFFF
    }
}
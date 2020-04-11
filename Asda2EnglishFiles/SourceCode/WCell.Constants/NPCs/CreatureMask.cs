using System;

namespace WCell.Constants.NPCs
{
    /// <summary>Mask from CreatureType.dbc</summary>
    [Flags]
    public enum CreatureMask
    {
        None = 0,
        Beast = 1,
        Dragonkin = 2,
        Demon = 4,
        Elemental = 8,
        Giant = 16, // 0x00000010
        Undead = 32, // 0x00000020
        Humanoid = 64, // 0x00000040
        Critter = 128, // 0x00000080
        Mechanical = 256, // 0x00000100
        NotSpecified = 512, // 0x00000200
        Totem = 1024, // 0x00000400
        NonCombatPet = 2048, // 0x00000800
        GasCloud = 4096, // 0x00001000
    }
}
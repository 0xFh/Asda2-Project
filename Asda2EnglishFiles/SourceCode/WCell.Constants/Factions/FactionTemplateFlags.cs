using System;

namespace WCell.Constants.Factions
{
    /// <summary>Flags used in Faction Template DBCs</summary>
    [Flags]
    public enum FactionTemplateFlags : uint
    {
        None = 0,
        Flagx1 = 1,
        Flagx2 = 2,
        Flagx4 = 4,
        Flagx8 = 8,
        Flagx10 = 16, // 0x00000010
        Flagx20 = 32, // 0x00000020
        Flagx40 = 64, // 0x00000040
        Flagx80 = 128, // 0x00000080
        Flagx100 = 256, // 0x00000100
        Flagx200 = 512, // 0x00000200
        Flagx400 = 1024, // 0x00000400
        PvP = 2048, // 0x00000800
        ContestedGuard = 4096, // 0x00001000
        Flagx2000 = 8192, // 0x00002000
    }
}
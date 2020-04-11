using System;

namespace WCell.Constants.Spells
{
    /// <summary>
    /// Contains information needed for ProcTriggerFlags depending on hit result
    /// </summary>
    [Flags]
    public enum ProcHitFlags : uint
    {
        None = 0,
        NormalHit = 1,
        CriticalHit = 2,
        Hit = CriticalHit | NormalHit, // 0x00000003
        Miss = 4,
        Resist = 8,
        Dodge = 16, // 0x00000010
        Parry = 32, // 0x00000020
        Block = 64, // 0x00000040
        Evade = 128, // 0x00000080
        Immune = 256, // 0x00000100
        Deflect = 512, // 0x00000200
        Absorb = 1024, // 0x00000400
        Reflect = 2048, // 0x00000800
        Interrupt = 4096, // 0x00001000
        FullBlock = 8192, // 0x00002000

        All = FullBlock | Interrupt | Reflect | Absorb | Deflect | Immune | Evade | Block | Parry | Dodge | Resist |
              Miss | Hit, // 0x00003FFF
    }
}
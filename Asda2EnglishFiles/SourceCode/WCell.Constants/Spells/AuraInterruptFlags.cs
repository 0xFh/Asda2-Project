using System;

namespace WCell.Constants.Spells
{
    /// <summary>Events that can interrupt Auras</summary>
    [Flags]
    public enum AuraInterruptFlags : uint
    {
        None = 0,
        OnHostileSpellInflicted = 1,
        OnDamage = 2,
        Flag_0x4 = 4,
        OnMovement = 8,
        OnTurn = 16, // 0x00000010
        OnEnterCombat = 32, // 0x00000020
        OnDismount = 64, // 0x00000040
        OnEnterWater = 128, // 0x00000080
        OnLeaveWater = 256, // 0x00000100
        Flag_0x200 = 512, // 0x00000200
        Flag_0x400 = 1024, // 0x00000400
        Flag_0x800 = 2048, // 0x00000800
        OnStartAttack = 4096, // 0x00001000
        Flag_0x2000 = 8192, // 0x00002000
        Flag_0x4000 = 16384, // 0x00004000
        OnCast = 32768, // 0x00008000
        Flag_0x10000 = 65536, // 0x00010000
        OnMount = 131072, // 0x00020000
        OnStandUp = 262144, // 0x00040000
        OnLeaveArea = 524288, // 0x00080000
        OnInvincible = 1048576, // 0x00100000
        OnStealth = 2097152, // 0x00200000
        Flag_0x400000 = 4194304, // 0x00400000
        OnEnterPvP = 8388608, // 0x00800000
        OnDirectDamage = 16777216, // 0x01000000
        InterruptFlag0x2000000 = 33554432, // 0x02000000
        InterruptFlag0x4000000 = 67108864, // 0x04000000
        InterruptFlag0x8000000 = 134217728, // 0x08000000
        InterruptFlag0x10000000 = 268435456, // 0x10000000
        InterruptFlag0x20000000 = 536870912, // 0x20000000
        InterruptFlag0x40000000 = 1073741824, // 0x40000000
        InterruptFlag0x80000000 = 2147483648, // 0x80000000
    }
}
using System;

namespace WCell.Constants.Spells
{
    [Flags]
    public enum AuraStateMask : uint
    {
        None = 0,
        DodgeOrBlockOrParry = 1,
        Health20Percent = 2,
        Berserk = 4,
        Frozen = 8,
        Judgement = 16, // 0x00000010
        AuraState0x0020 = 32, // 0x00000020
        Parry = 64, // 0x00000040
        State0x0080 = 128, // 0x00000080
        State0x0100 = 256, // 0x00000100
        KillYieldedHonorOrXp = 512, // 0x00000200
        ScoredCriticalHit = 1024, // 0x00000400
        State0x0800 = 2048, // 0x00000800
        Health35Percent = 4096, // 0x00001000
        Immolate = 8192, // 0x00002000
        RejuvenationOrRegrowth = 16384, // 0x00004000
        DeadlyPoison = 32768, // 0x00008000
        Enraged = 65536, // 0x00010000
        Bleeding = 131072, // 0x00020000
        Hypothermia = 262144, // 0x00040000
        AuraState0x0080000 = 524288, // 0x00080000
        AuraState0x0100000 = 1048576, // 0x00100000
        AuraState0x0200000 = 2097152, // 0x00200000
        HealthAbove75Pct = 4194304, // 0x00400000
    }
}
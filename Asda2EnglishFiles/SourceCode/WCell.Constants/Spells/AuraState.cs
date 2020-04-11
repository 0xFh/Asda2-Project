namespace WCell.Constants.Spells
{
    public enum AuraState : uint
    {
        None = 0,
        DodgeOrBlockOrParry = 1,
        Health20Percent = 2,
        Berserk = 3,
        Frozen = 4,
        Judgement = 5,
        AuraState6 = 6,
        HunterParryRogueStealthAttack = 7,
        State0x0080 = 8,
        State0x0100 = 9,
        KillYieldedHonorOrXp = 10, // 0x0000000A
        ScoredCriticalHit = 11, // 0x0000000B
        StealthInvis = 12, // 0x0000000C
        Health35Percent = 13, // 0x0000000D
        Immolate = 14, // 0x0000000E
        RejuvenationOrRegrowth = 15, // 0x0000000F
        DeadlyPoison = 16, // 0x00000010
        Enraged = 17, // 0x00000011
        HealthAbove75Pct = 23, // 0x00000017
    }
}
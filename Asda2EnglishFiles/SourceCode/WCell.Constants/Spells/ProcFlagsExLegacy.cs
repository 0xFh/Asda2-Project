namespace WCell.Constants.Spells
{
    /// <summary>
    /// procEx flags from UDB's spell_proc_event database table
    /// </summary>
    public enum ProcFlagsExLegacy : uint
    {
        None = 0,
        NormalHit = 1,
        CriticalHit = 2,
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
        Reserved = 16384, // 0x00004000
        NotActiveSpell = 32768, // 0x00008000
        TriggerAlways = 65536, // 0x00010000
        OneTimeTrigger = 131072, // 0x00020000
        OnlyActiveSpell = 262144, // 0x00040000
    }
}
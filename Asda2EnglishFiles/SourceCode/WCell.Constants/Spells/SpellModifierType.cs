namespace WCell.Constants.Spells
{
    /// <summary>Used for AddModifier* AuraEffects</summary>
    public enum SpellModifierType
    {
        SpellPower = 0,
        Duration = 1,
        Threat = 2,
        EffectValue1 = 3,
        Charges = 4,
        Range = 5,
        Radius = 6,
        CritChance = 7,
        AllEffectValues = 8,
        PushbackReduction = 9,
        CastTime = 10, // 0x0000000A
        CooldownTime = 11, // 0x0000000B
        EffectValue2 = 12, // 0x0000000C
        PowerCost = 14, // 0x0000000E
        CritDamage = 15, // 0x0000000F
        TargetResistance = 16, // 0x00000010
        ChainTargets = 17, // 0x00000011
        ProcChance = 18, // 0x00000012
        ActivationTime = 19, // 0x00000013
        ChainValueFactor = 20, // 0x00000014
        GlobalCooldown = 21, // 0x00000015
        PeriodicEffectValue = 22, // 0x00000016
        EffectValue3 = 23, // 0x00000017
        SpellPowerEffect = 24, // 0x00000018
        HealingOrPowerGain = 27, // 0x0000001B
        DispelResistance = 28, // 0x0000001C
        RefundFailedFinishingMoveEnergy = 30, // 0x0000001E
        EffectValue4AndBeyond = 100, // 0x00000064
    }
}
using System;

namespace WCell.Constants.Spells
{
    /// <summary>
    /// Indicates events that let this Spell proc (if it is a proc spell)
    /// </summary>
    /// <remarks>
    /// Spells with ProcTriggerFlags have ProcTriggerSpell aura effects that are usually
    /// missing the id of the Spell to be casted.
    /// </remarks>
    [Flags]
    public enum ProcTriggerFlags : uint
    {
        None = 0,
        [Obsolete("Not used")] ScriptedAction = 1,
        KilledTargetThatYieldsExperienceOrHonor = 2,
        DoneMeleeAutoAttack = 4,
        ReceivedMeleeAutoAttack = 8,
        DoneMeleeSpell = 16, // 0x00000010
        ReceivedMeleeSpell = 32, // 0x00000020
        DoneRangedAutoAttack = 64, // 0x00000040
        ReceivedRangedAutoAttack = 128, // 0x00000080
        DoneRangedSpell = 256, // 0x00000100
        ReceivedRangedSpell = 512, // 0x00000200
        DoneBeneficialSpell = 1024, // 0x00000400
        ReceivedBeneficialSpell = 2048, // 0x00000800
        DoneHarmfulSpell = 4096, // 0x00001000
        ReceivedHarmfulSpell = 8192, // 0x00002000
        DoneBeneficialMagicSpell = 16384, // 0x00004000
        ReceivedBeneficialMagicSpell = 32768, // 0x00008000
        DoneHarmfulMagicSpell = 65536, // 0x00010000
        ReceivedHarmfulMagicSpell = 131072, // 0x00020000
        DonePeriodicDamageOrHeal = 262144, // 0x00040000
        ReceivedPeriodicDamageOrHeal = 524288, // 0x00080000
        ReceivedAnyDamage = 1048576, // 0x00100000
        TrapTriggered = 2097152, // 0x00200000
        [Obsolete("Not used")] DoneMeleeAttackWithMainHandWeapon = 4194304, // 0x00400000
        [Obsolete("Not used")] DoneMeleeAttackWithOffHandWeapon = 8388608, // 0x00800000
        Death = 16777216, // 0x01000000

        RequiringHitFlags = ReceivedPeriodicDamageOrHeal | DonePeriodicDamageOrHeal | ReceivedHarmfulMagicSpell |
                            DoneHarmfulMagicSpell | ReceivedBeneficialMagicSpell | DoneBeneficialMagicSpell |
                            ReceivedHarmfulSpell | DoneHarmfulSpell | ReceivedBeneficialSpell | DoneBeneficialSpell |
                            ReceivedRangedSpell | DoneRangedSpell | ReceivedRangedAutoAttack | DoneRangedAutoAttack |
                            ReceivedMeleeSpell | DoneMeleeSpell | ReceivedMeleeAutoAttack |
                            DoneMeleeAutoAttack, // 0x000FFFFC
    }
}
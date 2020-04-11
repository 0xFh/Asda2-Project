using System;

namespace WCell.Constants.Spells
{
    [Flags]
    public enum AuraTickFlags
    {
        None = 0,
        PeriodicDamage = 2,
        PeriodicTriggerSpell = 4,
        PeriodicHeal = 8,
        PeriodicLeech = 16, // 0x00000010
        PeriodicEnergize = 32, // 0x00000020
    }
}
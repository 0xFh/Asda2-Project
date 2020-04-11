using System;
using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Formulas
{
    /// <summary>
    /// Determines the amount of base power per level and regenaration speed of all powers
    /// </summary>
    public static class RegenerationFormulas
    {
        /// <summary>
        /// The standard factor to be applied to regen on every tick
        /// </summary>
        public static int RegenRateFactor = 1;

        /// <summary>The delay between 2 regeneration ticks in millis</summary>
        public static int RegenTickDelayMillis = 1000;

        /// <summary>
        /// The amount of milliseconds for the time of "Interrupted" power regen
        /// See: http://www.wowwiki.com/Formulas:Mana_Regen#Five_Second_Rule
        /// </summary>
        public static uint PowerRegenInterruptedCooldown = 5000;

        public static readonly RegenCalculator[] PowerRegenCalculators = new RegenCalculator[7];
        public static readonly RegenCalculator[] BasePowerForLevelCalculators = new RegenCalculator[7];

        public static void SetPowerRegenCalculator(PowerType type, RegenCalculator calc)
        {
            RegenerationFormulas.PowerRegenCalculators[(int) type] = calc;
        }

        public static void SetBasePowerCalculator(PowerType type, RegenCalculator calc)
        {
            RegenerationFormulas.BasePowerForLevelCalculators[(int) type] = calc;
        }

        static RegenerationFormulas()
        {
            RegenerationFormulas.SetPowerRegenCalculator(PowerType.Mana,
                new RegenCalculator(RegenerationFormulas.CalculateManaRegen));
            RegenerationFormulas.SetPowerRegenCalculator(PowerType.Rage,
                new RegenCalculator(RegenerationFormulas.CalculateRageRegen));
            RegenerationFormulas.SetPowerRegenCalculator(PowerType.Energy,
                new RegenCalculator(RegenerationFormulas.CalculateEnergyRegen));
            RegenerationFormulas.SetPowerRegenCalculator(PowerType.Focus,
                new RegenCalculator(RegenerationFormulas.CalculateFocusRegen));
            RegenerationFormulas.SetPowerRegenCalculator(PowerType.RunicPower,
                new RegenCalculator(RegenerationFormulas.CalculateRunicPowerRegen));
            RegenerationFormulas.SetPowerRegenCalculator(PowerType.Runes,
                new RegenCalculator(RegenerationFormulas.CalculateRuneRegen));
            RegenerationFormulas.SetBasePowerCalculator(PowerType.Mana,
                new RegenCalculator(RegenerationFormulas.GetPowerForLevelDefault));
            RegenerationFormulas.SetBasePowerCalculator(PowerType.Rage,
                new RegenCalculator(RegenerationFormulas.GetRageForLevel));
            RegenerationFormulas.SetBasePowerCalculator(PowerType.Energy,
                new RegenCalculator(RegenerationFormulas.GetEnergyForLevel));
            RegenerationFormulas.SetBasePowerCalculator(PowerType.Focus,
                new RegenCalculator(RegenerationFormulas.GetFocusForLevel));
            RegenerationFormulas.SetBasePowerCalculator(PowerType.RunicPower,
                new RegenCalculator(RegenerationFormulas.GetRunicPowerForLevel));
            RegenerationFormulas.SetBasePowerCalculator(PowerType.Runes,
                new RegenCalculator(RegenerationFormulas.GetRunesForLevel));
        }

        public static int GetPowerRegen(Unit unit)
        {
            RegenCalculator powerRegenCalculator = RegenerationFormulas.PowerRegenCalculators[(int) unit.PowerType];
            if (powerRegenCalculator == null)
                return 0;
            return powerRegenCalculator(unit);
        }

        /// <summary>
        /// Calculates the amount of power regeneration for the class at a specific level, Intellect and Spirit.
        /// Changed in 3.1, overrides for casters are redundant.
        /// </summary>
        public static int CalculateManaRegen(Unit unit)
        {
            return (int) ((1.0 / 1000.0 + Math.Pow((double) unit.Spirit, 0.5)) * 0.600000023841858) *
                   RegenerationFormulas.RegenRateFactor;
        }

        /// <summary>1 Rage to the client is a value of 10</summary>
        public static int CalculateRageRegen(Unit unit)
        {
            if (unit.IsInCombat)
                return 0;
            return -10 * RegenerationFormulas.RegenRateFactor;
        }

        public static int CalculateEnergyRegen(Unit unit)
        {
            return 10 * RegenerationFormulas.RegenRateFactor;
        }

        public static int CalculateRunicPowerRegen(Unit unit)
        {
            return -10 * RegenerationFormulas.RegenRateFactor;
        }

        private static int CalculateRuneRegen(Unit unit)
        {
            return 0;
        }

        public static int CalculateFocusRegen(Unit unit)
        {
            return 5 * RegenerationFormulas.RegenRateFactor;
        }

        public static int GetPowerForLevel(Unit unit)
        {
            return (RegenerationFormulas.BasePowerForLevelCalculators[(int) unit.PowerType] ??
                    new RegenCalculator(RegenerationFormulas.GetPowerForLevelDefault))(unit);
        }

        public static int GetPowerForLevelDefault(Unit unit)
        {
            return unit.GetBaseClass().GetLevelSetting(unit.Level).Mana;
        }

        public static int GetRageForLevel(Unit unit)
        {
            return 1000;
        }

        public static int GetRunicPowerForLevel(Unit unit)
        {
            return 1000;
        }

        public static int GetRunesForLevel(Unit unit)
        {
            return 6;
        }

        public static int GetFocusForLevel(Unit unit)
        {
            return 5 * unit.Level;
        }

        public static int GetEnergyForLevel(Unit unit)
        {
            return 100;
        }

        public static int GetZero(Unit unit)
        {
            return 0;
        }
    }
}
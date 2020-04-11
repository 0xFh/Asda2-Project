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
      PowerRegenCalculators[(int) type] = calc;
    }

    public static void SetBasePowerCalculator(PowerType type, RegenCalculator calc)
    {
      BasePowerForLevelCalculators[(int) type] = calc;
    }

    static RegenerationFormulas()
    {
      SetPowerRegenCalculator(PowerType.Mana,
        CalculateManaRegen);
      SetPowerRegenCalculator(PowerType.Rage,
        CalculateRageRegen);
      SetPowerRegenCalculator(PowerType.Energy,
        CalculateEnergyRegen);
      SetPowerRegenCalculator(PowerType.Focus,
        CalculateFocusRegen);
      SetPowerRegenCalculator(PowerType.RunicPower,
        CalculateRunicPowerRegen);
      SetPowerRegenCalculator(PowerType.Runes,
        CalculateRuneRegen);
      SetBasePowerCalculator(PowerType.Mana,
        GetPowerForLevelDefault);
      SetBasePowerCalculator(PowerType.Rage,
        GetRageForLevel);
      SetBasePowerCalculator(PowerType.Energy,
        GetEnergyForLevel);
      SetBasePowerCalculator(PowerType.Focus,
        GetFocusForLevel);
      SetBasePowerCalculator(PowerType.RunicPower,
        GetRunicPowerForLevel);
      SetBasePowerCalculator(PowerType.Runes,
        GetRunesForLevel);
    }

    public static int GetPowerRegen(Unit unit)
    {
      RegenCalculator powerRegenCalculator = PowerRegenCalculators[(int) unit.PowerType];
      if(powerRegenCalculator == null)
        return 0;
      return powerRegenCalculator(unit);
    }

    /// <summary>
    /// Calculates the amount of power regeneration for the class at a specific level, Intellect and Spirit.
    /// Changed in 3.1, overrides for casters are redundant.
    /// </summary>
    public static int CalculateManaRegen(Unit unit)
    {
      return (int) ((1.0 / 1000.0 + Math.Pow(unit.Spirit, 0.5)) * 0.600000023841858) *
             RegenRateFactor;
    }

    /// <summary>1 Rage to the client is a value of 10</summary>
    public static int CalculateRageRegen(Unit unit)
    {
      if(unit.IsInCombat)
        return 0;
      return -10 * RegenRateFactor;
    }

    public static int CalculateEnergyRegen(Unit unit)
    {
      return 10 * RegenRateFactor;
    }

    public static int CalculateRunicPowerRegen(Unit unit)
    {
      return -10 * RegenRateFactor;
    }

    private static int CalculateRuneRegen(Unit unit)
    {
      return 0;
    }

    public static int CalculateFocusRegen(Unit unit)
    {
      return 5 * RegenRateFactor;
    }

    public static int GetPowerForLevel(Unit unit)
    {
      return (BasePowerForLevelCalculators[(int) unit.PowerType] ??
              GetPowerForLevelDefault)(unit);
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
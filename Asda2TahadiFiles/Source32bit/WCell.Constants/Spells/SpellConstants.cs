using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Util;

namespace WCell.Constants.Spells
{
  public static class SpellConstants
  {
    /// <summary>
    /// Several Hashsets containing all SpellMechanics that can toggle
    /// CanHarm, CanMove and CanCastSpells respectively
    /// </summary>
    public static readonly bool[] HarmPreventionMechanics = new bool[32];

    public static readonly bool[] MoveMechanics = new bool[32];
    public static readonly bool[] InteractMechanics = new bool[32];
    public static readonly bool[] SpellCastPreventionMechanics = new bool[32];
    public static readonly bool[] NegativeMechanics = new bool[32];

    /// <summary>
    /// Amount of bits that are necessary to store a single rune's type.
    /// Start counting from 1 to End, instead of 0 to End - 1
    /// </summary>
    public static readonly int BitsPerRune = (int) (Math.Log(5.0, 2.0) + 0.9999999);

    /// <summary>
    /// BitsPerRune 1 bits to mask away anything but a single rune's bit set
    /// </summary>
    public static readonly int SingleRuneFullBitMask = (1 << BitsPerRune) - 1;

    public static readonly uint[,] IndicesPerType = new uint[3, 2]
    {
      {
        0U,
        1U
      },
      {
        2U,
        3U
      },
      {
        4U,
        5U
      }
    };

    /// <summary>Default rune layout, 2 of every kind, in this order</summary>
    public static readonly RuneType[] DefaultRuneSet = new RuneType[6]
    {
      RuneType.Blood,
      RuneType.Blood,
      RuneType.Unholy,
      RuneType.Unholy,
      RuneType.Frost,
      RuneType.Frost
    };

    public static readonly DamageSchool[] AllDamageSchools =
      ((IEnumerable<DamageSchool>) Enum.GetValues(typeof(DamageSchool))).Except(
        new DamageSchool[1]
        {
          DamageSchool.Count
        }).ToArray();

    public static readonly uint[] AllDamageSchoolSet = Utility.GetSetIndices((uint) sbyte.MaxValue);
    public static readonly uint[] MagicDamageSchoolSet = Utility.GetSetIndices(126U);
    public const int SpellClassMaskSize = 3;

    /// <summary>Amount of different types of runes (3)</summary>
    public const int StandardRuneTypeCount = 3;

    public const int MaxRuneCount = 6;

    /// <summary>Amount of runes per type (usually 2)</summary>
    public const int MaxRuneCountPerType = 2;

    public const int MinHitChance = 0;
    public const int CharacterMinHitChance = 1;
    public const int MaxHitChance = 100;
    public const int HitChanceForEqualLevel = 96;
    public const int HitChancePerLevelPvP = 7;
    public const int HitChancePerLevelPvE = 11;

    static SpellConstants()
    {
      MoveMechanics[2] = true;
      MoveMechanics[10] = true;
      MoveMechanics[14] = true;
      MoveMechanics[13] = true;
      MoveMechanics[7] = true;
      MoveMechanics[12] = true;
      MoveMechanics[24] = true;
      MoveMechanics[20] = true;
      MoveMechanics[23] = true;
      MoveMechanics[30] = true;
      MoveMechanics[11] = true;
      InteractMechanics[2] = true;
      InteractMechanics[10] = true;
      InteractMechanics[1] = true;
      InteractMechanics[13] = true;
      InteractMechanics[14] = true;
      InteractMechanics[12] = true;
      InteractMechanics[5] = true;
      InteractMechanics[24] = true;
      InteractMechanics[20] = true;
      InteractMechanics[23] = true;
      InteractMechanics[30] = true;
      HarmPreventionMechanics[5] = true;
      HarmPreventionMechanics[13] = true;
      HarmPreventionMechanics[16] = true;
      HarmPreventionMechanics[18] = true;
      HarmPreventionMechanics[14] = true;
      HarmPreventionMechanics[12] = true;
      SpellCastPreventionMechanics[9] = true;
      SpellCastPreventionMechanics[2] = true;
      SpellCastPreventionMechanics[5] = true;
      SpellCastPreventionMechanics[14] = true;
      SpellCastPreventionMechanics[10] = true;
      SpellCastPreventionMechanics[1] = true;
      SpellCastPreventionMechanics[18] = true;
      SpellCastPreventionMechanics[24] = true;
      SpellCastPreventionMechanics[23] = true;
      SpellCastPreventionMechanics[12] = true;
      SpellCastPreventionMechanics[13] = true;
      foreach(SpellMechanic spellMechanic in Enum.GetValues(typeof(SpellMechanic)))
      {
        if(spellMechanic >= SpellMechanic.End)
          break;
        if(MoveMechanics[(int) spellMechanic] ||
           InteractMechanics[(int) spellMechanic] ||
           HarmPreventionMechanics[(int) spellMechanic] ||
           SpellCastPreventionMechanics[(int) spellMechanic])
          NegativeMechanics[(int) spellMechanic] = true;
      }
    }

    public static bool IsNegative(this SpellMechanic mech)
    {
      return NegativeMechanics[(int) mech];
    }
  }
}
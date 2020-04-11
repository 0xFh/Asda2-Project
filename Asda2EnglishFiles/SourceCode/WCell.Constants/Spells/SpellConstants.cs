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
        public static readonly int SingleRuneFullBitMask = (1 << SpellConstants.BitsPerRune) - 1;

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
            ((IEnumerable<DamageSchool>) Enum.GetValues(typeof(DamageSchool))).Except<DamageSchool>(
                (IEnumerable<DamageSchool>) new DamageSchool[1]
                {
                    DamageSchool.Count
                }).ToArray<DamageSchool>();

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
            SpellConstants.MoveMechanics[2] = true;
            SpellConstants.MoveMechanics[10] = true;
            SpellConstants.MoveMechanics[14] = true;
            SpellConstants.MoveMechanics[13] = true;
            SpellConstants.MoveMechanics[7] = true;
            SpellConstants.MoveMechanics[12] = true;
            SpellConstants.MoveMechanics[24] = true;
            SpellConstants.MoveMechanics[20] = true;
            SpellConstants.MoveMechanics[23] = true;
            SpellConstants.MoveMechanics[30] = true;
            SpellConstants.MoveMechanics[11] = true;
            SpellConstants.InteractMechanics[2] = true;
            SpellConstants.InteractMechanics[10] = true;
            SpellConstants.InteractMechanics[1] = true;
            SpellConstants.InteractMechanics[13] = true;
            SpellConstants.InteractMechanics[14] = true;
            SpellConstants.InteractMechanics[12] = true;
            SpellConstants.InteractMechanics[5] = true;
            SpellConstants.InteractMechanics[24] = true;
            SpellConstants.InteractMechanics[20] = true;
            SpellConstants.InteractMechanics[23] = true;
            SpellConstants.InteractMechanics[30] = true;
            SpellConstants.HarmPreventionMechanics[5] = true;
            SpellConstants.HarmPreventionMechanics[13] = true;
            SpellConstants.HarmPreventionMechanics[16] = true;
            SpellConstants.HarmPreventionMechanics[18] = true;
            SpellConstants.HarmPreventionMechanics[14] = true;
            SpellConstants.HarmPreventionMechanics[12] = true;
            SpellConstants.SpellCastPreventionMechanics[9] = true;
            SpellConstants.SpellCastPreventionMechanics[2] = true;
            SpellConstants.SpellCastPreventionMechanics[5] = true;
            SpellConstants.SpellCastPreventionMechanics[14] = true;
            SpellConstants.SpellCastPreventionMechanics[10] = true;
            SpellConstants.SpellCastPreventionMechanics[1] = true;
            SpellConstants.SpellCastPreventionMechanics[18] = true;
            SpellConstants.SpellCastPreventionMechanics[24] = true;
            SpellConstants.SpellCastPreventionMechanics[23] = true;
            SpellConstants.SpellCastPreventionMechanics[12] = true;
            SpellConstants.SpellCastPreventionMechanics[13] = true;
            foreach (SpellMechanic spellMechanic in Enum.GetValues(typeof(SpellMechanic)))
            {
                if (spellMechanic >= SpellMechanic.End)
                    break;
                if (SpellConstants.MoveMechanics[(int) spellMechanic] ||
                    SpellConstants.InteractMechanics[(int) spellMechanic] ||
                    SpellConstants.HarmPreventionMechanics[(int) spellMechanic] ||
                    SpellConstants.SpellCastPreventionMechanics[(int) spellMechanic])
                    SpellConstants.NegativeMechanics[(int) spellMechanic] = true;
            }
        }

        public static bool IsNegative(this SpellMechanic mech)
        {
            return SpellConstants.NegativeMechanics[(int) mech];
        }
    }
}
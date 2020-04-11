using System;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Formulas
{
    public static class FallDamageGenerator
    {
        /// <summary>The coefficient the distance fallen is multiplied by</summary>
        public static float DefaultFallDamageCoefficient = 0.018f;

        /// <summary>The amount substracted from the fall damage</summary>
        public static float DefaultFallDamageReduceAmount = 0.2426f;

        /// <summary>
        /// The coefficient the final fall damage is multiplied by
        /// </summary>
        public static float DefaultFallDamageRate = 1f;

        /// <summary>
        /// The amount of damage inflicted to a character for fall
        /// </summary>
        public static Func<Character, float, int> GetFallDmg = (Func<Character, float, int>) ((chr, fallenDistance) =>
        {
            float num1 = fallenDistance - (float) chr.SafeFall;
            float num2 = FallDamageGenerator.DefaultFallDamageCoefficient * num1 -
                         FallDamageGenerator.DefaultFallDamageReduceAmount;
            if ((double) num2 < 0.0)
                return 0;
            float num3 = num2 * FallDamageGenerator.DefaultFallDamageRate;
            return (int) ((double) chr.MaxHealth * (double) num3);
        });
    }
}
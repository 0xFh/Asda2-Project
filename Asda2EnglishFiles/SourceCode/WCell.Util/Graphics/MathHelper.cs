using System;

namespace WCell.Util.Graphics
{
    public static class MathHelper
    {
        public static float ToRadians(float degrees)
        {
            return degrees * ((float) Math.PI / 180f);
        }

        public static float ToDegrees(float radians)
        {
            return radians * 57.29578f;
        }

        internal static float Max(float f1, float f2)
        {
            return (double) f1 > (double) f2 ? f1 : f2;
        }

        internal static float Min(float f1, float f2)
        {
            return (double) f1 < (double) f2 ? f1 : f2;
        }
    }
}
using System;
using System.Threading;

namespace WCell.Util
{
    public static class MathUtil
    {
        /// <summary>PI with less precision but faster</summary>
        public const float PI = 3.141593f;

        /// <summary>PI with less precision but faster</summary>
        public const float TwoPI = 6.283185f;

        /// <summary>E with less precision but faster</summary>
        public const float E = 2.718282f;

        public const float Epsilonf = 0.001f;

        /// <summary>1 degree = 0.0174532925 radians</summary>
        public const float RadiansPerDegree = 0.01745329f;

        /// <summary>TODO: Implement faster version</summary>
        public static int CeilingInt(float value)
        {
            return (int) Math.Ceiling((double) value);
        }

        /// <summary>
        /// Unprecise but fast (don't use for values greater or smaller than integer range (+-2 billion))
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float Round(float value)
        {
            return (float) (int) ((double) value + 0.5);
        }

        public static uint RoundUInt(float value)
        {
            return (uint) ((double) value + 0.5);
        }

        public static int RoundInt(float value)
        {
            return (int) ((double) value + 0.5);
        }

        /// <summary>Divides and returns a rounded result</summary>
        /// <param name="nominator"></param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        public static uint Divide(uint nominator, uint divisor)
        {
            return (nominator + divisor / 2U) / divisor;
        }

        public static int Divide(int nominator, int divisor)
        {
            return (nominator + divisor / 2) / divisor;
        }

        /// <summary>Clamps a number by an upper and lower limit.</summary>
        /// <remarks>This method effectively lets you bound a value between two values, as in
        /// if you have bounds of 0 - 100, and a number is 130, it'll clamp it to 100, and vise versa.</remarks>
        /// <param name="value">the value to clamp</param>
        /// <param name="min">the minimum bound</param>
        /// <param name="max">the maximum bound</param>
        /// <returns>either the original number, or a clamped number based on the upper/lower bounds</returns>
        public static short ClampMinMax(short value, short min, short max)
        {
            if ((int) value > (int) max)
                return max;
            if ((int) value < (int) min)
                return min;
            return value;
        }

        /// <summary>Clamps a number by an upper and lower limit.</summary>
        /// <remarks>This method effectively lets you bound a value between two values, as in
        /// if you have bounds of 0 - 100, and a number is 130, it'll clamp it to 100, and vise versa.</remarks>
        /// <param name="value">the value to clamp</param>
        /// <param name="min">the minimum bound</param>
        /// <param name="max">the maximum bound</param>
        /// <returns>either the original number, or a clamped number based on the upper/lower bounds</returns>
        public static ushort ClampMinMax(ushort value, ushort min, ushort max)
        {
            if ((int) value > (int) max)
                return max;
            if ((int) value < (int) min)
                return min;
            return value;
        }

        /// <summary>Clamps a number by an upper and lower limit.</summary>
        /// <remarks>This method effectively lets you bound a value between two values, as in
        /// if you have bounds of 0 - 100, and a number is 130, it'll clamp it to 100, and vise versa.</remarks>
        /// <param name="value">the value to clamp</param>
        /// <param name="min">the minimum bound</param>
        /// <param name="max">the maximum bound</param>
        /// <returns>either the original number, or a clamped number based on the upper/lower bounds</returns>
        public static int ClampMinMax(int value, int min, int max)
        {
            if (value > max)
                return max;
            if (value < min)
                return min;
            return value;
        }

        /// <summary>Clamps a number by an upper and lower limit.</summary>
        /// <remarks>This method effectively lets you bound a value between two values, as in
        /// if you have bounds of 0 - 100, and a number is 130, it'll clamp it to 100, and vise versa.</remarks>
        /// <param name="value">the value to clamp</param>
        /// <param name="min">the minimum bound</param>
        /// <param name="max">the maximum bound</param>
        /// <returns>either the original number, or a clamped number based on the upper/lower bounds</returns>
        public static uint ClampMinMax(uint value, uint min, uint max)
        {
            if (value > max)
                return max;
            if (value < min)
                return min;
            return value;
        }

        /// <summary>Clamps a number by an upper and lower limit.</summary>
        /// <remarks>This method effectively lets you bound a value between two values, as in
        /// if you have bounds of 0 - 100, and a number is 130, it'll clamp it to 100, and vise versa.</remarks>
        /// <param name="value">the value to clamp</param>
        /// <param name="min">the minimum bound</param>
        /// <param name="max">the maximum bound</param>
        /// <returns>either the original number, or a clamped number based on the upper/lower bounds</returns>
        public static long ClampMinMax(long value, long min, long max)
        {
            if (value > max)
                return max;
            if (value < min)
                return min;
            return value;
        }

        /// <summary>Clamps a number by an upper and lower limit.</summary>
        /// <remarks>This method effectively lets you bound a value between two values, as in
        /// if you have bounds of 0 - 100, and a number is 130, it'll clamp it to 100, and vise versa.</remarks>
        /// <param name="value">the value to clamp</param>
        /// <param name="min">the minimum bound</param>
        /// <param name="max">the maximum bound</param>
        /// <returns>either the original number, or a clamped number based on the upper/lower bounds</returns>
        public static ulong ClampMinMax(ulong value, ulong min, ulong max)
        {
            if (value > max)
                return max;
            if (value < min)
                return min;
            return value;
        }

        /// <summary>Clamps a number by an upper and lower limit.</summary>
        /// <remarks>This method effectively lets you bound a value between two values, as in
        /// if you have bounds of 0 - 100, and a number is 130, it'll clamp it to 100, and vise versa.</remarks>
        /// <param name="value">the value to clamp</param>
        /// <param name="min">the minimum bound</param>
        /// <param name="max">the maximum bound</param>
        /// <returns>either the original number, or a clamped number based on the upper/lower bounds</returns>
        public static float ClampMinMax(float value, float min, float max)
        {
            if ((double) value > (double) max)
                return max;
            if ((double) value < (double) min)
                return min;
            return value;
        }

        /// <summary>Clamps a number by an upper and lower limit.</summary>
        /// <remarks>This method effectively lets you bound a value between two values, as in
        /// if you have bounds of 0 - 100, and a number is 130, it'll clamp it to 100, and vise versa.</remarks>
        /// <param name="value">the value to clamp</param>
        /// <param name="min">the minimum bound</param>
        /// <param name="max">the maximum bound</param>
        /// <returns>either the original number, or a clamped number based on the upper/lower bounds</returns>
        public static double ClampMinMax(double value, double min, double max)
        {
            if (value > max)
                return max;
            if (value < min)
                return min;
            return value;
        }

        /// <summary>Clamps a number by an upper and lower limit.</summary>
        /// <remarks>This method effectively lets you bound a value between two values, as in
        /// if you have bounds of 0 - 100, and a number is 130, it'll clamp it to 100, and vise versa.</remarks>
        /// <param name="value">the value to clamp</param>
        /// <param name="min">the minimum bound</param>
        /// <param name="max">the maximum bound</param>
        /// <returns>either the original number, or a clamped number based on the upper/lower bounds</returns>
        public static Decimal ClampMinMax(Decimal value, Decimal min, Decimal max)
        {
            if (value > max)
                return max;
            if (value < min)
                return min;
            return value;
        }

        public static int FastInterlockedAdd(ref int location, int value)
        {
            if (value == 1)
                return Interlocked.Increment(ref location);
            if (value == -1)
                return Interlocked.Decrement(ref location);
            return Interlocked.Add(ref location, value);
        }

        public static long FastInterlockedAdd(ref long location, long value)
        {
            if (value == 1L)
                return Interlocked.Increment(ref location);
            if (value == -1L)
                return Interlocked.Decrement(ref location);
            return Interlocked.Add(ref location, value);
        }

        /// <summary>
        /// Counts 1's , linear in the number of 1's in the given number
        /// </summary>
        public static int CountBits(int number)
        {
            int num = 0;
            while (number != 0)
            {
                ++num;
                number &= number - 1;
            }

            return num;
        }
    }
}
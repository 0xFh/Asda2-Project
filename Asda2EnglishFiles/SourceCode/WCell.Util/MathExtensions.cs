using System;
using WCell.Util.Graphics;

namespace WCell.Util
{
    public static class MathExtensions
    {
        public const float Epsilon = 1E-05f;

        public static bool NearlyZero(this float val)
        {
            return (double) val < 9.99999974737875E-06 && (double) val > -9.99999974737875E-06;
        }

        public static float Cos(this float angle)
        {
            return (float) Math.Cos((double) angle);
        }

        public static float CosABS(this float angle)
        {
            return (float) Math.Abs(Math.Cos((double) angle));
        }

        public static float Sin(this float angle)
        {
            return (float) Math.Sin((double) angle);
        }

        public static bool NearlyEqual(this Vector3 v1, Vector3 v2, float delta)
        {
            if (!(v2.X - v1.X).NearlyZero() || !(v2.Y - v1.Y).NearlyZero())
                return false;
            return (v2.Z - v1.Z).NearlyZero();
        }

        public static bool NearlyEquals(this float f1, float f2)
        {
            return (f2 - f1).NearlyZero();
        }

        public static Vector3 ABS(this Vector3 v)
        {
            return new Vector3(Math.Abs(v.X), Math.Abs(v.Y), Math.Abs(v.Z));
        }

        public static Plane PlaneFromPointNormal(Vector3 point, Vector3 normal)
        {
            return new Plane(normal, -Vector3.Dot(normal, point));
        }

        public static Vector3 NormalFromPoints(Vector3 a, Vector3 b, Vector3 c)
        {
            return Vector3.Normalize(Vector3.Cross(a - b, c - b));
        }
    }
}
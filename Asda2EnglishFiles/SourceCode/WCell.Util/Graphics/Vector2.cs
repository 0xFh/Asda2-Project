using System;
using System.Runtime.InteropServices;

namespace WCell.Util.Graphics
{
    /// <summary>Defines a vector with two components.</summary>
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct Vector2 : IEquatable<Vector2>
    {
        public static readonly Vector2 Zero = new Vector2(0.0f, 0.0f);

        /// <summary>The X component of the vector.</summary>
        [FieldOffset(0)] public float X;

        /// <summary>The Y component of the vector.</summary>
        [FieldOffset(4)] public float Y;

        /// <summary>
        /// Creates a new <see cref="T:WCell.Util.Graphics.Vector2" /> with the given X and Y components.
        /// </summary>
        /// <param name="x">the X component</param>
        /// <param name="y">the Y component</param>
        public Vector2(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Clamps the values of the vector to be within a specified range.
        /// </summary>
        /// <param name="min">the minimum value</param>
        /// <param name="max">the maximum value</param>
        /// <returns>a new <see cref="T:WCell.Util.Graphics.Vector2" /> that has been clamped within the specified range</returns>
        public Vector2 Clamp(ref Vector2 min, ref Vector2 max)
        {
            float x1 = this.X;
            float num1 = (double) x1 > (double) max.X ? max.X : x1;
            float x2 = (double) num1 < (double) min.X ? min.X : num1;
            float y1 = this.Y;
            float num2 = (double) y1 > (double) max.Y ? max.Y : y1;
            float y2 = (double) num2 < (double) min.Y ? min.Y : num2;
            return new Vector2(x2, y2);
        }

        /// <summary>Calculates the distance from this vector to another.</summary>
        /// <param name="point">the second <see cref="T:WCell.Util.Graphics.Vector2" /></param>
        /// <returns>the distance between the vectors</returns>
        public float GetDistance(ref Vector2 point)
        {
            float num1 = point.X - this.X;
            float num2 = point.Y - this.Y;
            return (float) Math.Sqrt((double) num1 * (double) num1 + (double) num2 * (double) num2);
        }

        /// <summary>
        /// Calculates the distance squared from this vector to another.
        /// </summary>
        /// <param name="point">the second <see cref="T:WCell.Util.Graphics.Vector2" /></param>
        /// <returns>the distance squared between the vectors</returns>
        public float GetDistanceSquared(ref Vector2 point)
        {
            float num1 = point.X - this.X;
            float num2 = point.Y - this.Y;
            return (float) ((double) num1 * (double) num1 + (double) num2 * (double) num2);
        }

        public float Length()
        {
            return (float) Math.Sqrt((double) this.X * (double) this.X + (double) this.Y * (double) this.Y);
        }

        public float LengthSquared()
        {
            return (float) ((double) this.X * (double) this.X + (double) this.Y * (double) this.Y);
        }

        /// <summary>Subtracts vector b from vector a.</summary>
        /// <param name="a">The vector to subtract from.</param>
        /// <param name="b">The subtracting vector.</param>
        /// <param name="result">A Vector2 filled with the result of (a - b).</param>
        public static void Subtract(ref Vector2 a, ref Vector2 b, out Vector2 result)
        {
            result.X = a.X - b.X;
            result.Y = a.Y - b.Y;
        }

        /// <summary>Turns the current vector into a unit vector.</summary>
        /// <remarks>The vector becomes one unit in length and points in the same direction of the original vector.</remarks>
        public void Normalize()
        {
            float num = 1f / (float) Math.Sqrt((double) this.X * (double) this.X + (double) this.Y * (double) this.Y);
            this.X *= num;
            this.Y *= num;
        }

        public Vector2 NormalizedCopy()
        {
            float num = 1f / (float) Math.Sqrt((double) this.X * (double) this.X + (double) this.Y * (double) this.Y);
            return new Vector2(this.X * num, this.Y * num);
        }

        public static float Dot(Vector2 value1, Vector2 value2)
        {
            return (float) ((double) value1.X * (double) value2.X + (double) value1.Y * (double) value2.Y);
        }

        public static void Dot(ref Vector2 value1, ref Vector2 value2, out float result)
        {
            result = (float) ((double) value1.X * (double) value2.X + (double) value1.Y * (double) value2.Y);
        }

        public Vector2 RightNormal()
        {
            return new Vector2(this.Y, -this.X);
        }

        public bool LerpZ(Vector3 start, Vector3 end, out float newZ)
        {
            newZ = float.MinValue;
            if ((end.X - start.X).IsWithinEpsilon(0.0f))
                return false;
            float num = (float) (((double) this.X - (double) start.X) / ((double) end.X - (double) start.X));
            newZ = start.Z + num * (end.Z - start.Z);
            return true;
        }

        /// <summary>Checks equality of two vectors.</summary>
        /// <param name="other">the other vector to compare with</param>
        /// <returns>true if both vectors are equal; false otherwise</returns>
        public bool Equals(Vector2 other)
        {
            return (double) this.X == (double) other.X && (double) this.Y == (double) other.Y;
        }

        /// <summary>Checks equality with another object.</summary>
        /// <param name="obj">the object to compare</param>
        /// <returns>true if the object is <see cref="T:WCell.Util.Graphics.Vector2" /> and is equal; false otherwise</returns>
        public override bool Equals(object obj)
        {
            return obj is Vector2 && this.Equals((Vector2) obj);
        }

        public override int GetHashCode()
        {
            return this.X.GetHashCode() + this.Y.GetHashCode();
        }

        public static bool operator ==(Vector2 a, Vector2 b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Vector2 a, Vector2 b)
        {
            return (double) a.X != (double) b.X || (double) a.Y != (double) b.Y;
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2 operator *(float f, Vector2 a)
        {
            return new Vector2(f * a.X, f * a.Y);
        }

        public static Vector2 operator *(Vector2 a, float f)
        {
            return new Vector2(f * a.X, f * a.Y);
        }

        public static Vector2 operator /(Vector2 a, Vector2 b)
        {
            Vector2 vector2;
            vector2.X = a.X / b.X;
            vector2.Y = a.Y / b.Y;
            return vector2;
        }

        public static Vector2 operator /(Vector2 a, float scaleFactor)
        {
            float num = 1f / scaleFactor;
            Vector2 vector2;
            vector2.X = a.X * num;
            vector2.Y = a.Y * num;
            return vector2;
        }

        public override string ToString()
        {
            return string.Format("(X:{0}, Y:{1})", (object) this.X, (object) this.Y);
        }
    }
}
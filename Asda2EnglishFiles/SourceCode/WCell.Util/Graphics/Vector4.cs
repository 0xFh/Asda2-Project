using System;
using System.Runtime.InteropServices;

namespace WCell.Util.Graphics
{
    /// <summary>Defines a vector with four components.</summary>
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct Vector4 : IEquatable<Vector4>
    {
        public static readonly Vector4 Zero = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

        /// <summary>The X component of the vector.</summary>
        [FieldOffset(0)] public float X;

        /// <summary>The Y component of the vector.</summary>
        [FieldOffset(4)] public float Y;

        /// <summary>The Z component of the vector.</summary>
        [FieldOffset(8)] public float Z;

        /// <summary>The W component of the vector.</summary>
        [FieldOffset(12)] public float W;

        /// <summary>
        /// Creates a new <see cref="T:WCell.Util.Graphics.Vector3" /> with the given X, Y, Z and W components.
        /// </summary>
        /// <param name="x">the X component</param>
        /// <param name="y">the Y component</param>
        /// <param name="z">the Z component</param>
        /// <param name="w">the W component</param>
        public Vector4(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        /// <summary>
        /// Creates a new <see cref="T:WCell.Util.Graphics.Vector3" /> with the given X, Y, Z and W components.
        /// </summary>
        /// <param name="xy">the XY component</param>
        /// <param name="z">the Z component</param>
        /// <param name="w">the W component</param>
        public Vector4(Vector2 xy, float z, float w)
        {
            this.X = xy.X;
            this.Y = xy.Y;
            this.Z = z;
            this.W = w;
        }

        /// <summary>
        /// Creates a new <see cref="T:WCell.Util.Graphics.Vector3" /> with the given X, Y, Z and W components.
        /// </summary>
        /// <param name="xyz">the XYZ component</param>
        /// <param name="w">the W component</param>
        public Vector4(Vector3 xyz, float w)
        {
            this.X = xyz.X;
            this.Y = xyz.Y;
            this.Z = xyz.Z;
            this.W = w;
        }

        /// <summary>
        /// Clamps the values of the vector to be within a specified range.
        /// </summary>
        /// <param name="min">the minimum value</param>
        /// <param name="max">the maximum value</param>
        /// <returns>a new <see cref="T:WCell.Util.Graphics.Vector4" /> that has been clamped within the specified range</returns>
        public Vector4 Clamp(ref Vector4 min, ref Vector4 max)
        {
            float x1 = this.X;
            float num1 = (double) x1 > (double) max.X ? max.X : x1;
            float x2 = (double) num1 < (double) min.X ? min.X : num1;
            float y1 = this.Y;
            float num2 = (double) y1 > (double) max.Y ? max.Y : y1;
            float y2 = (double) num2 < (double) min.Y ? min.Y : num2;
            float z1 = this.Z;
            float num3 = (double) z1 > (double) max.Z ? max.Z : z1;
            float z2 = (double) num3 < (double) min.Z ? min.Z : num3;
            float w1 = this.W;
            float num4 = (double) w1 > (double) max.W ? max.W : w1;
            float w2 = (double) num4 < (double) min.W ? min.W : num4;
            return new Vector4(x2, y2, z2, w2);
        }

        /// <summary>Calculates the distance from this vector to another.</summary>
        /// <param name="point">the second <see cref="T:WCell.Util.Graphics.Vector4" /></param>
        /// <returns>the distance between the vectors</returns>
        public float GetDistance(ref Vector4 point)
        {
            float num1 = point.X - this.X;
            float num2 = point.Y - this.Y;
            float num3 = point.Z - this.Z;
            float num4 = point.W - this.W;
            return (float) Math.Sqrt((double) num1 * (double) num1 + (double) num2 * (double) num2 +
                                     (double) num3 * (double) num3 + (double) num4 * (double) num4);
        }

        /// <summary>
        /// Calculates the distance squared from this vector to another.
        /// </summary>
        /// <param name="point">the second <see cref="T:WCell.Util.Graphics.Vector4" /></param>
        /// <returns>the distance squared between the vectors</returns>
        public float GetDistanceSquared(ref Vector4 point)
        {
            float num1 = point.X - this.X;
            float num2 = point.Y - this.Y;
            float num3 = point.Z - this.Z;
            float num4 = point.W - this.W;
            return (float) ((double) num1 * (double) num1 + (double) num2 * (double) num2 +
                            (double) num3 * (double) num3 + (double) num4 * (double) num4);
        }

        /// <summary>Turns the current vector into a unit vector.</summary>
        /// <remarks>The vector becomes one unit in length and points in the same direction of the original vector.</remarks>
        public void Normalize()
        {
            float num = 1f / (float) Math.Sqrt((double) this.X * (double) this.X + (double) this.Y * (double) this.Y +
                                               (double) this.Z * (double) this.Z + (double) this.W * (double) this.W);
            this.X *= num;
            this.Y *= num;
            this.Z *= num;
            this.W *= num;
        }

        /// <summary>Checks equality of two vectors.</summary>
        /// <param name="other">the other vector to compare with</param>
        /// <returns>true if both vectors are equal; false otherwise</returns>
        public bool Equals(Vector4 other)
        {
            return (double) this.X == (double) other.X && (double) this.Y == (double) other.Y &&
                   (double) this.Z == (double) other.Z && (double) this.W == (double) other.W;
        }

        /// <summary>Checks equality with another object.</summary>
        /// <param name="obj">the object to compare</param>
        /// <returns>true if the object is <see cref="T:WCell.Util.Graphics.Vector4" /> and is equal; false otherwise</returns>
        public override bool Equals(object obj)
        {
            return obj is Vector4 && this.Equals((Vector4) obj);
        }

        public override int GetHashCode()
        {
            return this.X.GetHashCode() + this.Y.GetHashCode() + this.Y.GetHashCode() + this.W.GetHashCode();
        }

        public static bool operator ==(Vector4 a, Vector4 b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Vector4 a, Vector4 b)
        {
            return (double) a.X != (double) b.X || (double) a.Y != (double) b.Y || (double) a.Z != (double) b.Z ||
                   (double) a.W != (double) b.W;
        }

        public override string ToString()
        {
            return string.Format("(X:{0}, Y:{1}, Z:{2}, W:{3})", (object) this.X, (object) this.Y, (object) this.Z,
                (object) this.W);
        }
    }
}
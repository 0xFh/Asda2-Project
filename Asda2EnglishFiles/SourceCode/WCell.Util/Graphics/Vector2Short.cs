using System;

namespace WCell.Util.Graphics
{
    public struct Vector2Short
    {
        public static readonly Vector2Short Zero = new Vector2Short((short) 0, (short) 0);

        /// <summary>The X component of the vector.</summary>
        public short X;

        /// <summary>The Y component of the vector.</summary>
        public short Y;

        /// <summary>
        /// Creates a new <see cref="T:WCell.Util.Graphics.Vector2" /> with the given X and Y components.
        /// </summary>
        /// <param name="x">the X component</param>
        /// <param name="y">the Y component</param>
        public Vector2Short(short x, short y)
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
            float x1 = (float) this.X;
            float num1 = (double) x1 > (double) max.X ? max.X : x1;
            float x2 = (double) num1 < (double) min.X ? min.X : num1;
            float y1 = (float) this.Y;
            float num2 = (double) y1 > (double) max.Y ? max.Y : y1;
            float y2 = (double) num2 < (double) min.Y ? min.Y : num2;
            return new Vector2(x2, y2);
        }

        /// <summary>Calculates the distance from this vector to another.</summary>
        /// <param name="point">the second <see cref="T:WCell.Util.Graphics.Vector2" /></param>
        /// <returns>the distance between the vectors</returns>
        public float GetDistance(ref Vector2 point)
        {
            float num1 = point.X - (float) this.X;
            float num2 = point.Y - (float) this.Y;
            return (float) Math.Sqrt((double) num1 * (double) num1 + (double) num2 * (double) num2);
        }

        /// <summary>
        /// Calculates the distance squared from this vector to another.
        /// </summary>
        /// <param name="point">the second <see cref="T:WCell.Util.Graphics.Vector2" /></param>
        /// <returns>the distance squared between the vectors</returns>
        public float GetDistanceSquared(ref Vector2 point)
        {
            float num1 = point.X - (float) this.X;
            float num2 = point.Y - (float) this.Y;
            return (float) ((double) num1 * (double) num1 + (double) num2 * (double) num2);
        }

        public float Length()
        {
            return (float) Math.Sqrt((double) ((int) this.X * (int) this.X + (int) this.Y * (int) this.Y));
        }

        public float LengthSquared()
        {
            return (float) ((int) this.X * (int) this.X + (int) this.Y * (int) this.Y);
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

        public static bool operator ==(Vector2Short a, Vector2Short b)
        {
            return a.Equals((object) b);
        }

        public static bool operator !=(Vector2Short a, Vector2Short b)
        {
            return (int) a.X != (int) b.X || (int) a.Y != (int) b.Y;
        }

        public static Vector2Short operator +(Vector2Short a, Vector2Short b)
        {
            return new Vector2Short((short) ((int) a.X + (int) b.X), (short) ((int) a.Y + (int) b.Y));
        }

        public static Vector2Short operator -(Vector2Short a, Vector2 b)
        {
            return new Vector2Short((short) ((double) a.X - (double) b.X), (short) ((double) a.Y - (double) b.Y));
        }

        public override string ToString()
        {
            return string.Format("(X:{0}, Y:{1})", (object) this.X, (object) this.Y);
        }
    }
}
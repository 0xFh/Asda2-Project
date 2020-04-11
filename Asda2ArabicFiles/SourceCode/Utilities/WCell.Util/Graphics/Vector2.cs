using System;
using System.Runtime.InteropServices;

namespace WCell.Util.Graphics
{
	/// <summary>
	/// Defines a vector with two components.
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Size = 8)]
	public struct Vector2 : IEquatable<Vector2>
	{
		public static readonly Vector2 Zero = new Vector2(0, 0);

		/// <summary>
		/// The X component of the vector.
		/// </summary>
		[FieldOffset(0)]
		public float X;

		/// <summary>
		/// The Y component of the vector.
		/// </summary>
		[FieldOffset(4)]
		public float Y;

		/// <summary>
		/// Creates a new <see cref="Vector2" /> with the given X and Y components.
		/// </summary>
		/// <param name="x">the X component</param>
		/// <param name="y">the Y component</param>
		public Vector2(float x, float y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// Clamps the values of the vector to be within a specified range.
		/// </summary>
		/// <param name="min">the minimum value</param>
		/// <param name="max">the maximum value</param>
		/// <returns>a new <see cref="Vector2" /> that has been clamped within the specified range</returns>
		public Vector2 Clamp(ref Vector2 min, ref Vector2 max)
		{
			float x = X;
			x = (x > max.X) ? max.X : x;
			x = (x < min.X) ? min.X : x;
			float y = Y;
			y = (y > max.Y) ? max.Y : y;
			y = (y < min.Y) ? min.Y : y;

			return new Vector2(x, y);
		}

		/// <summary>
		/// Calculates the distance from this vector to another.
		/// </summary>
		/// <param name="point">the second <see cref="Vector2" /></param>
		/// <returns>the distance between the vectors</returns>
		public float GetDistance(ref Vector2 point)
		{
			float x = point.X - X;
			float y = point.Y - Y;
			float dist = (x * x) + (y * y);

			return (float)Math.Sqrt(dist);
		}

		/// <summary>
		/// Calculates the distance squared from this vector to another.
		/// </summary>
		/// <param name="point">the second <see cref="Vector2" /></param>
		/// <returns>the distance squared between the vectors</returns>
		public float GetDistanceSquared(ref Vector2 point)
		{
			float x = point.X - X;
			float y = point.Y - Y;

			return (x * x) + (y * y);
		}

		public float Length()
		{
			return (float)Math.Sqrt(X*X + Y*Y);
		}

		public float LengthSquared()
		{
			return (X*X + Y*Y);
		}

		/// <summary>
		/// Subtracts vector b from vector a.
		/// </summary>
		/// <param name="a">The vector to subtract from.</param>
		/// <param name="b">The subtracting vector.</param>
		/// <param name="result">A Vector2 filled with the result of (a - b).</param>
		public static void Subtract(ref Vector2 a, ref Vector2 b, out Vector2 result)
		{
			result.X = a.X - b.X;
			result.Y = a.Y - b.Y;
		}

		/// <summary>
		/// Turns the current vector into a unit vector.
		/// </summary>
		/// <remarks>The vector becomes one unit in length and points in the same direction of the original vector.</remarks>
		public void Normalize()
		{
			float length = ((X * X) + (Y * Y));
			float normFactor = 1f / ((float)Math.Sqrt(length));

			X *= normFactor;
			Y *= normFactor;
		}

        public Vector2 NormalizedCopy()
        {
            var length = ((X*X) + (Y*Y));
            var normFactor = 1f/((float) Math.Sqrt(length));

            return new Vector2(X*normFactor, Y*normFactor);
        }

        public static float Dot(Vector2 value1, Vector2 value2)
        {
            return ((value1.X * value2.X) + (value1.Y * value2.Y));
        }

        public static void Dot(ref Vector2 value1, ref Vector2 value2, out float result)
        {
            result = (value1.X * value2.X) + (value1.Y * value2.Y);
        }

		public Vector2 RightNormal()
		{
			return new Vector2(Y, -X);
		}

        public bool LerpZ(Vector3 start, Vector3 end, out float newZ)
        {
            newZ = float.MinValue;
            var denom = (end.X - start.X);
            // start and end are the same point, linear interpolation is impossible
            if (denom.IsWithinEpsilon(0f)) return false;

            var ua = ((X - start.X) / (end.X - start.X));
            newZ = start.Z + ua * (end.Z - start.Z);

            return true;
        }

	    /// <summary>
		/// Checks equality of two vectors.
		/// </summary>
		/// <param name="other">the other vector to compare with</param>
		/// <returns>true if both vectors are equal; false otherwise</returns>
		public bool Equals(Vector2 other)
		{
			return ((X == other.X) && (Y == other.Y));
		}

		/// <summary>
		/// Checks equality with another object.
		/// </summary>
		/// <param name="obj">the object to compare</param>
		/// <returns>true if the object is <see cref="Vector2" /> and is equal; false otherwise</returns>
		public override bool Equals(object obj)
		{
			return obj is Vector2 && Equals((Vector2) obj);
		}

		public override int GetHashCode()
		{
			return (X.GetHashCode() + Y.GetHashCode());
		}

		public static bool operator ==(Vector2 a, Vector2 b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Vector2 a, Vector2 b)
		{
			return (a.X != b.X) || a.Y != b.Y;
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
            return new Vector2(f*a.X, f*a.Y);
        }

        public static Vector2 operator *(Vector2 a, float f)
        {
            return new Vector2(f*a.X, f*a.Y);
		}

		public static Vector2 operator /(Vector2 a, Vector2 b)
		{
			Vector2 vector;
			vector.X = a.X / b.X;
			vector.Y = a.Y / b.Y;

			return vector;
		}

		public static Vector2 operator /(Vector2 a, float scaleFactor)
		{
			Vector2 vector;
			float factor = 1f / scaleFactor;
			vector.X = a.X * factor;
			vector.Y = a.Y * factor;

			return vector;
		}

		public override string ToString()
		{
			return string.Format("(X:{0}, Y:{1})", X, Y);
		}
	}
    public struct Vector2Short
    {
        public static readonly Vector2Short Zero = new Vector2Short(0, 0);

		/// <summary>
		/// The X component of the vector.
		/// </summary>
		public short X;

		/// <summary>
		/// The Y component of the vector.
		/// </summary>
		public short Y;

		/// <summary>
		/// Creates a new <see cref="Vector2" /> with the given X and Y components.
		/// </summary>
		/// <param name="x">the X component</param>
		/// <param name="y">the Y component</param>
        public Vector2Short(short x, short y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// Clamps the values of the vector to be within a specified range.
		/// </summary>
		/// <param name="min">the minimum value</param>
		/// <param name="max">the maximum value</param>
		/// <returns>a new <see cref="Vector2" /> that has been clamped within the specified range</returns>
		public Vector2 Clamp(ref Vector2 min, ref Vector2 max)
		{
			float x = X;
			x = (x > max.X) ? max.X : x;
			x = (x < min.X) ? min.X : x;
			float y = Y;
			y = (y > max.Y) ? max.Y : y;
			y = (y < min.Y) ? min.Y : y;

			return new Vector2(x, y);
		}

		/// <summary>
		/// Calculates the distance from this vector to another.
		/// </summary>
		/// <param name="point">the second <see cref="Vector2" /></param>
		/// <returns>the distance between the vectors</returns>
		public float GetDistance(ref Vector2 point)
		{
			float x = point.X - X;
			float y = point.Y - Y;
			float dist = (x * x) + (y * y);

			return (float)Math.Sqrt(dist);
		}

		/// <summary>
		/// Calculates the distance squared from this vector to another.
		/// </summary>
		/// <param name="point">the second <see cref="Vector2" /></param>
		/// <returns>the distance squared between the vectors</returns>
		public float GetDistanceSquared(ref Vector2 point)
		{
			float x = point.X - X;
			float y = point.Y - Y;

			return (x * x) + (y * y);
		}

		public float Length()
		{
			return (float)Math.Sqrt(X*X + Y*Y);
		}

		public float LengthSquared()
		{
			return (X*X + Y*Y);
		}

		/// <summary>
		/// Subtracts vector b from vector a.
		/// </summary>
		/// <param name="a">The vector to subtract from.</param>
		/// <param name="b">The subtracting vector.</param>
		/// <param name="result">A Vector2 filled with the result of (a - b).</param>
		public static void Subtract(ref Vector2 a, ref Vector2 b, out Vector2 result)
		{
			result.X = a.X - b.X;
			result.Y = a.Y - b.Y;
		}
        
	    /// <summary>
		/// Checks equality of two vectors.
		/// </summary>
		/// <param name="other">the other vector to compare with</param>
		/// <returns>true if both vectors are equal; false otherwise</returns>
		public bool Equals(Vector2 other)
		{
			return ((X == other.X) && (Y == other.Y));
		}

		/// <summary>
		/// Checks equality with another object.
		/// </summary>
		/// <param name="obj">the object to compare</param>
		/// <returns>true if the object is <see cref="Vector2" /> and is equal; false otherwise</returns>
		public override bool Equals(object obj)
		{
			return obj is Vector2 && Equals((Vector2) obj);
		}

		public override int GetHashCode()
		{
			return (X.GetHashCode() + Y.GetHashCode());
		}

        public static bool operator ==(Vector2Short a, Vector2Short b)
		{
			return a.Equals(b);
		}

        public static bool operator !=(Vector2Short a, Vector2Short b)
		{
			return (a.X != b.X) || a.Y != b.Y;
		}

        public static Vector2Short operator +(Vector2Short a, Vector2Short b)
		{
            return new Vector2Short((short) (a.X + b.X), (short) (a.Y + b.Y));
		}

        public static Vector2Short operator -(Vector2Short a, Vector2 b)
		{
            return new Vector2Short((short) (a.X - b.X), (short) (a.Y - b.Y));
		}
        

		public override string ToString()
		{
			return string.Format("(X:{0}, Y:{1})", X, Y);
		}
    }
}
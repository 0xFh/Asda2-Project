using System;
using System.Runtime.InteropServices;

namespace WCell.Util.Graphics
{
  /// <summary>Defines a vector with three components.</summary>
  [Serializable]
  [StructLayout(LayoutKind.Explicit, Size = 12)]
  public struct Vector3 : IEquatable<Vector3>, IComparable<Vector3>
  {
    public static readonly Vector3 Zero = new Vector3(0.0f, 0.0f, 0.0f);
    public static readonly Vector3 One = new Vector3(1f, 1f, 1f);
    public static readonly Vector3 Up = new Vector3(0.0f, 0.0f, 1f);
    public static readonly Vector3 Down = new Vector3(0.0f, 0.0f, -1f);
    public static readonly Vector3 Left = new Vector3(-1f, 0.0f, 0.0f);
    public static readonly Vector3 Right = new Vector3(1f, 0.0f, 0.0f);
    public static readonly Vector3 Backward = new Vector3(0.0f, 0.0f, 1f);
    public static readonly Vector3 Forward = new Vector3(0.0f, 0.0f, -1f);

    /// <summary>The X component of the vector.</summary>
    [FieldOffset(0)]public float X;

    /// <summary>The Y component of the vector.</summary>
    [FieldOffset(4)]public float Y;

    /// <summary>The Z component of the vector.</summary>
    [FieldOffset(8)]public float Z;

    public Vector3(float val)
    {
      this = new Vector3(val, val, val);
    }

    /// <summary>
    /// Creates a new <see cref="T:WCell.Util.Graphics.Vector3" /> with the given X and Y, and Z = 0.
    /// </summary>
    /// <param name="x">the X component</param>
    /// <param name="y">the Y component</param>
    public Vector3(float x, float y)
    {
      this = new Vector3(x, y, 0.0f);
    }

    /// <summary>
    /// Creates a new <see cref="T:WCell.Util.Graphics.Vector3" /> with the given X, Y and Z components.
    /// </summary>
    /// <param name="x">the X component</param>
    /// <param name="y">the Y component</param>
    /// <param name="z">the Z component</param>
    public Vector3(float x, float y, float z)
    {
      X = x;
      Y = y;
      Z = z;
    }

    /// <summary>
    /// Creates a new <see cref="T:WCell.Util.Graphics.Vector3" /> with the given X, Y, and Z components.
    /// </summary>
    /// <param name="xy">the XY component</param>
    /// <param name="z">the Z component</param>
    public Vector3(Vector2 xy, float z)
    {
      X = xy.X;
      Y = xy.Y;
      Z = z;
    }

    public bool IsSet
    {
      get { return this != Zero; }
    }

    public Vector2 XY
    {
      get { return new Vector2(X, Y); }
    }

    /// <summary>Calculates the distance from this vector to another.</summary>
    /// <param name="point">the second <see cref="T:WCell.Util.Graphics.Vector3" /></param>
    /// <returns>the distance between the vectors</returns>
    public float GetDistance(Vector3 point)
    {
      float num1 = point.X - X;
      float num2 = point.Y - Y;
      return (float) Math.Sqrt(num1 * (double) num1 + num2 * (double) num2);
    }

    /// <summary>Calculates the distance from this vector to another.</summary>
    /// <param name="point">the second <see cref="T:WCell.Util.Graphics.Vector3" /></param>
    /// <returns>the distance between the vectors</returns>
    public float GetDistance(ref Vector3 point)
    {
      float num1 = point.X - X;
      float num2 = point.Y - Y;
      return (float) Math.Sqrt(num1 * (double) num1 + num2 * (double) num2);
    }

    public static float Distance(Vector3 value1, Vector3 value2)
    {
      float num1 = value1.X - value2.X;
      float num2 = value1.Y - value2.Y;
      return (float) Math.Sqrt(num1 * (double) num1 + num2 * (double) num2);
    }

    /// <summary>
    /// Calculates the distance squared from this vector to another.
    /// </summary>
    /// <param name="point">the second <see cref="T:WCell.Util.Graphics.Vector3" /></param>
    /// <returns>the distance squared between the vectors</returns>
    public float DistanceSquared(Vector3 point)
    {
      float num1 = point.X - X;
      float num2 = point.Y - Y;
      return (float) (num1 * (double) num1 + num2 * (double) num2);
    }

    /// <summary>
    /// Calculates the distance squared from this vector to another.
    /// </summary>
    /// <param name="point">the second <see cref="T:WCell.Util.Graphics.Vector3" /></param>
    /// <returns>the distance squared between the vectors</returns>
    public float DistanceSquared(ref Vector3 point)
    {
      float num1 = point.X - X;
      float num2 = point.Y - Y;
      return (float) (num1 * (double) num1 + num2 * (double) num2);
    }

    public static float DistanceSquared(Vector3 value1, Vector3 value2)
    {
      float num1 = value1.X - value2.X;
      float num2 = value1.Y - value2.Y;
      return (float) (num1 * (double) num1 + num2 * (double) num2);
    }

    public static void DistanceSquared(ref Vector3 value1, ref Vector3 value2, out float result)
    {
      float num1 = value1.X - value2.X;
      float num2 = value1.Y - value2.Y;
      result = (float) (num1 * (double) num1 + num2 * (double) num2);
    }

    public float LengthSquared()
    {
      return (float) (X * (double) X + Y * (double) Y +
                      Z * (double) Z);
    }

    public float Length()
    {
      return (float) Math.Sqrt(X * (double) X + Y * (double) Y +
                               Z * (double) Z);
    }

    /// <summary>
    /// Gets the angle between this object and the given position, in relation to the north-south axis
    /// </summary>
    public float GetAngleTowards(Vector3 v)
    {
      float num = (float) Math.Atan2(v.Y - (double) Y, v.X - (double) X);
      if(num < 0.0)
        num += 6.283185f;
      return num;
    }

    /// <summary>
    /// Gets the angle between this object and the given position, in relation to the north-south axis
    /// </summary>
    public float GetAngleTowards(ref Vector3 v)
    {
      float num = (float) Math.Atan2(v.Y - (double) Y, v.X - (double) X);
      if(num < 0.0)
        num += 6.283185f;
      return num;
    }

    /// <summary>
    /// Gets the Point that lies in the given angle and has dist from this Point (in the XY plane).
    /// </summary>
    public void GetPointXY(float angle, float dist, out Vector3 point)
    {
      point = new Vector3(X - dist * (float) Math.Sin(angle),
        Y + dist * (float) Math.Cos(angle));
    }

    /// <summary>
    /// Gets the Point that lies in the given angle and has dist from this Point
    /// (in the weird WoW coordinate system: X -&gt; South, Y -&gt; West).
    /// </summary>
    public void GetPointYX(float angle, float dist, out Vector3 point)
    {
      point = new Vector3(X + dist * (float) Math.Cos(angle),
        Y + dist * (float) Math.Sin(angle));
    }

    /// <summary>Turns the current vector into a unit vector.</summary>
    /// <remarks>The vector becomes one unit in length and points in the same direction of the original vector.</remarks>
    public void Normalize()
    {
      float num = 1f / (float) Math.Sqrt(X * (double) X + Y * (double) Y);
      X *= num;
      Y *= num;
    }

    /// <summary>Turns the current vector into a unit vector.</summary>
    /// <remarks>The vector becomes one unit in length and points in the same direction of the original vector.</remarks>
    /// <returns>The length of the original vector.</returns>
    public float NormalizeReturnLength()
    {
      float num1 = (float) Math.Sqrt(X * (double) X + Y * (double) Y);
      float num2 = 1f / num1;
      X *= num2;
      Y *= num2;
      return num1;
    }

    /// <summary>Turns the current vector into a unit vector.</summary>
    /// <remarks>The vector becomes one unit in length and points in the same direction of the original vector.</remarks>
    public Vector3 NormalizedCopy()
    {
      float num = 1f / (float) Math.Sqrt(X * (double) X + Y * (double) Y);
      return new Vector3(X * num, Y * num, 0.0f);
    }

    public static Vector3 FromPacked(uint packed)
    {
      Vector3 vector3;
      vector3.X = packed & 2047U;
      vector3.Y = packed >> 11 & 2047U;
      vector3.Z = packed >> 21 & 1023U;
      return vector3;
    }

    public static Vector3 FromDeltaPacked(uint packed, Vector3 startingVector, Vector3 firstPoint)
    {
      float num1 = packed & 2047U;
      float num2 = packed >> 11 & 2047U;
      float num3 = packed >> 21 & 1023U;
      Vector3 vector3;
      vector3.X = (float) (num1 / 4.0 - (startingVector.X + (double) firstPoint.X) / 2.0);
      vector3.Y = (float) (num2 / 4.0 - (startingVector.Y + (double) firstPoint.Y) / 2.0);
      vector3.Z = (float) (num3 / 4.0 - (startingVector.Z + (double) firstPoint.Z) / 2.0);
      return vector3;
    }

    public uint ToPacked()
    {
      return (uint) (0 | (int) (uint) X & 2047) | (uint) (((int) (uint) Y & 2047) << 11) |
             (uint) (((int) (uint) Z & 1023) << 22);
    }

    /// <summary>TODO: Ensure this is the correct order of packing</summary>
    /// <param name="startingVector"></param>
    /// <param name="firstPoint"></param>
    /// <returns></returns>
    public uint ToDeltaPacked(Vector3 startingVector, Vector3 firstPoint)
    {
      return (uint) (0 | (int) (uint) (float) (X * 4.0 +
                                               (startingVector.X - (double) firstPoint.X) * 2.0) &
                     2047) |
             (uint) (((int) (uint) (float) (Y * 4.0 +
                                            (startingVector.Y - (double) firstPoint.Y) * 2.0) & 2047) <<
                     11) | (uint) (((int) (uint) (float) (Z * 4.0 +
                                                          (startingVector.Z - (double) firstPoint.Z) *
                                                          2.0) & 1023) << 22);
    }

    public static Vector3 operator -(Vector3 a, Vector3 b)
    {
      Vector3 vector3;
      vector3.X = a.X - b.X;
      vector3.Y = a.Y - b.Y;
      vector3.Z = a.Z - b.Z;
      return vector3;
    }

    public static Vector3 operator -(Vector3 value)
    {
      Vector3 vector3;
      vector3.X = -value.X;
      vector3.Y = -value.Y;
      vector3.Z = -value.Z;
      return vector3;
    }

    public static Vector3 operator -(Vector3 a, float scalar)
    {
      a.X -= scalar;
      a.Y -= scalar;
      a.Z -= scalar;
      return a;
    }

    public static Vector3 operator +(Vector3 a, Vector3 b)
    {
      Vector3 vector3;
      vector3.X = a.X + b.X;
      vector3.Y = a.Y + b.Y;
      vector3.Z = a.Z + b.Z;
      return vector3;
    }

    public static Vector3 operator +(Vector3 a, float scalar)
    {
      a.X += scalar;
      a.Y += scalar;
      a.Z += scalar;
      return a;
    }

    public static Vector3 operator *(Vector3 a, Vector3 b)
    {
      Vector3 vector3;
      vector3.X = a.X * b.X;
      vector3.Y = a.Y * b.Y;
      vector3.Z = a.Z * b.Z;
      return vector3;
    }

    public static Vector3 operator *(Vector3 a, float scaleFactor)
    {
      Vector3 vector3;
      vector3.X = a.X * scaleFactor;
      vector3.Y = a.Y * scaleFactor;
      vector3.Z = a.Z * scaleFactor;
      return vector3;
    }

    public static Vector3 operator *(float scaleFactor, Vector3 a)
    {
      Vector3 vector3;
      vector3.X = a.X * scaleFactor;
      vector3.Y = a.Y * scaleFactor;
      vector3.Z = a.Z * scaleFactor;
      return vector3;
    }

    public static Vector3 operator /(Vector3 a, Vector3 b)
    {
      Vector3 vector3;
      vector3.X = a.X / b.X;
      vector3.Y = a.Y / b.Y;
      vector3.Z = a.Z / b.Z;
      return vector3;
    }

    public static Vector3 operator /(Vector3 a, float scaleFactor)
    {
      float num = 1f / scaleFactor;
      Vector3 vector3;
      vector3.X = a.X * num;
      vector3.Y = a.Y * num;
      vector3.Z = a.Z * num;
      return vector3;
    }

    public static Vector3 Cross(Vector3 vector1, Vector3 vector2)
    {
      Vector3 vector3;
      vector3.X = (float) (vector1.Y * (double) vector2.Z - vector1.Z * (double) vector2.Y);
      vector3.Y = (float) (vector1.Z * (double) vector2.X - vector1.X * (double) vector2.Z);
      vector3.Z = (float) (vector1.X * (double) vector2.Y - vector1.Y * (double) vector2.X);
      return vector3;
    }

    public static Vector3 Normalize(Vector3 value)
    {
      float num = 1f / (float) Math.Sqrt(value.X * (double) value.X +
                                         value.Y * (double) value.Y +
                                         value.Z * (double) value.Z);
      Vector3 vector3;
      vector3.X = value.X * num;
      vector3.Y = value.Y * num;
      vector3.Z = value.Z * num;
      return vector3;
    }

    public static float Dot(Vector3 vector1, Vector3 vector2)
    {
      return (float) (vector1.X * (double) vector2.X + vector1.Y * (double) vector2.Y +
                      vector1.Z * (double) vector2.Z);
    }

    public static void Add(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
    {
      result.X = value1.X + value2.X;
      result.Y = value1.Y + value2.Y;
      result.Z = value1.Z + value2.Z;
    }

    public static void Subtract(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
    {
      result.X = value1.X - value2.X;
      result.Y = value1.Y - value2.Y;
      result.Z = value1.Z - value2.Z;
    }

    public static bool operator ==(Vector3 a, Vector3 b)
    {
      return a.Equals(b);
    }

    public static bool operator !=(Vector3 a, Vector3 b)
    {
      if(a.X == (double) b.X && a.Y == (double) b.Y)
        return a.Z != (double) b.Z;
      return true;
    }

    public static Vector3 TransformNormal(Vector3 normal, Matrix matrix)
    {
      float num1 = (float) (normal.X * (double) matrix.M11 + normal.Y * (double) matrix.M21 +
                            normal.Z * (double) matrix.M31);
      float num2 = (float) (normal.X * (double) matrix.M12 + normal.Y * (double) matrix.M22 +
                            normal.Z * (double) matrix.M32);
      float num3 = (float) (normal.X * (double) matrix.M13 + normal.Y * (double) matrix.M23 +
                            normal.Z * (double) matrix.M33);
      Vector3 vector3;
      vector3.X = num1;
      vector3.Y = num2;
      vector3.Z = num3;
      return vector3;
    }

    public static void TransformNormal(ref Vector3 normal, ref Matrix matrix, out Vector3 newVector)
    {
      float num1 = (float) (normal.X * (double) matrix.M11 + normal.Y * (double) matrix.M21 +
                            normal.Z * (double) matrix.M31);
      float num2 = (float) (normal.X * (double) matrix.M12 + normal.Y * (double) matrix.M22 +
                            normal.Z * (double) matrix.M32);
      float num3 = (float) (normal.X * (double) matrix.M13 + normal.Y * (double) matrix.M23 +
                            normal.Z * (double) matrix.M33);
      newVector.X = num1;
      newVector.Y = num2;
      newVector.Z = num3;
    }

    public static Vector3 Transform(Vector3 position, Matrix matrix)
    {
      float num1 = (float) (position.X * (double) matrix.M11 +
                            position.Y * (double) matrix.M21 +
                            position.Z * (double) matrix.M31) + matrix.M41;
      float num2 = (float) (position.X * (double) matrix.M12 +
                            position.Y * (double) matrix.M22 +
                            position.Z * (double) matrix.M32) + matrix.M42;
      float num3 = (float) (position.X * (double) matrix.M13 +
                            position.Y * (double) matrix.M23 +
                            position.Z * (double) matrix.M33) + matrix.M43;
      Vector3 vector3;
      vector3.X = num1;
      vector3.Y = num2;
      vector3.Z = num3;
      return vector3;
    }

    public static Vector3 Transform(Vector3 value, Quaternion rotation)
    {
      float num1 = rotation.X + rotation.X;
      float num2 = rotation.Y + rotation.Y;
      float num3 = rotation.Z + rotation.Z;
      float num4 = rotation.W * num1;
      float num5 = rotation.W * num2;
      float num6 = rotation.W * num3;
      float num7 = rotation.X * num1;
      float num8 = rotation.X * num2;
      float num9 = rotation.X * num3;
      float num10 = rotation.Y * num2;
      float num11 = rotation.Y * num3;
      float num12 = rotation.Z * num3;
      float num13 = (float) (value.X * (1.0 - num10 - num12) +
                             value.Y * (num8 - (double) num6) +
                             value.Z * (num9 + (double) num5));
      float num14 = (float) (value.X * (num8 + (double) num6) +
                             value.Y * (1.0 - num7 - num12) +
                             value.Z * (num11 - (double) num4));
      float num15 = (float) (value.X * (num9 - (double) num5) +
                             value.Y * (num11 + (double) num4) +
                             value.Z * (1.0 - num7 - num10));
      Vector3 vector3;
      vector3.X = num13;
      vector3.Y = num14;
      vector3.Z = num15;
      return vector3;
    }

    public static void Transform(ref Vector3 position, ref Matrix matrix, out Vector3 result)
    {
      float num1 = (float) (position.X * (double) matrix.M11 +
                            position.Y * (double) matrix.M21 +
                            position.Z * (double) matrix.M31) + matrix.M41;
      float num2 = (float) (position.X * (double) matrix.M12 +
                            position.Y * (double) matrix.M22 +
                            position.Z * (double) matrix.M32) + matrix.M42;
      float num3 = (float) (position.X * (double) matrix.M13 +
                            position.Y * (double) matrix.M23 +
                            position.Z * (double) matrix.M33) + matrix.M43;
      result.X = num1;
      result.Y = num2;
      result.Z = num3;
    }

    /// <summary>
    /// Clamps the values of the vector to be within a specified range.
    /// </summary>
    /// <param name="min">the minimum value</param>
    /// <param name="max">the maximum value</param>
    /// <returns>a new <see cref="T:WCell.Util.Graphics.Vector3" /> that has been clamped within the specified range</returns>
    public Vector3 Clamp(ref Vector3 min, ref Vector3 max)
    {
      float x1 = X;
      float num1 = (double) x1 > (double) max.X ? max.X : x1;
      float x2 = (double) num1 < (double) min.X ? min.X : num1;
      float y1 = Y;
      float num2 = (double) y1 > (double) max.Y ? max.Y : y1;
      float y2 = (double) num2 < (double) min.Y ? min.Y : num2;
      float z1 = Z;
      float num3 = (double) z1 > (double) max.Z ? max.Z : z1;
      float z2 = (double) num3 < (double) min.Z ? min.Z : num3;
      return new Vector3(x2, y2, z2);
    }

    public static void Clamp(ref Vector3 value1, ref Vector3 min, ref Vector3 max, out Vector3 result)
    {
      float x = value1.X;
      float num1 = (double) x > (double) max.X ? max.X : x;
      float num2 = (double) num1 < (double) min.X ? min.X : num1;
      float y = value1.Y;
      float num3 = (double) y > (double) max.Y ? max.Y : y;
      float num4 = (double) num3 < (double) min.Y ? min.Y : num3;
      float z = value1.Z;
      float num5 = (double) z > (double) max.Z ? max.Z : z;
      float num6 = (double) num5 < (double) min.Z ? min.Z : num5;
      result.X = num2;
      result.Y = num4;
      result.Z = num6;
    }

    public static Vector3 Min(Vector3 a, Vector3 b)
    {
      return new Vector3(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
    }

    public static void Min(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
    {
      result.X = (double) value1.X < (double) value2.X ? value1.X : value2.X;
      result.Y = (double) value1.Y < (double) value2.Y ? value1.Y : value2.Y;
      result.Z = (double) value1.Z < (double) value2.Z ? value1.Z : value2.Z;
    }

    public static Vector3 Max(Vector3 a, Vector3 b)
    {
      return new Vector3(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
    }

    public static void Max(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
    {
      result.X = (double) value1.X > (double) value2.X ? value1.X : value2.X;
      result.Y = (double) value1.Y > (double) value2.Y ? value1.Y : value2.Y;
      result.Z = (double) value1.Z > (double) value2.Z ? value1.Z : value2.Z;
    }

    /// <summary>Checks equality of two vectors.</summary>
    /// <param name="other">the other vector to compare with</param>
    /// <returns>true if both vectors are equal; false otherwise</returns>
    public bool Equals(Vector3 other)
    {
      return Math.Abs(X - other.X) < 1.0 / 1000.0 &&
             Math.Abs(Y - other.Y) < 1.0 / 1000.0 &&
             Math.Abs(Z - other.Z) < 1.0 / 1000.0;
    }

    /// <summary>Checks equality with another object.</summary>
    /// <param name="obj">the object to compare</param>
    /// <returns>true if the object is <see cref="T:WCell.Util.Graphics.Vector3" /> and is equal; false otherwise</returns>
    public override bool Equals(object obj)
    {
      return obj is Vector3 && Equals((Vector3) obj);
    }

    public override int GetHashCode()
    {
      return (int) (Length() * 1000.0);
    }

    public int CompareTo(Vector3 other)
    {
      if(Equals(other))
        return 0;
      return (double) (other.Length() - Length()) > 0.0 ? 1 : -1;
    }

    public override string ToString()
    {
      return string.Format("(X:{0}, Y:{1}, Z:{2})", X, Y, Z);
    }
  }
}
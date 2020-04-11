using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WCell.Util.Graphics
{
  /// <summary>Defines an axis-aligned bounding box.</summary>
  [Serializable]
  [StructLayout(LayoutKind.Explicit, Size = 24)]
  public struct BoundingBox : IEquatable<BoundingBox>
  {
    public static BoundingBox INVALID = new BoundingBox(new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
      new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));

    /// <summary>The lower-left bound of the box.</summary>
    [FieldOffset(0)]public Vector3 Min;

    /// <summary>The upper-right bound of the box.</summary>
    [FieldOffset(12)]public Vector3 Max;

    /// <summary>
    /// Creates a new <see cref="T:WCell.Util.Graphics.BoundingBox" /> with the given coordinates.
    /// </summary>
    /// <param name="minX">lower-bound X</param>
    /// <param name="minY">lower-bound Y</param>
    /// <param name="minZ">lower-bound Z</param>
    /// <param name="maxX">upper-bound X</param>
    /// <param name="maxY">upper-bound Y</param>
    /// <param name="maxZ">upper-bound Z</param>
    public BoundingBox(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
    {
      Min = new Vector3(minX, minY, minZ);
      Max = new Vector3(maxX, maxY, maxZ);
    }

    /// <summary>
    /// Creates a new <see cref="T:WCell.Util.Graphics.BoundingBox" /> with the given coordinates.
    /// </summary>
    /// <param name="minX">lower-bound X</param>
    /// <param name="minY">lower-bound Y</param>
    /// <param name="minZ">lower-bound Z</param>
    /// <param name="max">upper-bound vector</param>
    public BoundingBox(float minX, float minY, float minZ, Vector3 max)
    {
      Min = new Vector3(minX, minY, minZ);
      Max = max;
    }

    /// <summary>
    /// Creates a new <see cref="T:WCell.Util.Graphics.BoundingBox" /> with the given coordinates.
    /// </summary>
    /// <param name="min">lower-bound vector</param>
    /// <param name="maxX">upper-bound X</param>
    /// <param name="maxY">upper-bound Y</param>
    /// <param name="maxZ">upper-bound Z</param>
    public BoundingBox(Vector3 min, float maxX, float maxY, float maxZ)
    {
      Min = min;
      Max = new Vector3(maxX, maxY, maxZ);
    }

    /// <summary>
    /// Creates a new <see cref="T:WCell.Util.Graphics.BoundingBox" /> with the given coordinates.
    /// </summary>
    /// <param name="min">lower-bound vector</param>
    /// <param name="max">upper-bound vector</param>
    public BoundingBox(Vector3 min, Vector3 max)
    {
      Min = min;
      Max = max;
    }

    /// <summary>
    /// Creates a new <see cref="T:WCell.Util.Graphics.BoundingBox" /> containing the given vectors.
    /// </summary>
    /// <param name="vectors">The array of vectors to use.</param>
    public BoundingBox(Vector3[] vectors)
    {
      float num1 = float.MaxValue;
      float num2 = float.MaxValue;
      float num3 = float.MaxValue;
      float num4 = float.MinValue;
      float num5 = float.MinValue;
      float num6 = float.MinValue;
      for(int index = 0; index < vectors.Length; ++index)
      {
        Vector3 vector = vectors[index];
        num1 = Math.Min(vector.X, num1);
        num4 = Math.Max(vector.X, num4);
        num2 = Math.Min(vector.Y, num2);
        num5 = Math.Max(vector.Y, num5);
        num3 = Math.Min(vector.Z, num3);
        num6 = Math.Max(vector.Z, num6);
      }

      Min = new Vector3(num1, num2, num3);
      Max = new Vector3(num4, num5, num6);
    }

    public BoundingBox(IEnumerable<Vector3> vectors)
    {
      float num1 = float.MaxValue;
      float num2 = float.MaxValue;
      float num3 = float.MaxValue;
      float num4 = float.MinValue;
      float num5 = float.MinValue;
      float num6 = float.MinValue;
      foreach(Vector3 vector in vectors)
      {
        num1 = Math.Min(vector.X, num1);
        num4 = Math.Max(vector.X, num4);
        num2 = Math.Min(vector.Y, num2);
        num5 = Math.Max(vector.Y, num5);
        num3 = Math.Min(vector.Z, num3);
        num6 = Math.Max(vector.Z, num6);
      }

      Min = new Vector3(num1, num2, num3);
      Max = new Vector3(num4, num5, num6);
    }

    public float Width
    {
      get { return Max.X - Min.X; }
    }

    public float Height
    {
      get { return Max.Y - Min.Y; }
    }

    /// <summary>
    /// Checks whether the current <see cref="T:WCell.Util.Graphics.BoundingBox" /> intersects with the given <see cref="T:WCell.Util.Graphics.BoundingBox" />.
    /// </summary>
    /// <param name="box">the <see cref="T:WCell.Util.Graphics.BoundingBox" /> to check for intersection</param>
    /// <returns>an enumeration value describing the type of intersection between the two boxes</returns>
    public IntersectionType Intersects(ref BoundingBox box)
    {
      if(Max.X < (double) box.Min.X || Min.X > (double) box.Max.X ||
         (Max.Y < (double) box.Min.Y || Min.Y > (double) box.Max.Y) ||
         (Max.Z < (double) box.Min.Z || Min.Z > (double) box.Max.Z))
        return IntersectionType.NoIntersection;
      return (double) Min.X <= (double) box.Min.X && (double) box.Max.X <= (double) Max.X &&
             ((double) Min.Y <= (double) box.Min.Y && (double) box.Max.Y <= (double) Max.Y) &&
             ((double) Min.Z <= (double) box.Min.Z && (double) box.Max.Z <= (double) Max.Z)
        ? IntersectionType.Contained
        : IntersectionType.Intersects;
    }

    /// <summary>
    /// Checks whether the current <see cref="T:WCell.Util.Graphics.BoundingBox" /> intersects with the given <see cref="T:WCell.Util.Graphics.BoundingSphere" />.
    /// </summary>
    /// <param name="sphere">the <see cref="T:WCell.Util.Graphics.BoundingSphere" /> to check for intersection</param>
    /// <returns>an enumeration value describing the type of intersection between the box and sphere</returns>
    public IntersectionType Intersects(ref BoundingSphere sphere)
    {
      Vector3 point = sphere.Center.Clamp(ref Min, ref Max);
      float num = sphere.Center.DistanceSquared(ref point);
      float radius = sphere.Radius;
      if(num > radius * (double) radius)
        return IntersectionType.NoIntersection;
      return (double) Min.X + (double) radius <= (double) sphere.Center.X &&
             (double) sphere.Center.X <= (double) Max.X - (double) radius &&
             ((double) Max.X - (double) Min.X > (double) radius &&
              (double) Min.Y + (double) radius <= (double) sphere.Center.Y) &&
             ((double) sphere.Center.Y <= (double) Max.Y - (double) radius &&
              (double) Max.Y - (double) Min.Y > (double) radius &&
              ((double) Min.Z + (double) radius <= (double) sphere.Center.Z &&
               (double) sphere.Center.Z <= (double) Max.Z - (double) radius &&
               (double) Max.X - (double) Min.X > (double) radius))
        ? IntersectionType.Contained
        : IntersectionType.Intersects;
    }

    public bool Intersects(BoundingBox box)
    {
      if(Max.X < (double) box.Min.X || Min.X > (double) box.Max.X ||
         (Max.Y < (double) box.Min.Y || Min.Y > (double) box.Max.Y))
        return false;
      return Max.Z >= (double) box.Min.Z && Min.Z <= (double) box.Max.Z;
    }

    /// <summary>
    /// Checks whether the <see cref="T:WCell.Util.Graphics.BoundingBox" /> contains the given <see cref="T:WCell.Util.Graphics.BoundingBox" />.
    /// </summary>
    /// <param name="box">the <see cref="T:WCell.Util.Graphics.BoundingBox" /> to check for containment.</param>
    /// <returns>true if the <see cref="T:WCell.Util.Graphics.BoundingBox" /> is contained; false otherwise</returns>
    public bool Contains(ref BoundingBox box)
    {
      return box.Min.X > (double) Min.X && box.Min.Y > (double) Min.Y &&
             box.Min.Z > (double) Min.Z &&
             (box.Max.X < (double) Max.X && box.Max.Y < (double) Max.Y &&
              box.Max.Z < (double) Max.Z);
    }

    /// <summary>
    /// Checks whether the <see cref="T:WCell.Util.Graphics.BoundingBox" /> contains the given point.
    /// </summary>
    /// <param name="point">the point to check for containment.</param>
    /// <returns>true if the point is contained; false otherwise</returns>
    public bool Contains(ref Vector3 point)
    {
      return Min.X <= (double) point.X && point.X <= (double) Max.X &&
             (Min.Y <= (double) point.Y && point.Y <= (double) Max.Y) &&
             (Min.Z <= (double) point.Z && point.Z <= (double) Max.Z);
    }

    /// <summary>
    /// Checks whether the <see cref="T:WCell.Util.Graphics.BoundingBox" /> contains the given point.
    /// </summary>
    /// <param name="point">the point to check for containment.</param>
    /// <returns>true if the point is contained; false otherwise</returns>
    public bool Contains(ref Vector4 point)
    {
      return Min.X <= (double) point.X && point.X <= (double) Max.X &&
             (Min.Y <= (double) point.Y && point.Y <= (double) Max.Y) &&
             (Min.Z <= (double) point.Z && point.Z <= (double) Max.Z);
    }

    public ContainmentType Contains(BoundingSphere sphere)
    {
      Vector3 result1;
      Vector3.Clamp(ref sphere.Center, ref Min, ref Max, out result1);
      float result2;
      Vector3.DistanceSquared(ref sphere.Center, ref result1, out result2);
      float radius = sphere.Radius;
      if(result2 > radius * (double) radius)
        return ContainmentType.Disjoint;
      return (double) Min.X + (double) radius <= (double) sphere.Center.X &&
             (double) sphere.Center.X <= (double) Max.X - (double) radius &&
             ((double) Max.X - (double) Min.X > (double) radius &&
              (double) Min.Y + (double) radius <= (double) sphere.Center.Y) &&
             ((double) sphere.Center.Y <= (double) Max.Y - (double) radius &&
              (double) Max.Y - (double) Min.Y > (double) radius &&
              ((double) Min.Z + (double) radius <= (double) sphere.Center.Z &&
               (double) sphere.Center.Z <= (double) Max.Z - (double) radius &&
               (double) Max.X - (double) Min.X > (double) radius))
        ? ContainmentType.Contains
        : ContainmentType.Intersects;
    }

    /// <summary>Checks equality of two boxes.</summary>
    /// <param name="other">the other box to compare with</param>
    /// <returns>true if both boxes are equal; false otherwise</returns>
    public bool Equals(BoundingBox other)
    {
      return Min == other.Min && Max == other.Max;
    }

    /// <summary>Checks equality with another object.</summary>
    /// <param name="obj">the object to compare</param>
    /// <returns>true if the object is <see cref="T:WCell.Util.Graphics.BoundingBox" /> and is equal; false otherwise</returns>
    public override bool Equals(object obj)
    {
      return obj is BoundingBox && Equals((BoundingBox) obj);
    }

    public override int GetHashCode()
    {
      return Min.GetHashCode() + Max.GetHashCode();
    }

    public static bool operator ==(BoundingBox a, BoundingBox b)
    {
      return a.Equals(b);
    }

    public static bool operator !=(BoundingBox a, BoundingBox b)
    {
      if(!(a.Min != b.Min))
        return a.Max != b.Max;
      return true;
    }

    public override string ToString()
    {
      return string.Format("(Min: {0}, Max: {1})", Min, Max);
    }

    public float? Intersects(Ray ray)
    {
      float val2_1 = 0.0f;
      float val2_2 = float.MaxValue;
      if(Math.Abs(ray.Direction.X) < 9.99999997475243E-07)
      {
        if(ray.Position.X < (double) Min.X || ray.Position.X > (double) Max.X)
          return new float?();
      }
      else
      {
        float num1 = 1f / ray.Direction.X;
        float val1_1 = (Min.X - ray.Position.X) * num1;
        float val1_2 = (Max.X - ray.Position.X) * num1;
        if(val1_1 > (double) val1_2)
        {
          float num2 = val1_1;
          val1_1 = val1_2;
          val1_2 = num2;
        }

        val2_1 = Math.Max(val1_1, val2_1);
        val2_2 = Math.Min(val1_2, val2_2);
        if(val2_1 > (double) val2_2)
          return new float?();
      }

      if(Math.Abs(ray.Direction.Y) < 9.99999997475243E-07)
      {
        if(ray.Position.Y < (double) Min.Y || ray.Position.Y > (double) Max.Y)
          return new float?();
      }
      else
      {
        float num1 = 1f / ray.Direction.Y;
        float val1_1 = (Min.Y - ray.Position.Y) * num1;
        float val1_2 = (Max.Y - ray.Position.Y) * num1;
        if(val1_1 > (double) val1_2)
        {
          float num2 = val1_1;
          val1_1 = val1_2;
          val1_2 = num2;
        }

        val2_1 = Math.Max(val1_1, val2_1);
        val2_2 = Math.Min(val1_2, val2_2);
        if(val2_1 > (double) val2_2)
          return new float?();
      }

      if(Math.Abs(ray.Direction.Z) < 9.99999997475243E-07)
      {
        if(ray.Position.Z < (double) Min.Z || ray.Position.Z > (double) Max.Z)
          return new float?();
      }
      else
      {
        float num1 = 1f / ray.Direction.Z;
        float val1_1 = (Min.Z - ray.Position.Z) * num1;
        float val1_2 = (Max.Z - ray.Position.Z) * num1;
        if(val1_1 > (double) val1_2)
        {
          float num2 = val1_1;
          val1_1 = val1_2;
          val1_2 = num2;
        }

        val2_1 = Math.Max(val1_1, val2_1);
        float num3 = Math.Min(val1_2, val2_2);
        if(val2_1 > (double) num3)
          return new float?();
      }

      return val2_1;
    }

    public void Intersects(ref Ray ray, out float? result)
    {
      result = 0.0f;
      float val2_1 = 0.0f;
      float val2_2 = float.MaxValue;
      if(Math.Abs(ray.Direction.X) < 9.99999997475243E-07)
      {
        if(ray.Position.X < (double) Min.X || ray.Position.X > (double) Max.X)
          return;
      }
      else
      {
        float num1 = 1f / ray.Direction.X;
        float val1_1 = (Min.X - ray.Position.X) * num1;
        float val1_2 = (Max.X - ray.Position.X) * num1;
        if(val1_1 > (double) val1_2)
        {
          float num2 = val1_1;
          val1_1 = val1_2;
          val1_2 = num2;
        }

        val2_1 = Math.Max(val1_1, val2_1);
        val2_2 = Math.Min(val1_2, val2_2);
        if(val2_1 > (double) val2_2)
          return;
      }

      if(Math.Abs(ray.Direction.Y) < 9.99999997475243E-07)
      {
        if(ray.Position.Y < (double) Min.Y || ray.Position.Y > (double) Max.Y)
          return;
      }
      else
      {
        float num1 = 1f / ray.Direction.Y;
        float val1_1 = (Min.Y - ray.Position.Y) * num1;
        float val1_2 = (Max.Y - ray.Position.Y) * num1;
        if(val1_1 > (double) val1_2)
        {
          float num2 = val1_1;
          val1_1 = val1_2;
          val1_2 = num2;
        }

        val2_1 = Math.Max(val1_1, val2_1);
        val2_2 = Math.Min(val1_2, val2_2);
        if(val2_1 > (double) val2_2)
          return;
      }

      if(Math.Abs(ray.Direction.Z) < 9.99999997475243E-07)
      {
        if(ray.Position.Z < (double) Min.Z || ray.Position.Z > (double) Max.Z)
          return;
      }
      else
      {
        float num1 = 1f / ray.Direction.Z;
        float val1_1 = (Min.Z - ray.Position.Z) * num1;
        float val1_2 = (Max.Z - ray.Position.Z) * num1;
        if(val1_1 > (double) val1_2)
        {
          float num2 = val1_1;
          val1_1 = val1_2;
          val1_2 = num2;
        }

        val2_1 = Math.Max(val1_1, val2_1);
        float num3 = Math.Min(val1_2, val2_2);
        if(val2_1 > (double) num3)
          return;
      }

      result = val2_1;
    }

    public static BoundingBox Join(ref BoundingBox a, ref BoundingBox b)
    {
      return new BoundingBox(Vector3.Min(a.Min, b.Min), Vector3.Max(a.Max, b.Max));
    }
  }
}
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace WCell.Util.Graphics
{
  [StructLayout(LayoutKind.Sequential, Size = 16)]
  public struct Plane : IEquatable<Plane>
  {
    public Vector3 Normal;
    public float D;

    public Plane(float a, float b, float c, float d)
    {
      Normal.X = a;
      Normal.Y = b;
      Normal.Z = c;
      D = d;
    }

    public Plane(Vector3 normal, float d)
    {
      Normal = normal;
      D = d;
    }

    public Plane(Vector4 value)
    {
      Normal.X = value.X;
      Normal.Y = value.Y;
      Normal.Z = value.Z;
      D = value.W;
    }

    public Plane(Vector3 point1, Vector3 point2, Vector3 point3)
    {
      Normal = Vector3.Cross(point2 - point1, point3 - point1);
      Normal.Normalize();
      D = -Vector3.Dot(Normal, point1);
    }

    public bool Equals(Plane other)
    {
      return Normal.X == (double) other.Normal.X &&
             Normal.Y == (double) other.Normal.Y &&
             Normal.Z == (double) other.Normal.Z && D == (double) other.D;
    }

    public override bool Equals(object obj)
    {
      bool flag = false;
      if(obj is Plane)
        flag = Equals((Plane) obj);
      return flag;
    }

    public override int GetHashCode()
    {
      return Normal.GetHashCode() + D.GetHashCode();
    }

    public override string ToString()
    {
      CultureInfo currentCulture = CultureInfo.CurrentCulture;
      return string.Format(currentCulture, "{{Normal:{0} D:{1}}}", (object) Normal.ToString(),
        (object) D.ToString(currentCulture));
    }

    public void Normalize()
    {
      float num1 = (float) (Normal.X * (double) Normal.X +
                            Normal.Y * (double) Normal.Y +
                            Normal.Z * (double) Normal.Z);
      if(Math.Abs(num1 - 1f) < 1.19209303761636E-07)
        return;
      float num2 = 1f / (float) Math.Sqrt(num1);
      Normal.X *= num2;
      Normal.Y *= num2;
      Normal.Z *= num2;
      D *= num2;
    }

    public static Plane Normalize(Plane value)
    {
      float num1 = (float) (value.Normal.X * (double) value.Normal.X +
                            value.Normal.Y * (double) value.Normal.Y +
                            value.Normal.Z * (double) value.Normal.Z);
      if(Math.Abs(num1 - 1f) < 1.19209303761636E-07)
      {
        Plane plane;
        plane.Normal = value.Normal;
        plane.D = value.D;
        return plane;
      }

      float num2 = 1f / (float) Math.Sqrt(num1);
      Plane plane1;
      plane1.Normal.X = value.Normal.X * num2;
      plane1.Normal.Y = value.Normal.Y * num2;
      plane1.Normal.Z = value.Normal.Z * num2;
      plane1.D = value.D * num2;
      return plane1;
    }

    public static void Normalize(ref Plane value, out Plane result)
    {
      float num1 = (float) (value.Normal.X * (double) value.Normal.X +
                            value.Normal.Y * (double) value.Normal.Y +
                            value.Normal.Z * (double) value.Normal.Z);
      if(Math.Abs(num1 - 1f) < 1.19209303761636E-07)
      {
        result.Normal = value.Normal;
        result.D = value.D;
      }
      else
      {
        float num2 = 1f / (float) Math.Sqrt(num1);
        result.Normal.X = value.Normal.X * num2;
        result.Normal.Y = value.Normal.Y * num2;
        result.Normal.Z = value.Normal.Z * num2;
        result.D = value.D * num2;
      }
    }

    public static Plane Transform(Plane plane, Matrix matrix)
    {
      Matrix result;
      Matrix.Invert(ref matrix, out result);
      float x = plane.Normal.X;
      float y = plane.Normal.Y;
      float z = plane.Normal.Z;
      float d = plane.D;
      Plane plane1;
      plane1.Normal.X = (float) (x * (double) result.M11 + y * (double) result.M12 +
                                 z * (double) result.M13 + d * (double) result.M14);
      plane1.Normal.Y = (float) (x * (double) result.M21 + y * (double) result.M22 +
                                 z * (double) result.M23 + d * (double) result.M24);
      plane1.Normal.Z = (float) (x * (double) result.M31 + y * (double) result.M32 +
                                 z * (double) result.M33 + d * (double) result.M34);
      plane1.D = (float) (x * (double) result.M41 + y * (double) result.M42 +
                          z * (double) result.M43 + d * (double) result.M44);
      return plane1;
    }

    public static void Transform(ref Plane plane, ref Matrix matrix, out Plane result)
    {
      Matrix result1;
      Matrix.Invert(ref matrix, out result1);
      float x = plane.Normal.X;
      float y = plane.Normal.Y;
      float z = plane.Normal.Z;
      float d = plane.D;
      result.Normal.X = (float) (x * (double) result1.M11 + y * (double) result1.M12 +
                                 z * (double) result1.M13 + d * (double) result1.M14);
      result.Normal.Y = (float) (x * (double) result1.M21 + y * (double) result1.M22 +
                                 z * (double) result1.M23 + d * (double) result1.M24);
      result.Normal.Z = (float) (x * (double) result1.M31 + y * (double) result1.M32 +
                                 z * (double) result1.M33 + d * (double) result1.M34);
      result.D = (float) (x * (double) result1.M41 + y * (double) result1.M42 +
                          z * (double) result1.M43 + d * (double) result1.M44);
    }

    public static Plane Transform(Plane plane, Quaternion rotation)
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
      float num13 = 1f - num10 - num12;
      float num14 = num8 - num6;
      float num15 = num9 + num5;
      float num16 = num8 + num6;
      float num17 = 1f - num7 - num12;
      float num18 = num11 - num4;
      float num19 = num9 - num5;
      float num20 = num11 + num4;
      float num21 = 1f - num7 - num10;
      float x = plane.Normal.X;
      float y = plane.Normal.Y;
      float z = plane.Normal.Z;
      Plane plane1;
      plane1.Normal.X = (float) (x * (double) num13 + y * (double) num14 +
                                 z * (double) num15);
      plane1.Normal.Y = (float) (x * (double) num16 + y * (double) num17 +
                                 z * (double) num18);
      plane1.Normal.Z = (float) (x * (double) num19 + y * (double) num20 +
                                 z * (double) num21);
      plane1.D = plane.D;
      return plane1;
    }

    public static void Transform(ref Plane plane, ref Quaternion rotation, out Plane result)
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
      float num13 = 1f - num10 - num12;
      float num14 = num8 - num6;
      float num15 = num9 + num5;
      float num16 = num8 + num6;
      float num17 = 1f - num7 - num12;
      float num18 = num11 - num4;
      float num19 = num9 - num5;
      float num20 = num11 + num4;
      float num21 = 1f - num7 - num10;
      float x = plane.Normal.X;
      float y = plane.Normal.Y;
      float z = plane.Normal.Z;
      result.Normal.X = (float) (x * (double) num13 + y * (double) num14 +
                                 z * (double) num15);
      result.Normal.Y = (float) (x * (double) num16 + y * (double) num17 +
                                 z * (double) num18);
      result.Normal.Z = (float) (x * (double) num19 + y * (double) num20 +
                                 z * (double) num21);
      result.D = plane.D;
    }

    public float Dot(Vector4 value)
    {
      return (float) (Normal.X * (double) value.X + Normal.Y * (double) value.Y +
                      Normal.Z * (double) value.Z + D * (double) value.W);
    }

    public void Dot(ref Vector4 value, out float result)
    {
      result = (float) (Normal.X * (double) value.X + Normal.Y * (double) value.Y +
                        Normal.Z * (double) value.Z + D * (double) value.W);
    }

    public float DotCoordinate(Vector3 value)
    {
      return (float) (Normal.X * (double) value.X + Normal.Y * (double) value.Y +
                      Normal.Z * (double) value.Z) + D;
    }

    public void DotCoordinate(ref Vector3 value, out float result)
    {
      result = (float) (Normal.X * (double) value.X + Normal.Y * (double) value.Y +
                        Normal.Z * (double) value.Z) + D;
    }

    public float DotNormal(Vector3 value)
    {
      return (float) (Normal.X * (double) value.X + Normal.Y * (double) value.Y +
                      Normal.Z * (double) value.Z);
    }

    public void DotNormal(ref Vector3 value, out float result)
    {
      result = (float) (Normal.X * (double) value.X + Normal.Y * (double) value.Y +
                        Normal.Z * (double) value.Z);
    }

    public PlaneIntersectionType Intersects(BoundingBox box)
    {
      Vector3 vector3_1;
      vector3_1.X = (double) Normal.X >= 0.0 ? box.Min.X : box.Max.X;
      vector3_1.Y = (double) Normal.Y >= 0.0 ? box.Min.Y : box.Max.Y;
      vector3_1.Z = (double) Normal.Z >= 0.0 ? box.Min.Z : box.Max.Z;
      Vector3 vector3_2;
      vector3_2.X = (double) Normal.X >= 0.0 ? box.Max.X : box.Min.X;
      vector3_2.Y = (double) Normal.Y >= 0.0 ? box.Max.Y : box.Min.Y;
      vector3_2.Z = (double) Normal.Z >= 0.0 ? box.Max.Z : box.Min.Z;
      if(Normal.X * (double) vector3_1.X + Normal.Y * (double) vector3_1.Y +
         Normal.Z * (double) vector3_1.Z + D > 0.0)
        return PlaneIntersectionType.Front;
      return (double) Normal.X * (double) vector3_2.X + (double) Normal.Y * (double) vector3_2.Y +
             (double) Normal.Z * (double) vector3_2.Z + (double) D < 0.0
        ? PlaneIntersectionType.Back
        : PlaneIntersectionType.Intersecting;
    }

    public void Intersects(ref BoundingBox box, out PlaneIntersectionType result)
    {
      Vector3 vector3_1;
      vector3_1.X = (double) Normal.X >= 0.0 ? box.Min.X : box.Max.X;
      vector3_1.Y = (double) Normal.Y >= 0.0 ? box.Min.Y : box.Max.Y;
      vector3_1.Z = (double) Normal.Z >= 0.0 ? box.Min.Z : box.Max.Z;
      Vector3 vector3_2;
      vector3_2.X = (double) Normal.X >= 0.0 ? box.Max.X : box.Min.X;
      vector3_2.Y = (double) Normal.Y >= 0.0 ? box.Max.Y : box.Min.Y;
      vector3_2.Z = (double) Normal.Z >= 0.0 ? box.Max.Z : box.Min.Z;
      if(Normal.X * (double) vector3_1.X + Normal.Y * (double) vector3_1.Y +
         Normal.Z * (double) vector3_1.Z + D > 0.0)
        result = PlaneIntersectionType.Front;
      else
        result = (double) Normal.X * (double) vector3_2.X + (double) Normal.Y * (double) vector3_2.Y +
                 (double) Normal.Z * (double) vector3_2.Z + (double) D >= 0.0
          ? PlaneIntersectionType.Intersecting
          : PlaneIntersectionType.Back;
    }

    public PlaneIntersectionType Intersects(BoundingSphere sphere)
    {
      float num = (float) (sphere.Center.X * (double) Normal.X +
                           sphere.Center.Y * (double) Normal.Y +
                           sphere.Center.Z * (double) Normal.Z) + D;
      if(num > (double) sphere.Radius)
        return PlaneIntersectionType.Front;
      return (double) num < -(double) sphere.Radius
        ? PlaneIntersectionType.Back
        : PlaneIntersectionType.Intersecting;
    }

    public void Intersects(ref BoundingSphere sphere, out PlaneIntersectionType result)
    {
      float num = (float) (sphere.Center.X * (double) Normal.X +
                           sphere.Center.Y * (double) Normal.Y +
                           sphere.Center.Z * (double) Normal.Z) + D;
      if(num > (double) sphere.Radius)
        result = PlaneIntersectionType.Front;
      else if(num < -(double) sphere.Radius)
        result = PlaneIntersectionType.Back;
      else
        result = PlaneIntersectionType.Intersecting;
    }

    public static bool operator ==(Plane lhs, Plane rhs)
    {
      return lhs.Equals(rhs);
    }

    public static bool operator !=(Plane lhs, Plane rhs)
    {
      if(lhs.Normal.X == (double) rhs.Normal.X && lhs.Normal.Y == (double) rhs.Normal.Y &&
         lhs.Normal.Z == (double) rhs.Normal.Z)
        return lhs.D != (double) rhs.D;
      return true;
    }
  }
}
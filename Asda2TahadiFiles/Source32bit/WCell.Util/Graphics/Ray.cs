using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace WCell.Util.Graphics
{
  [StructLayout(LayoutKind.Sequential, Size = 24)]
  public struct Ray : IEquatable<Ray>
  {
    public Vector3 Position;
    public Vector3 Direction;

    public Ray(Vector3 position, Vector3 direction)
    {
      Position = position;
      Direction = direction;
    }

    public bool Equals(Ray other)
    {
      return Position.X == (double) other.Position.X &&
             Position.Y == (double) other.Position.Y &&
             (Position.Z == (double) other.Position.Z &&
              Direction.X == (double) other.Direction.X) &&
             Direction.Y == (double) other.Direction.Y &&
             Direction.Z == (double) other.Direction.Z;
    }

    public override bool Equals(object obj)
    {
      bool flag = false;
      if(obj != null && obj is Ray)
        flag = Equals((Ray) obj);
      return flag;
    }

    public override int GetHashCode()
    {
      return Position.GetHashCode() + Direction.GetHashCode();
    }

    public override string ToString()
    {
      return string.Format(CultureInfo.CurrentCulture, "{{Position:{0} Direction:{1}}}", (object) Position.ToString(),
        (object) Direction.ToString());
    }

    public float? Intersects(BoundingBox box)
    {
      return box.Intersects(this);
    }

    public void Intersects(ref BoundingBox box, out float? result)
    {
      box.Intersects(ref this, out result);
    }

    public float? Intersects(Plane plane)
    {
      float num1 = (float) (plane.Normal.X * (double) Direction.X +
                            plane.Normal.Y * (double) Direction.Y +
                            plane.Normal.Z * (double) Direction.Z);
      if(Math.Abs(num1) < 9.99999974737875E-06)
        return new float?();
      float num2 = (float) (plane.Normal.X * (double) Position.X +
                            plane.Normal.Y * (double) Position.Y +
                            plane.Normal.Z * (double) Position.Z);
      float num3 = (-plane.D - num2) / num1;
      if(num3 < 0.0)
      {
        if(num3 < -9.99999974737875E-06)
          return new float?();
        num3 = 0.0f;
      }

      return num3;
    }

    public float Intersect(Plane plane)
    {
      float num1 = Vector3.Dot(plane.Normal, Direction);
      if(Math.Abs(num1) < 9.99999974737875E-06)
        return float.NaN;
      float num2 = Vector3.Dot(plane.Normal, Position);
      return (-plane.D - num2) / num1;
    }

    public void Intersects(ref Plane plane, out float? result)
    {
      float num1 = (float) (plane.Normal.X * (double) Direction.X +
                            plane.Normal.Y * (double) Direction.Y +
                            plane.Normal.Z * (double) Direction.Z);
      if(Math.Abs(num1) < 9.99999974737875E-06)
      {
        result = 0.0f;
      }
      else
      {
        float num2 = (float) (plane.Normal.X * (double) Position.X +
                              plane.Normal.Y * (double) Position.Y +
                              plane.Normal.Z * (double) Position.Z);
        float num3 = (-plane.D - num2) / num1;
        if(num3 < 0.0)
        {
          if(num3 < -9.99999974737875E-06)
          {
            result = 0.0f;
            return;
          }

          result = 0.0f;
        }

        result = num3;
      }
    }

    public float? Intersects(BoundingSphere sphere)
    {
      float num1 = sphere.Center.X - Position.X;
      float num2 = sphere.Center.Y - Position.Y;
      float num3 = sphere.Center.Z - Position.Z;
      float num4 = (float) (num1 * (double) num1 + num2 * (double) num2 +
                            num3 * (double) num3);
      float num5 = sphere.Radius * sphere.Radius;
      if(num4 <= (double) num5)
        return 0.0f;
      float num6 = (float) (num1 * (double) Direction.X +
                            num2 * (double) Direction.Y +
                            num3 * (double) Direction.Z);
      if(num6 < 0.0)
        return new float?();
      float num7 = num4 - num6 * num6;
      if(num7 > (double) num5)
        return new float?();
      float num8 = (float) Math.Sqrt(num5 - (double) num7);
      return num6 - num8;
    }

    public void Intersects(ref BoundingSphere sphere, out float? result)
    {
      float num1 = sphere.Center.X - Position.X;
      float num2 = sphere.Center.Y - Position.Y;
      float num3 = sphere.Center.Z - Position.Z;
      float num4 = (float) (num1 * (double) num1 + num2 * (double) num2 +
                            num3 * (double) num3);
      float num5 = sphere.Radius * sphere.Radius;
      if(num4 <= (double) num5)
      {
        result = 0.0f;
      }
      else
      {
        result = 0.0f;
        float num6 = (float) (num1 * (double) Direction.X +
                              num2 * (double) Direction.Y +
                              num3 * (double) Direction.Z);
        if(num6 >= 0.0)
        {
          float num7 = num4 - num6 * num6;
          if(num7 <= (double) num5)
          {
            float num8 = (float) Math.Sqrt(num5 - (double) num7);
            result = num6 - num8;
          }
        }
      }
    }

    public static bool operator ==(Ray a, Ray b)
    {
      return a.Position.X == (double) b.Position.X && a.Position.Y == (double) b.Position.Y &&
             (a.Position.Z == (double) b.Position.Z &&
              a.Direction.X == (double) b.Direction.X) &&
             a.Direction.Y == (double) b.Direction.Y && a.Direction.Z == (double) b.Direction.Z;
    }

    public static bool operator !=(Ray a, Ray b)
    {
      if(a.Position.X == (double) b.Position.X && a.Position.Y == (double) b.Position.Y &&
         (a.Position.Z == (double) b.Position.Z && a.Direction.X == (double) b.Direction.X) &&
         a.Direction.Y == (double) b.Direction.Y)
        return a.Direction.Z != (double) b.Direction.Z;
      return true;
    }
  }
}
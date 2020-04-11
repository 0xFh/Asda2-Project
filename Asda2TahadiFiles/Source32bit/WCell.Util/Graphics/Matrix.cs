using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace WCell.Util.Graphics
{
  [StructLayout(LayoutKind.Sequential, Size = 64)]
  public struct Matrix : IEquatable<Matrix>
  {
    private static Matrix _identity = new Matrix(1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f,
      0.0f, 0.0f, 0.0f, 1f);

    public float M11;
    public float M12;
    public float M13;
    public float M14;
    public float M21;
    public float M22;
    public float M23;
    public float M24;
    public float M31;
    public float M32;
    public float M33;
    public float M34;
    public float M41;
    public float M42;
    public float M43;
    public float M44;

    public static Matrix Identity
    {
      get { return _identity; }
    }

    public Vector3 Up
    {
      get
      {
        Vector3 vector3;
        vector3.X = M21;
        vector3.Y = M22;
        vector3.Z = M23;
        return vector3;
      }
      set
      {
        M21 = value.X;
        M22 = value.Y;
        M23 = value.Z;
      }
    }

    public Vector3 Down
    {
      get
      {
        Vector3 vector3;
        vector3.X = -M21;
        vector3.Y = -M22;
        vector3.Z = -M23;
        return vector3;
      }
      set
      {
        M21 = -value.X;
        M22 = -value.Y;
        M23 = -value.Z;
      }
    }

    public Vector3 Right
    {
      get
      {
        Vector3 vector3;
        vector3.X = M11;
        vector3.Y = M12;
        vector3.Z = M13;
        return vector3;
      }
      set
      {
        M11 = value.X;
        M12 = value.Y;
        M13 = value.Z;
      }
    }

    public Vector3 Left
    {
      get
      {
        Vector3 vector3;
        vector3.X = -M11;
        vector3.Y = -M12;
        vector3.Z = -M13;
        return vector3;
      }
      set
      {
        M11 = -value.X;
        M12 = -value.Y;
        M13 = -value.Z;
      }
    }

    public Vector3 Forward
    {
      get
      {
        Vector3 vector3;
        vector3.X = -M31;
        vector3.Y = -M32;
        vector3.Z = -M33;
        return vector3;
      }
      set
      {
        M31 = -value.X;
        M32 = -value.Y;
        M33 = -value.Z;
      }
    }

    public Vector3 Backward
    {
      get
      {
        Vector3 vector3;
        vector3.X = M31;
        vector3.Y = M32;
        vector3.Z = M33;
        return vector3;
      }
      set
      {
        M31 = value.X;
        M32 = value.Y;
        M33 = value.Z;
      }
    }

    public Vector3 Translation
    {
      get
      {
        Vector3 vector3;
        vector3.X = M41;
        vector3.Y = M42;
        vector3.Z = M43;
        return vector3;
      }
      set
      {
        M41 = value.X;
        M42 = value.Y;
        M43 = value.Z;
      }
    }

    public Matrix(float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24, float m31,
      float m32, float m33, float m34, float m41, float m42, float m43, float m44)
    {
      M11 = m11;
      M12 = m12;
      M13 = m13;
      M14 = m14;
      M21 = m21;
      M22 = m22;
      M23 = m23;
      M24 = m24;
      M31 = m31;
      M32 = m32;
      M33 = m33;
      M34 = m34;
      M41 = m41;
      M42 = m42;
      M43 = m43;
      M44 = m44;
    }

    public static Matrix CreateTranslation(Vector3 position)
    {
      Matrix matrix;
      matrix.M11 = 1f;
      matrix.M12 = 0.0f;
      matrix.M13 = 0.0f;
      matrix.M14 = 0.0f;
      matrix.M21 = 0.0f;
      matrix.M22 = 1f;
      matrix.M23 = 0.0f;
      matrix.M24 = 0.0f;
      matrix.M31 = 0.0f;
      matrix.M32 = 0.0f;
      matrix.M33 = 1f;
      matrix.M34 = 0.0f;
      matrix.M41 = position.X;
      matrix.M42 = position.Y;
      matrix.M43 = position.Z;
      matrix.M44 = 1f;
      return matrix;
    }

    public static void CreateTranslation(ref Vector3 position, out Matrix result)
    {
      result.M11 = 1f;
      result.M12 = 0.0f;
      result.M13 = 0.0f;
      result.M14 = 0.0f;
      result.M21 = 0.0f;
      result.M22 = 1f;
      result.M23 = 0.0f;
      result.M24 = 0.0f;
      result.M31 = 0.0f;
      result.M32 = 0.0f;
      result.M33 = 1f;
      result.M34 = 0.0f;
      result.M41 = position.X;
      result.M42 = position.Y;
      result.M43 = position.Z;
      result.M44 = 1f;
    }

    public static Matrix CreateTranslation(float xPosition, float yPosition, float zPosition)
    {
      Matrix matrix;
      matrix.M11 = 1f;
      matrix.M12 = 0.0f;
      matrix.M13 = 0.0f;
      matrix.M14 = 0.0f;
      matrix.M21 = 0.0f;
      matrix.M22 = 1f;
      matrix.M23 = 0.0f;
      matrix.M24 = 0.0f;
      matrix.M31 = 0.0f;
      matrix.M32 = 0.0f;
      matrix.M33 = 1f;
      matrix.M34 = 0.0f;
      matrix.M41 = xPosition;
      matrix.M42 = yPosition;
      matrix.M43 = zPosition;
      matrix.M44 = 1f;
      return matrix;
    }

    public static void CreateTranslation(float xPosition, float yPosition, float zPosition, out Matrix result)
    {
      result.M11 = 1f;
      result.M12 = 0.0f;
      result.M13 = 0.0f;
      result.M14 = 0.0f;
      result.M21 = 0.0f;
      result.M22 = 1f;
      result.M23 = 0.0f;
      result.M24 = 0.0f;
      result.M31 = 0.0f;
      result.M32 = 0.0f;
      result.M33 = 1f;
      result.M34 = 0.0f;
      result.M41 = xPosition;
      result.M42 = yPosition;
      result.M43 = zPosition;
      result.M44 = 1f;
    }

    public static Matrix CreateScale(float xScale, float yScale, float zScale)
    {
      float num1 = xScale;
      float num2 = yScale;
      float num3 = zScale;
      Matrix matrix;
      matrix.M11 = num1;
      matrix.M12 = 0.0f;
      matrix.M13 = 0.0f;
      matrix.M14 = 0.0f;
      matrix.M21 = 0.0f;
      matrix.M22 = num2;
      matrix.M23 = 0.0f;
      matrix.M24 = 0.0f;
      matrix.M31 = 0.0f;
      matrix.M32 = 0.0f;
      matrix.M33 = num3;
      matrix.M34 = 0.0f;
      matrix.M41 = 0.0f;
      matrix.M42 = 0.0f;
      matrix.M43 = 0.0f;
      matrix.M44 = 1f;
      return matrix;
    }

    public static void CreateScale(float xScale, float yScale, float zScale, out Matrix result)
    {
      float num1 = xScale;
      float num2 = yScale;
      float num3 = zScale;
      result.M11 = num1;
      result.M12 = 0.0f;
      result.M13 = 0.0f;
      result.M14 = 0.0f;
      result.M21 = 0.0f;
      result.M22 = num2;
      result.M23 = 0.0f;
      result.M24 = 0.0f;
      result.M31 = 0.0f;
      result.M32 = 0.0f;
      result.M33 = num3;
      result.M34 = 0.0f;
      result.M41 = 0.0f;
      result.M42 = 0.0f;
      result.M43 = 0.0f;
      result.M44 = 1f;
    }

    public static Matrix CreateScale(Vector3 scales)
    {
      float x = scales.X;
      float y = scales.Y;
      float z = scales.Z;
      Matrix matrix;
      matrix.M11 = x;
      matrix.M12 = 0.0f;
      matrix.M13 = 0.0f;
      matrix.M14 = 0.0f;
      matrix.M21 = 0.0f;
      matrix.M22 = y;
      matrix.M23 = 0.0f;
      matrix.M24 = 0.0f;
      matrix.M31 = 0.0f;
      matrix.M32 = 0.0f;
      matrix.M33 = z;
      matrix.M34 = 0.0f;
      matrix.M41 = 0.0f;
      matrix.M42 = 0.0f;
      matrix.M43 = 0.0f;
      matrix.M44 = 1f;
      return matrix;
    }

    public static void CreateScale(ref Vector3 scales, out Matrix result)
    {
      float x = scales.X;
      float y = scales.Y;
      float z = scales.Z;
      result.M11 = x;
      result.M12 = 0.0f;
      result.M13 = 0.0f;
      result.M14 = 0.0f;
      result.M21 = 0.0f;
      result.M22 = y;
      result.M23 = 0.0f;
      result.M24 = 0.0f;
      result.M31 = 0.0f;
      result.M32 = 0.0f;
      result.M33 = z;
      result.M34 = 0.0f;
      result.M41 = 0.0f;
      result.M42 = 0.0f;
      result.M43 = 0.0f;
      result.M44 = 1f;
    }

    public static Matrix CreateScale(float scale)
    {
      float num = scale;
      Matrix matrix;
      matrix.M11 = num;
      matrix.M12 = 0.0f;
      matrix.M13 = 0.0f;
      matrix.M14 = 0.0f;
      matrix.M21 = 0.0f;
      matrix.M22 = num;
      matrix.M23 = 0.0f;
      matrix.M24 = 0.0f;
      matrix.M31 = 0.0f;
      matrix.M32 = 0.0f;
      matrix.M33 = num;
      matrix.M34 = 0.0f;
      matrix.M41 = 0.0f;
      matrix.M42 = 0.0f;
      matrix.M43 = 0.0f;
      matrix.M44 = 1f;
      return matrix;
    }

    public static void CreateScale(float scale, out Matrix result)
    {
      float num = scale;
      result.M11 = num;
      result.M12 = 0.0f;
      result.M13 = 0.0f;
      result.M14 = 0.0f;
      result.M21 = 0.0f;
      result.M22 = num;
      result.M23 = 0.0f;
      result.M24 = 0.0f;
      result.M31 = 0.0f;
      result.M32 = 0.0f;
      result.M33 = num;
      result.M34 = 0.0f;
      result.M41 = 0.0f;
      result.M42 = 0.0f;
      result.M43 = 0.0f;
      result.M44 = 1f;
    }

    public static Matrix CreateRotationX(float radians)
    {
      float num1 = (float) Math.Cos(radians);
      float num2 = (float) Math.Sin(radians);
      Matrix matrix;
      matrix.M11 = 1f;
      matrix.M12 = 0.0f;
      matrix.M13 = 0.0f;
      matrix.M14 = 0.0f;
      matrix.M21 = 0.0f;
      matrix.M22 = num1;
      matrix.M23 = num2;
      matrix.M24 = 0.0f;
      matrix.M31 = 0.0f;
      matrix.M32 = -num2;
      matrix.M33 = num1;
      matrix.M34 = 0.0f;
      matrix.M41 = 0.0f;
      matrix.M42 = 0.0f;
      matrix.M43 = 0.0f;
      matrix.M44 = 1f;
      return matrix;
    }

    public static void CreateRotationX(float radians, out Matrix result)
    {
      float num1 = (float) Math.Cos(radians);
      float num2 = (float) Math.Sin(radians);
      result.M11 = 1f;
      result.M12 = 0.0f;
      result.M13 = 0.0f;
      result.M14 = 0.0f;
      result.M21 = 0.0f;
      result.M22 = num1;
      result.M23 = num2;
      result.M24 = 0.0f;
      result.M31 = 0.0f;
      result.M32 = -num2;
      result.M33 = num1;
      result.M34 = 0.0f;
      result.M41 = 0.0f;
      result.M42 = 0.0f;
      result.M43 = 0.0f;
      result.M44 = 1f;
    }

    public static Matrix CreateRotationY(float radians)
    {
      float num1 = (float) Math.Cos(radians);
      float num2 = (float) Math.Sin(radians);
      Matrix matrix;
      matrix.M11 = num1;
      matrix.M12 = 0.0f;
      matrix.M13 = -num2;
      matrix.M14 = 0.0f;
      matrix.M21 = 0.0f;
      matrix.M22 = 1f;
      matrix.M23 = 0.0f;
      matrix.M24 = 0.0f;
      matrix.M31 = num2;
      matrix.M32 = 0.0f;
      matrix.M33 = num1;
      matrix.M34 = 0.0f;
      matrix.M41 = 0.0f;
      matrix.M42 = 0.0f;
      matrix.M43 = 0.0f;
      matrix.M44 = 1f;
      return matrix;
    }

    public static void CreateRotationY(float radians, out Matrix result)
    {
      float num1 = (float) Math.Cos(radians);
      float num2 = (float) Math.Sin(radians);
      result.M11 = num1;
      result.M12 = 0.0f;
      result.M13 = -num2;
      result.M14 = 0.0f;
      result.M21 = 0.0f;
      result.M22 = 1f;
      result.M23 = 0.0f;
      result.M24 = 0.0f;
      result.M31 = num2;
      result.M32 = 0.0f;
      result.M33 = num1;
      result.M34 = 0.0f;
      result.M41 = 0.0f;
      result.M42 = 0.0f;
      result.M43 = 0.0f;
      result.M44 = 1f;
    }

    public static Matrix CreateRotationZ(float radians)
    {
      float num1 = (float) Math.Cos(radians);
      float num2 = (float) Math.Sin(radians);
      Matrix matrix;
      matrix.M11 = num1;
      matrix.M12 = num2;
      matrix.M13 = 0.0f;
      matrix.M14 = 0.0f;
      matrix.M21 = -num2;
      matrix.M22 = num1;
      matrix.M23 = 0.0f;
      matrix.M24 = 0.0f;
      matrix.M31 = 0.0f;
      matrix.M32 = 0.0f;
      matrix.M33 = 1f;
      matrix.M34 = 0.0f;
      matrix.M41 = 0.0f;
      matrix.M42 = 0.0f;
      matrix.M43 = 0.0f;
      matrix.M44 = 1f;
      return matrix;
    }

    public static void CreateRotationZ(float radians, out Matrix result)
    {
      float num1 = (float) Math.Cos(radians);
      float num2 = (float) Math.Sin(radians);
      result.M11 = num1;
      result.M12 = num2;
      result.M13 = 0.0f;
      result.M14 = 0.0f;
      result.M21 = -num2;
      result.M22 = num1;
      result.M23 = 0.0f;
      result.M24 = 0.0f;
      result.M31 = 0.0f;
      result.M32 = 0.0f;
      result.M33 = 1f;
      result.M34 = 0.0f;
      result.M41 = 0.0f;
      result.M42 = 0.0f;
      result.M43 = 0.0f;
      result.M44 = 1f;
    }

    public static Matrix CreateFromAxisAngle(Vector3 axis, float angle)
    {
      float x = axis.X;
      float y = axis.Y;
      float z = axis.Z;
      float num1 = (float) Math.Sin(angle);
      float num2 = (float) Math.Cos(angle);
      float num3 = x * x;
      float num4 = y * y;
      float num5 = z * z;
      float num6 = x * y;
      float num7 = x * z;
      float num8 = y * z;
      Matrix matrix;
      matrix.M11 = num3 + num2 * (1f - num3);
      matrix.M12 = (float) (num6 - num2 * (double) num6 + num1 * (double) z);
      matrix.M13 = (float) (num7 - num2 * (double) num7 - num1 * (double) y);
      matrix.M14 = 0.0f;
      matrix.M21 = (float) (num6 - num2 * (double) num6 - num1 * (double) z);
      matrix.M22 = num4 + num2 * (1f - num4);
      matrix.M23 = (float) (num8 - num2 * (double) num8 + num1 * (double) x);
      matrix.M24 = 0.0f;
      matrix.M31 = (float) (num7 - num2 * (double) num7 + num1 * (double) y);
      matrix.M32 = (float) (num8 - num2 * (double) num8 - num1 * (double) x);
      matrix.M33 = num5 + num2 * (1f - num5);
      matrix.M34 = 0.0f;
      matrix.M41 = 0.0f;
      matrix.M42 = 0.0f;
      matrix.M43 = 0.0f;
      matrix.M44 = 1f;
      return matrix;
    }

    public static void CreateFromAxisAngle(ref Vector3 axis, float angle, out Matrix result)
    {
      float x = axis.X;
      float y = axis.Y;
      float z = axis.Z;
      float num1 = (float) Math.Sin(angle);
      float num2 = (float) Math.Cos(angle);
      float num3 = x * x;
      float num4 = y * y;
      float num5 = z * z;
      float num6 = x * y;
      float num7 = x * z;
      float num8 = y * z;
      result.M11 = num3 + num2 * (1f - num3);
      result.M12 = (float) (num6 - num2 * (double) num6 + num1 * (double) z);
      result.M13 = (float) (num7 - num2 * (double) num7 - num1 * (double) y);
      result.M14 = 0.0f;
      result.M21 = (float) (num6 - num2 * (double) num6 - num1 * (double) z);
      result.M22 = num4 + num2 * (1f - num4);
      result.M23 = (float) (num8 - num2 * (double) num8 + num1 * (double) x);
      result.M24 = 0.0f;
      result.M31 = (float) (num7 - num2 * (double) num7 + num1 * (double) y);
      result.M32 = (float) (num8 - num2 * (double) num8 - num1 * (double) x);
      result.M33 = num5 + num2 * (1f - num5);
      result.M34 = 0.0f;
      result.M41 = 0.0f;
      result.M42 = 0.0f;
      result.M43 = 0.0f;
      result.M44 = 1f;
    }

    public static Matrix CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane)
    {
      Matrix matrix;
      matrix.M11 = 2f / width;
      matrix.M12 = matrix.M13 = matrix.M14 = 0.0f;
      matrix.M22 = 2f / height;
      matrix.M21 = matrix.M23 = matrix.M24 = 0.0f;
      matrix.M33 = (float) (1.0 / (zNearPlane - (double) zFarPlane));
      matrix.M31 = matrix.M32 = matrix.M34 = 0.0f;
      matrix.M41 = matrix.M42 = 0.0f;
      matrix.M43 = zNearPlane / (zNearPlane - zFarPlane);
      matrix.M44 = 1f;
      return matrix;
    }

    public static void CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane,
      out Matrix result)
    {
      result.M11 = 2f / width;
      result.M12 = result.M13 = result.M14 = 0.0f;
      result.M22 = 2f / height;
      result.M21 = result.M23 = result.M24 = 0.0f;
      result.M33 = (float) (1.0 / (zNearPlane - (double) zFarPlane));
      result.M31 = result.M32 = result.M34 = 0.0f;
      result.M41 = result.M42 = 0.0f;
      result.M43 = zNearPlane / (zNearPlane - zFarPlane);
      result.M44 = 1f;
    }

    public static Matrix CreateOrthographicOffCenter(float left, float right, float bottom, float top,
      float zNearPlane, float zFarPlane)
    {
      Matrix matrix;
      matrix.M11 = (float) (2.0 / (right - (double) left));
      matrix.M12 = matrix.M13 = matrix.M14 = 0.0f;
      matrix.M22 = (float) (2.0 / (top - (double) bottom));
      matrix.M21 = matrix.M23 = matrix.M24 = 0.0f;
      matrix.M33 = (float) (1.0 / (zNearPlane - (double) zFarPlane));
      matrix.M31 = matrix.M32 = matrix.M34 = 0.0f;
      matrix.M41 = (float) ((left + (double) right) / (left - (double) right));
      matrix.M42 = (float) ((top + (double) bottom) / (bottom - (double) top));
      matrix.M43 = zNearPlane / (zNearPlane - zFarPlane);
      matrix.M44 = 1f;
      return matrix;
    }

    public static void CreateOrthographicOffCenter(float left, float right, float bottom, float top,
      float zNearPlane, float zFarPlane, out Matrix result)
    {
      result.M11 = (float) (2.0 / (right - (double) left));
      result.M12 = result.M13 = result.M14 = 0.0f;
      result.M22 = (float) (2.0 / (top - (double) bottom));
      result.M21 = result.M23 = result.M24 = 0.0f;
      result.M33 = (float) (1.0 / (zNearPlane - (double) zFarPlane));
      result.M31 = result.M32 = result.M34 = 0.0f;
      result.M41 = (float) ((left + (double) right) / (left - (double) right));
      result.M42 = (float) ((top + (double) bottom) / (bottom - (double) top));
      result.M43 = zNearPlane / (zNearPlane - zFarPlane);
      result.M44 = 1f;
    }

    public static Matrix CreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
    {
      Vector3 vector3_1 = Vector3.Normalize(cameraPosition - cameraTarget);
      Vector3 vector3_2 = Vector3.Normalize(Vector3.Cross(cameraUpVector, vector3_1));
      Vector3 vector1 = Vector3.Cross(vector3_1, vector3_2);
      Matrix matrix;
      matrix.M11 = vector3_2.X;
      matrix.M12 = vector1.X;
      matrix.M13 = vector3_1.X;
      matrix.M14 = 0.0f;
      matrix.M21 = vector3_2.Y;
      matrix.M22 = vector1.Y;
      matrix.M23 = vector3_1.Y;
      matrix.M24 = 0.0f;
      matrix.M31 = vector3_2.Z;
      matrix.M32 = vector1.Z;
      matrix.M33 = vector3_1.Z;
      matrix.M34 = 0.0f;
      matrix.M41 = -Vector3.Dot(vector3_2, cameraPosition);
      matrix.M42 = -Vector3.Dot(vector1, cameraPosition);
      matrix.M43 = -Vector3.Dot(vector3_1, cameraPosition);
      matrix.M44 = 1f;
      return matrix;
    }

    public static void CreateLookAt(ref Vector3 cameraPosition, ref Vector3 cameraTarget,
      ref Vector3 cameraUpVector, out Matrix result)
    {
      Vector3 vector3_1 = Vector3.Normalize(cameraPosition - cameraTarget);
      Vector3 vector3_2 = Vector3.Normalize(Vector3.Cross(cameraUpVector, vector3_1));
      Vector3 vector1 = Vector3.Cross(vector3_1, vector3_2);
      result.M11 = vector3_2.X;
      result.M12 = vector1.X;
      result.M13 = vector3_1.X;
      result.M14 = 0.0f;
      result.M21 = vector3_2.Y;
      result.M22 = vector1.Y;
      result.M23 = vector3_1.Y;
      result.M24 = 0.0f;
      result.M31 = vector3_2.Z;
      result.M32 = vector1.Z;
      result.M33 = vector3_1.Z;
      result.M34 = 0.0f;
      result.M41 = -Vector3.Dot(vector3_2, cameraPosition);
      result.M42 = -Vector3.Dot(vector1, cameraPosition);
      result.M43 = -Vector3.Dot(vector3_1, cameraPosition);
      result.M44 = 1f;
    }

    public static Matrix CreateWorld(Vector3 position, Vector3 forward, Vector3 up)
    {
      Vector3 vector3_1 = Vector3.Normalize(-forward);
      Vector3 vector2 = Vector3.Normalize(Vector3.Cross(up, vector3_1));
      Vector3 vector3_2 = Vector3.Cross(vector3_1, vector2);
      Matrix matrix;
      matrix.M11 = vector2.X;
      matrix.M12 = vector2.Y;
      matrix.M13 = vector2.Z;
      matrix.M14 = 0.0f;
      matrix.M21 = vector3_2.X;
      matrix.M22 = vector3_2.Y;
      matrix.M23 = vector3_2.Z;
      matrix.M24 = 0.0f;
      matrix.M31 = vector3_1.X;
      matrix.M32 = vector3_1.Y;
      matrix.M33 = vector3_1.Z;
      matrix.M34 = 0.0f;
      matrix.M41 = position.X;
      matrix.M42 = position.Y;
      matrix.M43 = position.Z;
      matrix.M44 = 1f;
      return matrix;
    }

    public static void CreateWorld(ref Vector3 position, ref Vector3 forward, ref Vector3 up, out Matrix result)
    {
      Vector3 vector3_1 = Vector3.Normalize(-forward);
      Vector3 vector2 = Vector3.Normalize(Vector3.Cross(up, vector3_1));
      Vector3 vector3_2 = Vector3.Cross(vector3_1, vector2);
      result.M11 = vector2.X;
      result.M12 = vector2.Y;
      result.M13 = vector2.Z;
      result.M14 = 0.0f;
      result.M21 = vector3_2.X;
      result.M22 = vector3_2.Y;
      result.M23 = vector3_2.Z;
      result.M24 = 0.0f;
      result.M31 = vector3_1.X;
      result.M32 = vector3_1.Y;
      result.M33 = vector3_1.Z;
      result.M34 = 0.0f;
      result.M41 = position.X;
      result.M42 = position.Y;
      result.M43 = position.Z;
      result.M44 = 1f;
    }

    public static Matrix CreateFromQuaternion(Quaternion quaternion)
    {
      float num1 = quaternion.X * quaternion.X;
      float num2 = quaternion.Y * quaternion.Y;
      float num3 = quaternion.Z * quaternion.Z;
      float num4 = quaternion.X * quaternion.Y;
      float num5 = quaternion.Z * quaternion.W;
      float num6 = quaternion.Z * quaternion.X;
      float num7 = quaternion.Y * quaternion.W;
      float num8 = quaternion.Y * quaternion.Z;
      float num9 = quaternion.X * quaternion.W;
      Matrix matrix;
      matrix.M11 = (float) (1.0 - 2.0 * (num2 + (double) num3));
      matrix.M12 = (float) (2.0 * (num4 + (double) num5));
      matrix.M13 = (float) (2.0 * (num6 - (double) num7));
      matrix.M14 = 0.0f;
      matrix.M21 = (float) (2.0 * (num4 - (double) num5));
      matrix.M22 = (float) (1.0 - 2.0 * (num3 + (double) num1));
      matrix.M23 = (float) (2.0 * (num8 + (double) num9));
      matrix.M24 = 0.0f;
      matrix.M31 = (float) (2.0 * (num6 + (double) num7));
      matrix.M32 = (float) (2.0 * (num8 - (double) num9));
      matrix.M33 = (float) (1.0 - 2.0 * (num2 + (double) num1));
      matrix.M34 = 0.0f;
      matrix.M41 = 0.0f;
      matrix.M42 = 0.0f;
      matrix.M43 = 0.0f;
      matrix.M44 = 1f;
      return matrix;
    }

    public static void CreateFromQuaternion(ref Quaternion quaternion, out Matrix result)
    {
      float num1 = quaternion.X * quaternion.X;
      float num2 = quaternion.Y * quaternion.Y;
      float num3 = quaternion.Z * quaternion.Z;
      float num4 = quaternion.X * quaternion.Y;
      float num5 = quaternion.Z * quaternion.W;
      float num6 = quaternion.Z * quaternion.X;
      float num7 = quaternion.Y * quaternion.W;
      float num8 = quaternion.Y * quaternion.Z;
      float num9 = quaternion.X * quaternion.W;
      result.M11 = (float) (1.0 - 2.0 * (num2 + (double) num3));
      result.M12 = (float) (2.0 * (num4 + (double) num5));
      result.M13 = (float) (2.0 * (num6 - (double) num7));
      result.M14 = 0.0f;
      result.M21 = (float) (2.0 * (num4 - (double) num5));
      result.M22 = (float) (1.0 - 2.0 * (num3 + (double) num1));
      result.M23 = (float) (2.0 * (num8 + (double) num9));
      result.M24 = 0.0f;
      result.M31 = (float) (2.0 * (num6 + (double) num7));
      result.M32 = (float) (2.0 * (num8 - (double) num9));
      result.M33 = (float) (1.0 - 2.0 * (num2 + (double) num1));
      result.M34 = 0.0f;
      result.M41 = 0.0f;
      result.M42 = 0.0f;
      result.M43 = 0.0f;
      result.M44 = 1f;
    }

    public static Matrix CreateFromYawPitchRoll(float yaw, float pitch, float roll)
    {
      Quaternion result1;
      Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll, out result1);
      Matrix result2;
      CreateFromQuaternion(ref result1, out result2);
      return result2;
    }

    public static void CreateFromYawPitchRoll(float yaw, float pitch, float roll, out Matrix result)
    {
      Quaternion result1;
      Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll, out result1);
      CreateFromQuaternion(ref result1, out result);
    }

    public static Matrix Transform(Matrix value, Quaternion rotation)
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
      Matrix matrix;
      matrix.M11 = (float) (value.M11 * (double) num13 + value.M12 * (double) num14 +
                            value.M13 * (double) num15);
      matrix.M12 = (float) (value.M11 * (double) num16 + value.M12 * (double) num17 +
                            value.M13 * (double) num18);
      matrix.M13 = (float) (value.M11 * (double) num19 + value.M12 * (double) num20 +
                            value.M13 * (double) num21);
      matrix.M14 = value.M14;
      matrix.M21 = (float) (value.M21 * (double) num13 + value.M22 * (double) num14 +
                            value.M23 * (double) num15);
      matrix.M22 = (float) (value.M21 * (double) num16 + value.M22 * (double) num17 +
                            value.M23 * (double) num18);
      matrix.M23 = (float) (value.M21 * (double) num19 + value.M22 * (double) num20 +
                            value.M23 * (double) num21);
      matrix.M24 = value.M24;
      matrix.M31 = (float) (value.M31 * (double) num13 + value.M32 * (double) num14 +
                            value.M33 * (double) num15);
      matrix.M32 = (float) (value.M31 * (double) num16 + value.M32 * (double) num17 +
                            value.M33 * (double) num18);
      matrix.M33 = (float) (value.M31 * (double) num19 + value.M32 * (double) num20 +
                            value.M33 * (double) num21);
      matrix.M34 = value.M34;
      matrix.M41 = (float) (value.M41 * (double) num13 + value.M42 * (double) num14 +
                            value.M43 * (double) num15);
      matrix.M42 = (float) (value.M41 * (double) num16 + value.M42 * (double) num17 +
                            value.M43 * (double) num18);
      matrix.M43 = (float) (value.M41 * (double) num19 + value.M42 * (double) num20 +
                            value.M43 * (double) num21);
      matrix.M44 = value.M44;
      return matrix;
    }

    public static void Transform(ref Matrix value, ref Quaternion rotation, out Matrix result)
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
      float num22 = (float) (value.M11 * (double) num13 + value.M12 * (double) num14 +
                             value.M13 * (double) num15);
      float num23 = (float) (value.M11 * (double) num16 + value.M12 * (double) num17 +
                             value.M13 * (double) num18);
      float num24 = (float) (value.M11 * (double) num19 + value.M12 * (double) num20 +
                             value.M13 * (double) num21);
      float m14 = value.M14;
      float num25 = (float) (value.M21 * (double) num13 + value.M22 * (double) num14 +
                             value.M23 * (double) num15);
      float num26 = (float) (value.M21 * (double) num16 + value.M22 * (double) num17 +
                             value.M23 * (double) num18);
      float num27 = (float) (value.M21 * (double) num19 + value.M22 * (double) num20 +
                             value.M23 * (double) num21);
      float m24 = value.M24;
      float num28 = (float) (value.M31 * (double) num13 + value.M32 * (double) num14 +
                             value.M33 * (double) num15);
      float num29 = (float) (value.M31 * (double) num16 + value.M32 * (double) num17 +
                             value.M33 * (double) num18);
      float num30 = (float) (value.M31 * (double) num19 + value.M32 * (double) num20 +
                             value.M33 * (double) num21);
      float m34 = value.M34;
      float num31 = (float) (value.M41 * (double) num13 + value.M42 * (double) num14 +
                             value.M43 * (double) num15);
      float num32 = (float) (value.M41 * (double) num16 + value.M42 * (double) num17 +
                             value.M43 * (double) num18);
      float num33 = (float) (value.M41 * (double) num19 + value.M42 * (double) num20 +
                             value.M43 * (double) num21);
      float m44 = value.M44;
      result.M11 = num22;
      result.M12 = num23;
      result.M13 = num24;
      result.M14 = m14;
      result.M21 = num25;
      result.M22 = num26;
      result.M23 = num27;
      result.M24 = m24;
      result.M31 = num28;
      result.M32 = num29;
      result.M33 = num30;
      result.M34 = m34;
      result.M41 = num31;
      result.M42 = num32;
      result.M43 = num33;
      result.M44 = m44;
    }

    public override string ToString()
    {
      CultureInfo currentCulture = CultureInfo.CurrentCulture;
      return "{ " +
             string.Format(currentCulture, "{{M11:{0} M12:{1} M13:{2} M14:{3}}} ",
               (object) M11.ToString(currentCulture),
               (object) M12.ToString(currentCulture),
               (object) M13.ToString(currentCulture),
               (object) M14.ToString(currentCulture)) +
             string.Format(currentCulture, "{{M21:{0} M22:{1} M23:{2} M24:{3}}} ",
               (object) M21.ToString(currentCulture),
               (object) M22.ToString(currentCulture),
               (object) M23.ToString(currentCulture),
               (object) M24.ToString(currentCulture)) +
             string.Format(currentCulture, "{{M31:{0} M32:{1} M33:{2} M34:{3}}} ",
               (object) M31.ToString(currentCulture),
               (object) M32.ToString(currentCulture),
               (object) M33.ToString(currentCulture),
               (object) M34.ToString(currentCulture)) + string.Format(
               currentCulture, "{{M41:{0} M42:{1} M43:{2} M44:{3}}} ",
               (object) M41.ToString(currentCulture),
               (object) M42.ToString(currentCulture),
               (object) M43.ToString(currentCulture),
               (object) M44.ToString(currentCulture)) + "}";
    }

    public bool Equals(Matrix other)
    {
      return M11 == (double) other.M11 && M22 == (double) other.M22 &&
             (M33 == (double) other.M33 && M44 == (double) other.M44) &&
             (M12 == (double) other.M12 && M13 == (double) other.M13 &&
              (M14 == (double) other.M14 && M21 == (double) other.M21)) &&
             (M23 == (double) other.M23 && M24 == (double) other.M24 &&
              (M31 == (double) other.M31 && M32 == (double) other.M32) &&
              (M34 == (double) other.M34 && M41 == (double) other.M41 &&
               M42 == (double) other.M42)) && M43 == (double) other.M43;
    }

    public override bool Equals(object obj)
    {
      bool flag = false;
      if(obj is Matrix)
        flag = Equals((Matrix) obj);
      return flag;
    }

    public override int GetHashCode()
    {
      return M11.GetHashCode() + M12.GetHashCode() + M13.GetHashCode() + M14.GetHashCode() +
             M21.GetHashCode() + M22.GetHashCode() + M23.GetHashCode() + M24.GetHashCode() +
             M31.GetHashCode() + M32.GetHashCode() + M33.GetHashCode() + M34.GetHashCode() +
             M41.GetHashCode() + M42.GetHashCode() + M43.GetHashCode() + M44.GetHashCode();
    }

    public static Matrix Transpose(Matrix matrix)
    {
      Matrix matrix1;
      matrix1.M11 = matrix.M11;
      matrix1.M12 = matrix.M21;
      matrix1.M13 = matrix.M31;
      matrix1.M14 = matrix.M41;
      matrix1.M21 = matrix.M12;
      matrix1.M22 = matrix.M22;
      matrix1.M23 = matrix.M32;
      matrix1.M24 = matrix.M42;
      matrix1.M31 = matrix.M13;
      matrix1.M32 = matrix.M23;
      matrix1.M33 = matrix.M33;
      matrix1.M34 = matrix.M43;
      matrix1.M41 = matrix.M14;
      matrix1.M42 = matrix.M24;
      matrix1.M43 = matrix.M34;
      matrix1.M44 = matrix.M44;
      return matrix1;
    }

    public static void Transpose(ref Matrix matrix, out Matrix result)
    {
      float m11 = matrix.M11;
      float m12 = matrix.M12;
      float m13 = matrix.M13;
      float m14 = matrix.M14;
      float m21 = matrix.M21;
      float m22 = matrix.M22;
      float m23 = matrix.M23;
      float m24 = matrix.M24;
      float m31 = matrix.M31;
      float m32 = matrix.M32;
      float m33 = matrix.M33;
      float m34 = matrix.M34;
      float m41 = matrix.M41;
      float m42 = matrix.M42;
      float m43 = matrix.M43;
      float m44 = matrix.M44;
      result.M11 = m11;
      result.M12 = m21;
      result.M13 = m31;
      result.M14 = m41;
      result.M21 = m12;
      result.M22 = m22;
      result.M23 = m32;
      result.M24 = m42;
      result.M31 = m13;
      result.M32 = m23;
      result.M33 = m33;
      result.M34 = m43;
      result.M41 = m14;
      result.M42 = m24;
      result.M43 = m34;
      result.M44 = m44;
    }

    public float Determinant()
    {
      float m11 = M11;
      float m12 = M12;
      float m13 = M13;
      float m14 = M14;
      float m21 = M21;
      float m22 = M22;
      float m23 = M23;
      float m24 = M24;
      float m31 = M31;
      float m32 = M32;
      float m33 = M33;
      float m34 = M34;
      float m41 = M41;
      float m42 = M42;
      float m43 = M43;
      float m44 = M44;
      float num1 = (float) (m33 * (double) m44 - m34 * (double) m43);
      float num2 = (float) (m32 * (double) m44 - m34 * (double) m42);
      float num3 = (float) (m32 * (double) m43 - m33 * (double) m42);
      float num4 = (float) (m31 * (double) m44 - m34 * (double) m41);
      float num5 = (float) (m31 * (double) m43 - m33 * (double) m41);
      float num6 = (float) (m31 * (double) m42 - m32 * (double) m41);
      return (float) (
        m11 * (m22 * (double) num1 - m23 * (double) num2 +
               m24 * (double) num3) -
        m12 * (m21 * (double) num1 - m23 * (double) num4 +
               m24 * (double) num5) +
        m13 * (m21 * (double) num2 - m22 * (double) num4 +
               m24 * (double) num6) - m14 *
        (m21 * (double) num3 - m22 * (double) num5 + m23 * (double) num6));
    }

    public static Matrix Invert(Matrix matrix)
    {
      float m11 = matrix.M11;
      float m12 = matrix.M12;
      float m13 = matrix.M13;
      float m14 = matrix.M14;
      float m21 = matrix.M21;
      float m22 = matrix.M22;
      float m23 = matrix.M23;
      float m24 = matrix.M24;
      float m31 = matrix.M31;
      float m32 = matrix.M32;
      float m33 = matrix.M33;
      float m34 = matrix.M34;
      float m41 = matrix.M41;
      float m42 = matrix.M42;
      float m43 = matrix.M43;
      float m44 = matrix.M44;
      float num1 = (float) (m33 * (double) m44 - m34 * (double) m43);
      float num2 = (float) (m32 * (double) m44 - m34 * (double) m42);
      float num3 = (float) (m32 * (double) m43 - m33 * (double) m42);
      float num4 = (float) (m31 * (double) m44 - m34 * (double) m41);
      float num5 = (float) (m31 * (double) m43 - m33 * (double) m41);
      float num6 = (float) (m31 * (double) m42 - m32 * (double) m41);
      float num7 = (float) (m22 * (double) num1 - m23 * (double) num2 +
                            m24 * (double) num3);
      float num8 = (float) -(m21 * (double) num1 - m23 * (double) num4 +
                             m24 * (double) num5);
      float num9 = (float) (m21 * (double) num2 - m22 * (double) num4 +
                            m24 * (double) num6);
      float num10 = (float) -(m21 * (double) num3 - m22 * (double) num5 +
                              m23 * (double) num6);
      float num11 = (float) (1.0 / (m11 * (double) num7 + m12 * (double) num8 +
                                    m13 * (double) num9 + m14 * (double) num10));
      Matrix matrix1;
      matrix1.M11 = num7 * num11;
      matrix1.M21 = num8 * num11;
      matrix1.M31 = num9 * num11;
      matrix1.M41 = num10 * num11;
      matrix1.M12 =
        (float) -(m12 * (double) num1 - m13 * (double) num2 + m14 * (double) num3) *
        num11;
      matrix1.M22 =
        (float) (m11 * (double) num1 - m13 * (double) num4 + m14 * (double) num5) *
        num11;
      matrix1.M32 =
        (float) -(m11 * (double) num2 - m12 * (double) num4 + m14 * (double) num6) *
        num11;
      matrix1.M42 =
        (float) (m11 * (double) num3 - m12 * (double) num5 + m13 * (double) num6) *
        num11;
      float num12 = (float) (m23 * (double) m44 - m24 * (double) m43);
      float num13 = (float) (m22 * (double) m44 - m24 * (double) m42);
      float num14 = (float) (m22 * (double) m43 - m23 * (double) m42);
      float num15 = (float) (m21 * (double) m44 - m24 * (double) m41);
      float num16 = (float) (m21 * (double) m43 - m23 * (double) m41);
      float num17 = (float) (m21 * (double) m42 - m22 * (double) m41);
      matrix1.M13 = (float) (m12 * (double) num12 - m13 * (double) num13 +
                             m14 * (double) num14) * num11;
      matrix1.M23 = (float) -(m11 * (double) num12 - m13 * (double) num15 +
                              m14 * (double) num16) * num11;
      matrix1.M33 = (float) (m11 * (double) num13 - m12 * (double) num15 +
                             m14 * (double) num17) * num11;
      matrix1.M43 = (float) -(m11 * (double) num14 - m12 * (double) num16 +
                              m13 * (double) num17) * num11;
      float num18 = (float) (m23 * (double) m34 - m24 * (double) m33);
      float num19 = (float) (m22 * (double) m34 - m24 * (double) m32);
      float num20 = (float) (m22 * (double) m33 - m23 * (double) m32);
      float num21 = (float) (m21 * (double) m34 - m24 * (double) m31);
      float num22 = (float) (m21 * (double) m33 - m23 * (double) m31);
      float num23 = (float) (m21 * (double) m32 - m22 * (double) m31);
      matrix1.M14 = (float) -(m12 * (double) num18 - m13 * (double) num19 +
                              m14 * (double) num20) * num11;
      matrix1.M24 = (float) (m11 * (double) num18 - m13 * (double) num21 +
                             m14 * (double) num22) * num11;
      matrix1.M34 = (float) -(m11 * (double) num19 - m12 * (double) num21 +
                              m14 * (double) num23) * num11;
      matrix1.M44 = (float) (m11 * (double) num20 - m12 * (double) num22 +
                             m13 * (double) num23) * num11;
      return matrix1;
    }

    public static void Invert(ref Matrix matrix, out Matrix result)
    {
      float m11 = matrix.M11;
      float m12 = matrix.M12;
      float m13 = matrix.M13;
      float m14 = matrix.M14;
      float m21 = matrix.M21;
      float m22 = matrix.M22;
      float m23 = matrix.M23;
      float m24 = matrix.M24;
      float m31 = matrix.M31;
      float m32 = matrix.M32;
      float m33 = matrix.M33;
      float m34 = matrix.M34;
      float m41 = matrix.M41;
      float m42 = matrix.M42;
      float m43 = matrix.M43;
      float m44 = matrix.M44;
      float num1 = (float) (m33 * (double) m44 - m34 * (double) m43);
      float num2 = (float) (m32 * (double) m44 - m34 * (double) m42);
      float num3 = (float) (m32 * (double) m43 - m33 * (double) m42);
      float num4 = (float) (m31 * (double) m44 - m34 * (double) m41);
      float num5 = (float) (m31 * (double) m43 - m33 * (double) m41);
      float num6 = (float) (m31 * (double) m42 - m32 * (double) m41);
      float num7 = (float) (m22 * (double) num1 - m23 * (double) num2 +
                            m24 * (double) num3);
      float num8 = (float) -(m21 * (double) num1 - m23 * (double) num4 +
                             m24 * (double) num5);
      float num9 = (float) (m21 * (double) num2 - m22 * (double) num4 +
                            m24 * (double) num6);
      float num10 = (float) -(m21 * (double) num3 - m22 * (double) num5 +
                              m23 * (double) num6);
      float num11 = (float) (1.0 / (m11 * (double) num7 + m12 * (double) num8 +
                                    m13 * (double) num9 + m14 * (double) num10));
      result.M11 = num7 * num11;
      result.M21 = num8 * num11;
      result.M31 = num9 * num11;
      result.M41 = num10 * num11;
      result.M12 =
        (float) -(m12 * (double) num1 - m13 * (double) num2 + m14 * (double) num3) *
        num11;
      result.M22 =
        (float) (m11 * (double) num1 - m13 * (double) num4 + m14 * (double) num5) *
        num11;
      result.M32 =
        (float) -(m11 * (double) num2 - m12 * (double) num4 + m14 * (double) num6) *
        num11;
      result.M42 =
        (float) (m11 * (double) num3 - m12 * (double) num5 + m13 * (double) num6) *
        num11;
      float num12 = (float) (m23 * (double) m44 - m24 * (double) m43);
      float num13 = (float) (m22 * (double) m44 - m24 * (double) m42);
      float num14 = (float) (m22 * (double) m43 - m23 * (double) m42);
      float num15 = (float) (m21 * (double) m44 - m24 * (double) m41);
      float num16 = (float) (m21 * (double) m43 - m23 * (double) m41);
      float num17 = (float) (m21 * (double) m42 - m22 * (double) m41);
      result.M13 = (float) (m12 * (double) num12 - m13 * (double) num13 +
                            m14 * (double) num14) * num11;
      result.M23 = (float) -(m11 * (double) num12 - m13 * (double) num15 +
                             m14 * (double) num16) * num11;
      result.M33 = (float) (m11 * (double) num13 - m12 * (double) num15 +
                            m14 * (double) num17) * num11;
      result.M43 = (float) -(m11 * (double) num14 - m12 * (double) num16 +
                             m13 * (double) num17) * num11;
      float num18 = (float) (m23 * (double) m34 - m24 * (double) m33);
      float num19 = (float) (m22 * (double) m34 - m24 * (double) m32);
      float num20 = (float) (m22 * (double) m33 - m23 * (double) m32);
      float num21 = (float) (m21 * (double) m34 - m24 * (double) m31);
      float num22 = (float) (m21 * (double) m33 - m23 * (double) m31);
      float num23 = (float) (m21 * (double) m32 - m22 * (double) m31);
      result.M14 = (float) -(m12 * (double) num18 - m13 * (double) num19 +
                             m14 * (double) num20) * num11;
      result.M24 = (float) (m11 * (double) num18 - m13 * (double) num21 +
                            m14 * (double) num22) * num11;
      result.M34 = (float) -(m11 * (double) num19 - m12 * (double) num21 +
                             m14 * (double) num23) * num11;
      result.M44 = (float) (m11 * (double) num20 - m12 * (double) num22 +
                            m13 * (double) num23) * num11;
    }

    public static Matrix Lerp(Matrix matrix1, Matrix matrix2, float amount)
    {
      Matrix matrix;
      matrix.M11 = matrix1.M11 + (matrix2.M11 - matrix1.M11) * amount;
      matrix.M12 = matrix1.M12 + (matrix2.M12 - matrix1.M12) * amount;
      matrix.M13 = matrix1.M13 + (matrix2.M13 - matrix1.M13) * amount;
      matrix.M14 = matrix1.M14 + (matrix2.M14 - matrix1.M14) * amount;
      matrix.M21 = matrix1.M21 + (matrix2.M21 - matrix1.M21) * amount;
      matrix.M22 = matrix1.M22 + (matrix2.M22 - matrix1.M22) * amount;
      matrix.M23 = matrix1.M23 + (matrix2.M23 - matrix1.M23) * amount;
      matrix.M24 = matrix1.M24 + (matrix2.M24 - matrix1.M24) * amount;
      matrix.M31 = matrix1.M31 + (matrix2.M31 - matrix1.M31) * amount;
      matrix.M32 = matrix1.M32 + (matrix2.M32 - matrix1.M32) * amount;
      matrix.M33 = matrix1.M33 + (matrix2.M33 - matrix1.M33) * amount;
      matrix.M34 = matrix1.M34 + (matrix2.M34 - matrix1.M34) * amount;
      matrix.M41 = matrix1.M41 + (matrix2.M41 - matrix1.M41) * amount;
      matrix.M42 = matrix1.M42 + (matrix2.M42 - matrix1.M42) * amount;
      matrix.M43 = matrix1.M43 + (matrix2.M43 - matrix1.M43) * amount;
      matrix.M44 = matrix1.M44 + (matrix2.M44 - matrix1.M44) * amount;
      return matrix;
    }

    public static void Lerp(ref Matrix matrix1, ref Matrix matrix2, float amount, out Matrix result)
    {
      result.M11 = matrix1.M11 + (matrix2.M11 - matrix1.M11) * amount;
      result.M12 = matrix1.M12 + (matrix2.M12 - matrix1.M12) * amount;
      result.M13 = matrix1.M13 + (matrix2.M13 - matrix1.M13) * amount;
      result.M14 = matrix1.M14 + (matrix2.M14 - matrix1.M14) * amount;
      result.M21 = matrix1.M21 + (matrix2.M21 - matrix1.M21) * amount;
      result.M22 = matrix1.M22 + (matrix2.M22 - matrix1.M22) * amount;
      result.M23 = matrix1.M23 + (matrix2.M23 - matrix1.M23) * amount;
      result.M24 = matrix1.M24 + (matrix2.M24 - matrix1.M24) * amount;
      result.M31 = matrix1.M31 + (matrix2.M31 - matrix1.M31) * amount;
      result.M32 = matrix1.M32 + (matrix2.M32 - matrix1.M32) * amount;
      result.M33 = matrix1.M33 + (matrix2.M33 - matrix1.M33) * amount;
      result.M34 = matrix1.M34 + (matrix2.M34 - matrix1.M34) * amount;
      result.M41 = matrix1.M41 + (matrix2.M41 - matrix1.M41) * amount;
      result.M42 = matrix1.M42 + (matrix2.M42 - matrix1.M42) * amount;
      result.M43 = matrix1.M43 + (matrix2.M43 - matrix1.M43) * amount;
      result.M44 = matrix1.M44 + (matrix2.M44 - matrix1.M44) * amount;
    }

    public static Matrix Negate(Matrix matrix)
    {
      Matrix matrix1;
      matrix1.M11 = -matrix.M11;
      matrix1.M12 = -matrix.M12;
      matrix1.M13 = -matrix.M13;
      matrix1.M14 = -matrix.M14;
      matrix1.M21 = -matrix.M21;
      matrix1.M22 = -matrix.M22;
      matrix1.M23 = -matrix.M23;
      matrix1.M24 = -matrix.M24;
      matrix1.M31 = -matrix.M31;
      matrix1.M32 = -matrix.M32;
      matrix1.M33 = -matrix.M33;
      matrix1.M34 = -matrix.M34;
      matrix1.M41 = -matrix.M41;
      matrix1.M42 = -matrix.M42;
      matrix1.M43 = -matrix.M43;
      matrix1.M44 = -matrix.M44;
      return matrix1;
    }

    public static void Negate(ref Matrix matrix, out Matrix result)
    {
      result.M11 = -matrix.M11;
      result.M12 = -matrix.M12;
      result.M13 = -matrix.M13;
      result.M14 = -matrix.M14;
      result.M21 = -matrix.M21;
      result.M22 = -matrix.M22;
      result.M23 = -matrix.M23;
      result.M24 = -matrix.M24;
      result.M31 = -matrix.M31;
      result.M32 = -matrix.M32;
      result.M33 = -matrix.M33;
      result.M34 = -matrix.M34;
      result.M41 = -matrix.M41;
      result.M42 = -matrix.M42;
      result.M43 = -matrix.M43;
      result.M44 = -matrix.M44;
    }

    public static Matrix Add(Matrix matrix1, Matrix matrix2)
    {
      Matrix matrix;
      matrix.M11 = matrix1.M11 + matrix2.M11;
      matrix.M12 = matrix1.M12 + matrix2.M12;
      matrix.M13 = matrix1.M13 + matrix2.M13;
      matrix.M14 = matrix1.M14 + matrix2.M14;
      matrix.M21 = matrix1.M21 + matrix2.M21;
      matrix.M22 = matrix1.M22 + matrix2.M22;
      matrix.M23 = matrix1.M23 + matrix2.M23;
      matrix.M24 = matrix1.M24 + matrix2.M24;
      matrix.M31 = matrix1.M31 + matrix2.M31;
      matrix.M32 = matrix1.M32 + matrix2.M32;
      matrix.M33 = matrix1.M33 + matrix2.M33;
      matrix.M34 = matrix1.M34 + matrix2.M34;
      matrix.M41 = matrix1.M41 + matrix2.M41;
      matrix.M42 = matrix1.M42 + matrix2.M42;
      matrix.M43 = matrix1.M43 + matrix2.M43;
      matrix.M44 = matrix1.M44 + matrix2.M44;
      return matrix;
    }

    public static void Add(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
    {
      result.M11 = matrix1.M11 + matrix2.M11;
      result.M12 = matrix1.M12 + matrix2.M12;
      result.M13 = matrix1.M13 + matrix2.M13;
      result.M14 = matrix1.M14 + matrix2.M14;
      result.M21 = matrix1.M21 + matrix2.M21;
      result.M22 = matrix1.M22 + matrix2.M22;
      result.M23 = matrix1.M23 + matrix2.M23;
      result.M24 = matrix1.M24 + matrix2.M24;
      result.M31 = matrix1.M31 + matrix2.M31;
      result.M32 = matrix1.M32 + matrix2.M32;
      result.M33 = matrix1.M33 + matrix2.M33;
      result.M34 = matrix1.M34 + matrix2.M34;
      result.M41 = matrix1.M41 + matrix2.M41;
      result.M42 = matrix1.M42 + matrix2.M42;
      result.M43 = matrix1.M43 + matrix2.M43;
      result.M44 = matrix1.M44 + matrix2.M44;
    }

    public static Matrix Subtract(Matrix matrix1, Matrix matrix2)
    {
      Matrix matrix;
      matrix.M11 = matrix1.M11 - matrix2.M11;
      matrix.M12 = matrix1.M12 - matrix2.M12;
      matrix.M13 = matrix1.M13 - matrix2.M13;
      matrix.M14 = matrix1.M14 - matrix2.M14;
      matrix.M21 = matrix1.M21 - matrix2.M21;
      matrix.M22 = matrix1.M22 - matrix2.M22;
      matrix.M23 = matrix1.M23 - matrix2.M23;
      matrix.M24 = matrix1.M24 - matrix2.M24;
      matrix.M31 = matrix1.M31 - matrix2.M31;
      matrix.M32 = matrix1.M32 - matrix2.M32;
      matrix.M33 = matrix1.M33 - matrix2.M33;
      matrix.M34 = matrix1.M34 - matrix2.M34;
      matrix.M41 = matrix1.M41 - matrix2.M41;
      matrix.M42 = matrix1.M42 - matrix2.M42;
      matrix.M43 = matrix1.M43 - matrix2.M43;
      matrix.M44 = matrix1.M44 - matrix2.M44;
      return matrix;
    }

    public static void Subtract(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
    {
      result.M11 = matrix1.M11 - matrix2.M11;
      result.M12 = matrix1.M12 - matrix2.M12;
      result.M13 = matrix1.M13 - matrix2.M13;
      result.M14 = matrix1.M14 - matrix2.M14;
      result.M21 = matrix1.M21 - matrix2.M21;
      result.M22 = matrix1.M22 - matrix2.M22;
      result.M23 = matrix1.M23 - matrix2.M23;
      result.M24 = matrix1.M24 - matrix2.M24;
      result.M31 = matrix1.M31 - matrix2.M31;
      result.M32 = matrix1.M32 - matrix2.M32;
      result.M33 = matrix1.M33 - matrix2.M33;
      result.M34 = matrix1.M34 - matrix2.M34;
      result.M41 = matrix1.M41 - matrix2.M41;
      result.M42 = matrix1.M42 - matrix2.M42;
      result.M43 = matrix1.M43 - matrix2.M43;
      result.M44 = matrix1.M44 - matrix2.M44;
    }

    public static Matrix Multiply(Matrix matrix1, Matrix matrix2)
    {
      Matrix matrix;
      matrix.M11 = (float) (matrix1.M11 * (double) matrix2.M11 +
                            matrix1.M12 * (double) matrix2.M21 +
                            matrix1.M13 * (double) matrix2.M31 +
                            matrix1.M14 * (double) matrix2.M41);
      matrix.M12 = (float) (matrix1.M11 * (double) matrix2.M12 +
                            matrix1.M12 * (double) matrix2.M22 +
                            matrix1.M13 * (double) matrix2.M32 +
                            matrix1.M14 * (double) matrix2.M42);
      matrix.M13 = (float) (matrix1.M11 * (double) matrix2.M13 +
                            matrix1.M12 * (double) matrix2.M23 +
                            matrix1.M13 * (double) matrix2.M33 +
                            matrix1.M14 * (double) matrix2.M43);
      matrix.M14 = (float) (matrix1.M11 * (double) matrix2.M14 +
                            matrix1.M12 * (double) matrix2.M24 +
                            matrix1.M13 * (double) matrix2.M34 +
                            matrix1.M14 * (double) matrix2.M44);
      matrix.M21 = (float) (matrix1.M21 * (double) matrix2.M11 +
                            matrix1.M22 * (double) matrix2.M21 +
                            matrix1.M23 * (double) matrix2.M31 +
                            matrix1.M24 * (double) matrix2.M41);
      matrix.M22 = (float) (matrix1.M21 * (double) matrix2.M12 +
                            matrix1.M22 * (double) matrix2.M22 +
                            matrix1.M23 * (double) matrix2.M32 +
                            matrix1.M24 * (double) matrix2.M42);
      matrix.M23 = (float) (matrix1.M21 * (double) matrix2.M13 +
                            matrix1.M22 * (double) matrix2.M23 +
                            matrix1.M23 * (double) matrix2.M33 +
                            matrix1.M24 * (double) matrix2.M43);
      matrix.M24 = (float) (matrix1.M21 * (double) matrix2.M14 +
                            matrix1.M22 * (double) matrix2.M24 +
                            matrix1.M23 * (double) matrix2.M34 +
                            matrix1.M24 * (double) matrix2.M44);
      matrix.M31 = (float) (matrix1.M31 * (double) matrix2.M11 +
                            matrix1.M32 * (double) matrix2.M21 +
                            matrix1.M33 * (double) matrix2.M31 +
                            matrix1.M34 * (double) matrix2.M41);
      matrix.M32 = (float) (matrix1.M31 * (double) matrix2.M12 +
                            matrix1.M32 * (double) matrix2.M22 +
                            matrix1.M33 * (double) matrix2.M32 +
                            matrix1.M34 * (double) matrix2.M42);
      matrix.M33 = (float) (matrix1.M31 * (double) matrix2.M13 +
                            matrix1.M32 * (double) matrix2.M23 +
                            matrix1.M33 * (double) matrix2.M33 +
                            matrix1.M34 * (double) matrix2.M43);
      matrix.M34 = (float) (matrix1.M31 * (double) matrix2.M14 +
                            matrix1.M32 * (double) matrix2.M24 +
                            matrix1.M33 * (double) matrix2.M34 +
                            matrix1.M34 * (double) matrix2.M44);
      matrix.M41 = (float) (matrix1.M41 * (double) matrix2.M11 +
                            matrix1.M42 * (double) matrix2.M21 +
                            matrix1.M43 * (double) matrix2.M31 +
                            matrix1.M44 * (double) matrix2.M41);
      matrix.M42 = (float) (matrix1.M41 * (double) matrix2.M12 +
                            matrix1.M42 * (double) matrix2.M22 +
                            matrix1.M43 * (double) matrix2.M32 +
                            matrix1.M44 * (double) matrix2.M42);
      matrix.M43 = (float) (matrix1.M41 * (double) matrix2.M13 +
                            matrix1.M42 * (double) matrix2.M23 +
                            matrix1.M43 * (double) matrix2.M33 +
                            matrix1.M44 * (double) matrix2.M43);
      matrix.M44 = (float) (matrix1.M41 * (double) matrix2.M14 +
                            matrix1.M42 * (double) matrix2.M24 +
                            matrix1.M43 * (double) matrix2.M34 +
                            matrix1.M44 * (double) matrix2.M44);
      return matrix;
    }

    public static void Multiply(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
    {
      float num1 = (float) (matrix1.M11 * (double) matrix2.M11 +
                            matrix1.M12 * (double) matrix2.M21 +
                            matrix1.M13 * (double) matrix2.M31 +
                            matrix1.M14 * (double) matrix2.M41);
      float num2 = (float) (matrix1.M11 * (double) matrix2.M12 +
                            matrix1.M12 * (double) matrix2.M22 +
                            matrix1.M13 * (double) matrix2.M32 +
                            matrix1.M14 * (double) matrix2.M42);
      float num3 = (float) (matrix1.M11 * (double) matrix2.M13 +
                            matrix1.M12 * (double) matrix2.M23 +
                            matrix1.M13 * (double) matrix2.M33 +
                            matrix1.M14 * (double) matrix2.M43);
      float num4 = (float) (matrix1.M11 * (double) matrix2.M14 +
                            matrix1.M12 * (double) matrix2.M24 +
                            matrix1.M13 * (double) matrix2.M34 +
                            matrix1.M14 * (double) matrix2.M44);
      float num5 = (float) (matrix1.M21 * (double) matrix2.M11 +
                            matrix1.M22 * (double) matrix2.M21 +
                            matrix1.M23 * (double) matrix2.M31 +
                            matrix1.M24 * (double) matrix2.M41);
      float num6 = (float) (matrix1.M21 * (double) matrix2.M12 +
                            matrix1.M22 * (double) matrix2.M22 +
                            matrix1.M23 * (double) matrix2.M32 +
                            matrix1.M24 * (double) matrix2.M42);
      float num7 = (float) (matrix1.M21 * (double) matrix2.M13 +
                            matrix1.M22 * (double) matrix2.M23 +
                            matrix1.M23 * (double) matrix2.M33 +
                            matrix1.M24 * (double) matrix2.M43);
      float num8 = (float) (matrix1.M21 * (double) matrix2.M14 +
                            matrix1.M22 * (double) matrix2.M24 +
                            matrix1.M23 * (double) matrix2.M34 +
                            matrix1.M24 * (double) matrix2.M44);
      float num9 = (float) (matrix1.M31 * (double) matrix2.M11 +
                            matrix1.M32 * (double) matrix2.M21 +
                            matrix1.M33 * (double) matrix2.M31 +
                            matrix1.M34 * (double) matrix2.M41);
      float num10 = (float) (matrix1.M31 * (double) matrix2.M12 +
                             matrix1.M32 * (double) matrix2.M22 +
                             matrix1.M33 * (double) matrix2.M32 +
                             matrix1.M34 * (double) matrix2.M42);
      float num11 = (float) (matrix1.M31 * (double) matrix2.M13 +
                             matrix1.M32 * (double) matrix2.M23 +
                             matrix1.M33 * (double) matrix2.M33 +
                             matrix1.M34 * (double) matrix2.M43);
      float num12 = (float) (matrix1.M31 * (double) matrix2.M14 +
                             matrix1.M32 * (double) matrix2.M24 +
                             matrix1.M33 * (double) matrix2.M34 +
                             matrix1.M34 * (double) matrix2.M44);
      float num13 = (float) (matrix1.M41 * (double) matrix2.M11 +
                             matrix1.M42 * (double) matrix2.M21 +
                             matrix1.M43 * (double) matrix2.M31 +
                             matrix1.M44 * (double) matrix2.M41);
      float num14 = (float) (matrix1.M41 * (double) matrix2.M12 +
                             matrix1.M42 * (double) matrix2.M22 +
                             matrix1.M43 * (double) matrix2.M32 +
                             matrix1.M44 * (double) matrix2.M42);
      float num15 = (float) (matrix1.M41 * (double) matrix2.M13 +
                             matrix1.M42 * (double) matrix2.M23 +
                             matrix1.M43 * (double) matrix2.M33 +
                             matrix1.M44 * (double) matrix2.M43);
      float num16 = (float) (matrix1.M41 * (double) matrix2.M14 +
                             matrix1.M42 * (double) matrix2.M24 +
                             matrix1.M43 * (double) matrix2.M34 +
                             matrix1.M44 * (double) matrix2.M44);
      result.M11 = num1;
      result.M12 = num2;
      result.M13 = num3;
      result.M14 = num4;
      result.M21 = num5;
      result.M22 = num6;
      result.M23 = num7;
      result.M24 = num8;
      result.M31 = num9;
      result.M32 = num10;
      result.M33 = num11;
      result.M34 = num12;
      result.M41 = num13;
      result.M42 = num14;
      result.M43 = num15;
      result.M44 = num16;
    }

    public static Matrix Multiply(Matrix matrix1, float scaleFactor)
    {
      float num = scaleFactor;
      Matrix matrix;
      matrix.M11 = matrix1.M11 * num;
      matrix.M12 = matrix1.M12 * num;
      matrix.M13 = matrix1.M13 * num;
      matrix.M14 = matrix1.M14 * num;
      matrix.M21 = matrix1.M21 * num;
      matrix.M22 = matrix1.M22 * num;
      matrix.M23 = matrix1.M23 * num;
      matrix.M24 = matrix1.M24 * num;
      matrix.M31 = matrix1.M31 * num;
      matrix.M32 = matrix1.M32 * num;
      matrix.M33 = matrix1.M33 * num;
      matrix.M34 = matrix1.M34 * num;
      matrix.M41 = matrix1.M41 * num;
      matrix.M42 = matrix1.M42 * num;
      matrix.M43 = matrix1.M43 * num;
      matrix.M44 = matrix1.M44 * num;
      return matrix;
    }

    public static void Multiply(ref Matrix matrix1, float scaleFactor, out Matrix result)
    {
      float num = scaleFactor;
      result.M11 = matrix1.M11 * num;
      result.M12 = matrix1.M12 * num;
      result.M13 = matrix1.M13 * num;
      result.M14 = matrix1.M14 * num;
      result.M21 = matrix1.M21 * num;
      result.M22 = matrix1.M22 * num;
      result.M23 = matrix1.M23 * num;
      result.M24 = matrix1.M24 * num;
      result.M31 = matrix1.M31 * num;
      result.M32 = matrix1.M32 * num;
      result.M33 = matrix1.M33 * num;
      result.M34 = matrix1.M34 * num;
      result.M41 = matrix1.M41 * num;
      result.M42 = matrix1.M42 * num;
      result.M43 = matrix1.M43 * num;
      result.M44 = matrix1.M44 * num;
    }

    public static Matrix Divide(Matrix matrix1, Matrix matrix2)
    {
      Matrix matrix;
      matrix.M11 = matrix1.M11 / matrix2.M11;
      matrix.M12 = matrix1.M12 / matrix2.M12;
      matrix.M13 = matrix1.M13 / matrix2.M13;
      matrix.M14 = matrix1.M14 / matrix2.M14;
      matrix.M21 = matrix1.M21 / matrix2.M21;
      matrix.M22 = matrix1.M22 / matrix2.M22;
      matrix.M23 = matrix1.M23 / matrix2.M23;
      matrix.M24 = matrix1.M24 / matrix2.M24;
      matrix.M31 = matrix1.M31 / matrix2.M31;
      matrix.M32 = matrix1.M32 / matrix2.M32;
      matrix.M33 = matrix1.M33 / matrix2.M33;
      matrix.M34 = matrix1.M34 / matrix2.M34;
      matrix.M41 = matrix1.M41 / matrix2.M41;
      matrix.M42 = matrix1.M42 / matrix2.M42;
      matrix.M43 = matrix1.M43 / matrix2.M43;
      matrix.M44 = matrix1.M44 / matrix2.M44;
      return matrix;
    }

    public static void Divide(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
    {
      result.M11 = matrix1.M11 / matrix2.M11;
      result.M12 = matrix1.M12 / matrix2.M12;
      result.M13 = matrix1.M13 / matrix2.M13;
      result.M14 = matrix1.M14 / matrix2.M14;
      result.M21 = matrix1.M21 / matrix2.M21;
      result.M22 = matrix1.M22 / matrix2.M22;
      result.M23 = matrix1.M23 / matrix2.M23;
      result.M24 = matrix1.M24 / matrix2.M24;
      result.M31 = matrix1.M31 / matrix2.M31;
      result.M32 = matrix1.M32 / matrix2.M32;
      result.M33 = matrix1.M33 / matrix2.M33;
      result.M34 = matrix1.M34 / matrix2.M34;
      result.M41 = matrix1.M41 / matrix2.M41;
      result.M42 = matrix1.M42 / matrix2.M42;
      result.M43 = matrix1.M43 / matrix2.M43;
      result.M44 = matrix1.M44 / matrix2.M44;
    }

    public static Matrix Divide(Matrix matrix1, float divider)
    {
      float num = 1f / divider;
      Matrix matrix;
      matrix.M11 = matrix1.M11 * num;
      matrix.M12 = matrix1.M12 * num;
      matrix.M13 = matrix1.M13 * num;
      matrix.M14 = matrix1.M14 * num;
      matrix.M21 = matrix1.M21 * num;
      matrix.M22 = matrix1.M22 * num;
      matrix.M23 = matrix1.M23 * num;
      matrix.M24 = matrix1.M24 * num;
      matrix.M31 = matrix1.M31 * num;
      matrix.M32 = matrix1.M32 * num;
      matrix.M33 = matrix1.M33 * num;
      matrix.M34 = matrix1.M34 * num;
      matrix.M41 = matrix1.M41 * num;
      matrix.M42 = matrix1.M42 * num;
      matrix.M43 = matrix1.M43 * num;
      matrix.M44 = matrix1.M44 * num;
      return matrix;
    }

    public static void Divide(ref Matrix matrix1, float divider, out Matrix result)
    {
      float num = 1f / divider;
      result.M11 = matrix1.M11 * num;
      result.M12 = matrix1.M12 * num;
      result.M13 = matrix1.M13 * num;
      result.M14 = matrix1.M14 * num;
      result.M21 = matrix1.M21 * num;
      result.M22 = matrix1.M22 * num;
      result.M23 = matrix1.M23 * num;
      result.M24 = matrix1.M24 * num;
      result.M31 = matrix1.M31 * num;
      result.M32 = matrix1.M32 * num;
      result.M33 = matrix1.M33 * num;
      result.M34 = matrix1.M34 * num;
      result.M41 = matrix1.M41 * num;
      result.M42 = matrix1.M42 * num;
      result.M43 = matrix1.M43 * num;
      result.M44 = matrix1.M44 * num;
    }

    public static Matrix operator -(Matrix matrix1)
    {
      Matrix matrix;
      matrix.M11 = -matrix1.M11;
      matrix.M12 = -matrix1.M12;
      matrix.M13 = -matrix1.M13;
      matrix.M14 = -matrix1.M14;
      matrix.M21 = -matrix1.M21;
      matrix.M22 = -matrix1.M22;
      matrix.M23 = -matrix1.M23;
      matrix.M24 = -matrix1.M24;
      matrix.M31 = -matrix1.M31;
      matrix.M32 = -matrix1.M32;
      matrix.M33 = -matrix1.M33;
      matrix.M34 = -matrix1.M34;
      matrix.M41 = -matrix1.M41;
      matrix.M42 = -matrix1.M42;
      matrix.M43 = -matrix1.M43;
      matrix.M44 = -matrix1.M44;
      return matrix;
    }

    public static bool operator ==(Matrix matrix1, Matrix matrix2)
    {
      return matrix1.M11 == (double) matrix2.M11 && matrix1.M22 == (double) matrix2.M22 &&
             (matrix1.M33 == (double) matrix2.M33 && matrix1.M44 == (double) matrix2.M44) &&
             (matrix1.M12 == (double) matrix2.M12 && matrix1.M13 == (double) matrix2.M13 &&
              (matrix1.M14 == (double) matrix2.M14 && matrix1.M21 == (double) matrix2.M21)) &&
             (matrix1.M23 == (double) matrix2.M23 && matrix1.M24 == (double) matrix2.M24 &&
              (matrix1.M31 == (double) matrix2.M31 && matrix1.M32 == (double) matrix2.M32) &&
              (matrix1.M34 == (double) matrix2.M34 && matrix1.M41 == (double) matrix2.M41 &&
               matrix1.M42 == (double) matrix2.M42)) && matrix1.M43 == (double) matrix2.M43;
    }

    public static bool operator !=(Matrix matrix1, Matrix matrix2)
    {
      if(matrix1.M11 == (double) matrix2.M11 && matrix1.M12 == (double) matrix2.M12 &&
         (matrix1.M13 == (double) matrix2.M13 && matrix1.M14 == (double) matrix2.M14) &&
         (matrix1.M21 == (double) matrix2.M21 && matrix1.M22 == (double) matrix2.M22 &&
          (matrix1.M23 == (double) matrix2.M23 && matrix1.M24 == (double) matrix2.M24)) &&
         (matrix1.M31 == (double) matrix2.M31 && matrix1.M32 == (double) matrix2.M32 &&
          (matrix1.M33 == (double) matrix2.M33 && matrix1.M34 == (double) matrix2.M34) &&
          (matrix1.M41 == (double) matrix2.M41 && matrix1.M42 == (double) matrix2.M42 &&
           matrix1.M43 == (double) matrix2.M43)))
        return matrix1.M44 != (double) matrix2.M44;
      return true;
    }

    public static Matrix operator +(Matrix matrix1, Matrix matrix2)
    {
      Matrix matrix;
      matrix.M11 = matrix1.M11 + matrix2.M11;
      matrix.M12 = matrix1.M12 + matrix2.M12;
      matrix.M13 = matrix1.M13 + matrix2.M13;
      matrix.M14 = matrix1.M14 + matrix2.M14;
      matrix.M21 = matrix1.M21 + matrix2.M21;
      matrix.M22 = matrix1.M22 + matrix2.M22;
      matrix.M23 = matrix1.M23 + matrix2.M23;
      matrix.M24 = matrix1.M24 + matrix2.M24;
      matrix.M31 = matrix1.M31 + matrix2.M31;
      matrix.M32 = matrix1.M32 + matrix2.M32;
      matrix.M33 = matrix1.M33 + matrix2.M33;
      matrix.M34 = matrix1.M34 + matrix2.M34;
      matrix.M41 = matrix1.M41 + matrix2.M41;
      matrix.M42 = matrix1.M42 + matrix2.M42;
      matrix.M43 = matrix1.M43 + matrix2.M43;
      matrix.M44 = matrix1.M44 + matrix2.M44;
      return matrix;
    }

    public static Matrix operator -(Matrix matrix1, Matrix matrix2)
    {
      Matrix matrix;
      matrix.M11 = matrix1.M11 - matrix2.M11;
      matrix.M12 = matrix1.M12 - matrix2.M12;
      matrix.M13 = matrix1.M13 - matrix2.M13;
      matrix.M14 = matrix1.M14 - matrix2.M14;
      matrix.M21 = matrix1.M21 - matrix2.M21;
      matrix.M22 = matrix1.M22 - matrix2.M22;
      matrix.M23 = matrix1.M23 - matrix2.M23;
      matrix.M24 = matrix1.M24 - matrix2.M24;
      matrix.M31 = matrix1.M31 - matrix2.M31;
      matrix.M32 = matrix1.M32 - matrix2.M32;
      matrix.M33 = matrix1.M33 - matrix2.M33;
      matrix.M34 = matrix1.M34 - matrix2.M34;
      matrix.M41 = matrix1.M41 - matrix2.M41;
      matrix.M42 = matrix1.M42 - matrix2.M42;
      matrix.M43 = matrix1.M43 - matrix2.M43;
      matrix.M44 = matrix1.M44 - matrix2.M44;
      return matrix;
    }

    public static Matrix operator *(Matrix matrix1, Matrix matrix2)
    {
      Matrix matrix;
      matrix.M11 = (float) (matrix1.M11 * (double) matrix2.M11 +
                            matrix1.M12 * (double) matrix2.M21 +
                            matrix1.M13 * (double) matrix2.M31 +
                            matrix1.M14 * (double) matrix2.M41);
      matrix.M12 = (float) (matrix1.M11 * (double) matrix2.M12 +
                            matrix1.M12 * (double) matrix2.M22 +
                            matrix1.M13 * (double) matrix2.M32 +
                            matrix1.M14 * (double) matrix2.M42);
      matrix.M13 = (float) (matrix1.M11 * (double) matrix2.M13 +
                            matrix1.M12 * (double) matrix2.M23 +
                            matrix1.M13 * (double) matrix2.M33 +
                            matrix1.M14 * (double) matrix2.M43);
      matrix.M14 = (float) (matrix1.M11 * (double) matrix2.M14 +
                            matrix1.M12 * (double) matrix2.M24 +
                            matrix1.M13 * (double) matrix2.M34 +
                            matrix1.M14 * (double) matrix2.M44);
      matrix.M21 = (float) (matrix1.M21 * (double) matrix2.M11 +
                            matrix1.M22 * (double) matrix2.M21 +
                            matrix1.M23 * (double) matrix2.M31 +
                            matrix1.M24 * (double) matrix2.M41);
      matrix.M22 = (float) (matrix1.M21 * (double) matrix2.M12 +
                            matrix1.M22 * (double) matrix2.M22 +
                            matrix1.M23 * (double) matrix2.M32 +
                            matrix1.M24 * (double) matrix2.M42);
      matrix.M23 = (float) (matrix1.M21 * (double) matrix2.M13 +
                            matrix1.M22 * (double) matrix2.M23 +
                            matrix1.M23 * (double) matrix2.M33 +
                            matrix1.M24 * (double) matrix2.M43);
      matrix.M24 = (float) (matrix1.M21 * (double) matrix2.M14 +
                            matrix1.M22 * (double) matrix2.M24 +
                            matrix1.M23 * (double) matrix2.M34 +
                            matrix1.M24 * (double) matrix2.M44);
      matrix.M31 = (float) (matrix1.M31 * (double) matrix2.M11 +
                            matrix1.M32 * (double) matrix2.M21 +
                            matrix1.M33 * (double) matrix2.M31 +
                            matrix1.M34 * (double) matrix2.M41);
      matrix.M32 = (float) (matrix1.M31 * (double) matrix2.M12 +
                            matrix1.M32 * (double) matrix2.M22 +
                            matrix1.M33 * (double) matrix2.M32 +
                            matrix1.M34 * (double) matrix2.M42);
      matrix.M33 = (float) (matrix1.M31 * (double) matrix2.M13 +
                            matrix1.M32 * (double) matrix2.M23 +
                            matrix1.M33 * (double) matrix2.M33 +
                            matrix1.M34 * (double) matrix2.M43);
      matrix.M34 = (float) (matrix1.M31 * (double) matrix2.M14 +
                            matrix1.M32 * (double) matrix2.M24 +
                            matrix1.M33 * (double) matrix2.M34 +
                            matrix1.M34 * (double) matrix2.M44);
      matrix.M41 = (float) (matrix1.M41 * (double) matrix2.M11 +
                            matrix1.M42 * (double) matrix2.M21 +
                            matrix1.M43 * (double) matrix2.M31 +
                            matrix1.M44 * (double) matrix2.M41);
      matrix.M42 = (float) (matrix1.M41 * (double) matrix2.M12 +
                            matrix1.M42 * (double) matrix2.M22 +
                            matrix1.M43 * (double) matrix2.M32 +
                            matrix1.M44 * (double) matrix2.M42);
      matrix.M43 = (float) (matrix1.M41 * (double) matrix2.M13 +
                            matrix1.M42 * (double) matrix2.M23 +
                            matrix1.M43 * (double) matrix2.M33 +
                            matrix1.M44 * (double) matrix2.M43);
      matrix.M44 = (float) (matrix1.M41 * (double) matrix2.M14 +
                            matrix1.M42 * (double) matrix2.M24 +
                            matrix1.M43 * (double) matrix2.M34 +
                            matrix1.M44 * (double) matrix2.M44);
      return matrix;
    }

    public static Matrix operator *(Matrix matrix, float scaleFactor)
    {
      float num = scaleFactor;
      Matrix matrix1;
      matrix1.M11 = matrix.M11 * num;
      matrix1.M12 = matrix.M12 * num;
      matrix1.M13 = matrix.M13 * num;
      matrix1.M14 = matrix.M14 * num;
      matrix1.M21 = matrix.M21 * num;
      matrix1.M22 = matrix.M22 * num;
      matrix1.M23 = matrix.M23 * num;
      matrix1.M24 = matrix.M24 * num;
      matrix1.M31 = matrix.M31 * num;
      matrix1.M32 = matrix.M32 * num;
      matrix1.M33 = matrix.M33 * num;
      matrix1.M34 = matrix.M34 * num;
      matrix1.M41 = matrix.M41 * num;
      matrix1.M42 = matrix.M42 * num;
      matrix1.M43 = matrix.M43 * num;
      matrix1.M44 = matrix.M44 * num;
      return matrix1;
    }

    public static Matrix operator *(float scaleFactor, Matrix matrix)
    {
      float num = scaleFactor;
      Matrix matrix1;
      matrix1.M11 = matrix.M11 * num;
      matrix1.M12 = matrix.M12 * num;
      matrix1.M13 = matrix.M13 * num;
      matrix1.M14 = matrix.M14 * num;
      matrix1.M21 = matrix.M21 * num;
      matrix1.M22 = matrix.M22 * num;
      matrix1.M23 = matrix.M23 * num;
      matrix1.M24 = matrix.M24 * num;
      matrix1.M31 = matrix.M31 * num;
      matrix1.M32 = matrix.M32 * num;
      matrix1.M33 = matrix.M33 * num;
      matrix1.M34 = matrix.M34 * num;
      matrix1.M41 = matrix.M41 * num;
      matrix1.M42 = matrix.M42 * num;
      matrix1.M43 = matrix.M43 * num;
      matrix1.M44 = matrix.M44 * num;
      return matrix1;
    }

    public static Matrix operator /(Matrix matrix1, Matrix matrix2)
    {
      Matrix matrix;
      matrix.M11 = matrix1.M11 / matrix2.M11;
      matrix.M12 = matrix1.M12 / matrix2.M12;
      matrix.M13 = matrix1.M13 / matrix2.M13;
      matrix.M14 = matrix1.M14 / matrix2.M14;
      matrix.M21 = matrix1.M21 / matrix2.M21;
      matrix.M22 = matrix1.M22 / matrix2.M22;
      matrix.M23 = matrix1.M23 / matrix2.M23;
      matrix.M24 = matrix1.M24 / matrix2.M24;
      matrix.M31 = matrix1.M31 / matrix2.M31;
      matrix.M32 = matrix1.M32 / matrix2.M32;
      matrix.M33 = matrix1.M33 / matrix2.M33;
      matrix.M34 = matrix1.M34 / matrix2.M34;
      matrix.M41 = matrix1.M41 / matrix2.M41;
      matrix.M42 = matrix1.M42 / matrix2.M42;
      matrix.M43 = matrix1.M43 / matrix2.M43;
      matrix.M44 = matrix1.M44 / matrix2.M44;
      return matrix;
    }

    public static Matrix operator /(Matrix matrix1, float divider)
    {
      float num = 1f / divider;
      Matrix matrix;
      matrix.M11 = matrix1.M11 * num;
      matrix.M12 = matrix1.M12 * num;
      matrix.M13 = matrix1.M13 * num;
      matrix.M14 = matrix1.M14 * num;
      matrix.M21 = matrix1.M21 * num;
      matrix.M22 = matrix1.M22 * num;
      matrix.M23 = matrix1.M23 * num;
      matrix.M24 = matrix1.M24 * num;
      matrix.M31 = matrix1.M31 * num;
      matrix.M32 = matrix1.M32 * num;
      matrix.M33 = matrix1.M33 * num;
      matrix.M34 = matrix1.M34 * num;
      matrix.M41 = matrix1.M41 * num;
      matrix.M42 = matrix1.M42 * num;
      matrix.M43 = matrix1.M43 * num;
      matrix.M44 = matrix1.M44 * num;
      return matrix;
    }

    private struct CanonicalBasis
    {
      public Vector3 Row0;
      public Vector3 Row1;
      public Vector3 Row2;
    }

    private struct VectorBasis
    {
      public unsafe Vector3* Element0;
      public unsafe Vector3* Element1;
      public unsafe Vector3* Element2;
    }
  }
}
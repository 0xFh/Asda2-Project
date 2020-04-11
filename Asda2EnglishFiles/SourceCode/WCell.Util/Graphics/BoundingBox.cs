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
        [FieldOffset(0)] public Vector3 Min;

        /// <summary>The upper-right bound of the box.</summary>
        [FieldOffset(12)] public Vector3 Max;

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
            this.Min = new Vector3(minX, minY, minZ);
            this.Max = new Vector3(maxX, maxY, maxZ);
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
            this.Min = new Vector3(minX, minY, minZ);
            this.Max = max;
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
            this.Min = min;
            this.Max = new Vector3(maxX, maxY, maxZ);
        }

        /// <summary>
        /// Creates a new <see cref="T:WCell.Util.Graphics.BoundingBox" /> with the given coordinates.
        /// </summary>
        /// <param name="min">lower-bound vector</param>
        /// <param name="max">upper-bound vector</param>
        public BoundingBox(Vector3 min, Vector3 max)
        {
            this.Min = min;
            this.Max = max;
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
            for (int index = 0; index < vectors.Length; ++index)
            {
                Vector3 vector = vectors[index];
                num1 = Math.Min(vector.X, num1);
                num4 = Math.Max(vector.X, num4);
                num2 = Math.Min(vector.Y, num2);
                num5 = Math.Max(vector.Y, num5);
                num3 = Math.Min(vector.Z, num3);
                num6 = Math.Max(vector.Z, num6);
            }

            this.Min = new Vector3(num1, num2, num3);
            this.Max = new Vector3(num4, num5, num6);
        }

        public BoundingBox(IEnumerable<Vector3> vectors)
        {
            float num1 = float.MaxValue;
            float num2 = float.MaxValue;
            float num3 = float.MaxValue;
            float num4 = float.MinValue;
            float num5 = float.MinValue;
            float num6 = float.MinValue;
            foreach (Vector3 vector in vectors)
            {
                num1 = Math.Min(vector.X, num1);
                num4 = Math.Max(vector.X, num4);
                num2 = Math.Min(vector.Y, num2);
                num5 = Math.Max(vector.Y, num5);
                num3 = Math.Min(vector.Z, num3);
                num6 = Math.Max(vector.Z, num6);
            }

            this.Min = new Vector3(num1, num2, num3);
            this.Max = new Vector3(num4, num5, num6);
        }

        public float Width
        {
            get { return this.Max.X - this.Min.X; }
        }

        public float Height
        {
            get { return this.Max.Y - this.Min.Y; }
        }

        /// <summary>
        /// Checks whether the current <see cref="T:WCell.Util.Graphics.BoundingBox" /> intersects with the given <see cref="T:WCell.Util.Graphics.BoundingBox" />.
        /// </summary>
        /// <param name="box">the <see cref="T:WCell.Util.Graphics.BoundingBox" /> to check for intersection</param>
        /// <returns>an enumeration value describing the type of intersection between the two boxes</returns>
        public IntersectionType Intersects(ref BoundingBox box)
        {
            if ((double) this.Max.X < (double) box.Min.X || (double) this.Min.X > (double) box.Max.X ||
                ((double) this.Max.Y < (double) box.Min.Y || (double) this.Min.Y > (double) box.Max.Y) ||
                ((double) this.Max.Z < (double) box.Min.Z || (double) this.Min.Z > (double) box.Max.Z))
                return IntersectionType.NoIntersection;
            return (double) this.Min.X <= (double) box.Min.X && (double) box.Max.X <= (double) this.Max.X &&
                   ((double) this.Min.Y <= (double) box.Min.Y && (double) box.Max.Y <= (double) this.Max.Y) &&
                   ((double) this.Min.Z <= (double) box.Min.Z && (double) box.Max.Z <= (double) this.Max.Z)
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
            Vector3 point = sphere.Center.Clamp(ref this.Min, ref this.Max);
            float num = sphere.Center.DistanceSquared(ref point);
            float radius = sphere.Radius;
            if ((double) num > (double) radius * (double) radius)
                return IntersectionType.NoIntersection;
            return (double) this.Min.X + (double) radius <= (double) sphere.Center.X &&
                   (double) sphere.Center.X <= (double) this.Max.X - (double) radius &&
                   ((double) this.Max.X - (double) this.Min.X > (double) radius &&
                    (double) this.Min.Y + (double) radius <= (double) sphere.Center.Y) &&
                   ((double) sphere.Center.Y <= (double) this.Max.Y - (double) radius &&
                    (double) this.Max.Y - (double) this.Min.Y > (double) radius &&
                    ((double) this.Min.Z + (double) radius <= (double) sphere.Center.Z &&
                     (double) sphere.Center.Z <= (double) this.Max.Z - (double) radius &&
                     (double) this.Max.X - (double) this.Min.X > (double) radius))
                ? IntersectionType.Contained
                : IntersectionType.Intersects;
        }

        public bool Intersects(BoundingBox box)
        {
            if ((double) this.Max.X < (double) box.Min.X || (double) this.Min.X > (double) box.Max.X ||
                ((double) this.Max.Y < (double) box.Min.Y || (double) this.Min.Y > (double) box.Max.Y))
                return false;
            return (double) this.Max.Z >= (double) box.Min.Z && (double) this.Min.Z <= (double) box.Max.Z;
        }

        /// <summary>
        /// Checks whether the <see cref="T:WCell.Util.Graphics.BoundingBox" /> contains the given <see cref="T:WCell.Util.Graphics.BoundingBox" />.
        /// </summary>
        /// <param name="box">the <see cref="T:WCell.Util.Graphics.BoundingBox" /> to check for containment.</param>
        /// <returns>true if the <see cref="T:WCell.Util.Graphics.BoundingBox" /> is contained; false otherwise</returns>
        public bool Contains(ref BoundingBox box)
        {
            return (double) box.Min.X > (double) this.Min.X && (double) box.Min.Y > (double) this.Min.Y &&
                   (double) box.Min.Z > (double) this.Min.Z &&
                   ((double) box.Max.X < (double) this.Max.X && (double) box.Max.Y < (double) this.Max.Y &&
                    (double) box.Max.Z < (double) this.Max.Z);
        }

        /// <summary>
        /// Checks whether the <see cref="T:WCell.Util.Graphics.BoundingBox" /> contains the given point.
        /// </summary>
        /// <param name="point">the point to check for containment.</param>
        /// <returns>true if the point is contained; false otherwise</returns>
        public bool Contains(ref Vector3 point)
        {
            return (double) this.Min.X <= (double) point.X && (double) point.X <= (double) this.Max.X &&
                   ((double) this.Min.Y <= (double) point.Y && (double) point.Y <= (double) this.Max.Y) &&
                   ((double) this.Min.Z <= (double) point.Z && (double) point.Z <= (double) this.Max.Z);
        }

        /// <summary>
        /// Checks whether the <see cref="T:WCell.Util.Graphics.BoundingBox" /> contains the given point.
        /// </summary>
        /// <param name="point">the point to check for containment.</param>
        /// <returns>true if the point is contained; false otherwise</returns>
        public bool Contains(ref Vector4 point)
        {
            return (double) this.Min.X <= (double) point.X && (double) point.X <= (double) this.Max.X &&
                   ((double) this.Min.Y <= (double) point.Y && (double) point.Y <= (double) this.Max.Y) &&
                   ((double) this.Min.Z <= (double) point.Z && (double) point.Z <= (double) this.Max.Z);
        }

        public ContainmentType Contains(BoundingSphere sphere)
        {
            Vector3 result1;
            Vector3.Clamp(ref sphere.Center, ref this.Min, ref this.Max, out result1);
            float result2;
            Vector3.DistanceSquared(ref sphere.Center, ref result1, out result2);
            float radius = sphere.Radius;
            if ((double) result2 > (double) radius * (double) radius)
                return ContainmentType.Disjoint;
            return (double) this.Min.X + (double) radius <= (double) sphere.Center.X &&
                   (double) sphere.Center.X <= (double) this.Max.X - (double) radius &&
                   ((double) this.Max.X - (double) this.Min.X > (double) radius &&
                    (double) this.Min.Y + (double) radius <= (double) sphere.Center.Y) &&
                   ((double) sphere.Center.Y <= (double) this.Max.Y - (double) radius &&
                    (double) this.Max.Y - (double) this.Min.Y > (double) radius &&
                    ((double) this.Min.Z + (double) radius <= (double) sphere.Center.Z &&
                     (double) sphere.Center.Z <= (double) this.Max.Z - (double) radius &&
                     (double) this.Max.X - (double) this.Min.X > (double) radius))
                ? ContainmentType.Contains
                : ContainmentType.Intersects;
        }

        /// <summary>Checks equality of two boxes.</summary>
        /// <param name="other">the other box to compare with</param>
        /// <returns>true if both boxes are equal; false otherwise</returns>
        public bool Equals(BoundingBox other)
        {
            return this.Min == other.Min && this.Max == other.Max;
        }

        /// <summary>Checks equality with another object.</summary>
        /// <param name="obj">the object to compare</param>
        /// <returns>true if the object is <see cref="T:WCell.Util.Graphics.BoundingBox" /> and is equal; false otherwise</returns>
        public override bool Equals(object obj)
        {
            return obj is BoundingBox && this.Equals((BoundingBox) obj);
        }

        public override int GetHashCode()
        {
            return this.Min.GetHashCode() + this.Max.GetHashCode();
        }

        public static bool operator ==(BoundingBox a, BoundingBox b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(BoundingBox a, BoundingBox b)
        {
            if (!(a.Min != b.Min))
                return a.Max != b.Max;
            return true;
        }

        public override string ToString()
        {
            return string.Format("(Min: {0}, Max: {1})", (object) this.Min, (object) this.Max);
        }

        public float? Intersects(Ray ray)
        {
            float val2_1 = 0.0f;
            float val2_2 = float.MaxValue;
            if ((double) Math.Abs(ray.Direction.X) < 9.99999997475243E-07)
            {
                if ((double) ray.Position.X < (double) this.Min.X || (double) ray.Position.X > (double) this.Max.X)
                    return new float?();
            }
            else
            {
                float num1 = 1f / ray.Direction.X;
                float val1_1 = (this.Min.X - ray.Position.X) * num1;
                float val1_2 = (this.Max.X - ray.Position.X) * num1;
                if ((double) val1_1 > (double) val1_2)
                {
                    float num2 = val1_1;
                    val1_1 = val1_2;
                    val1_2 = num2;
                }

                val2_1 = Math.Max(val1_1, val2_1);
                val2_2 = Math.Min(val1_2, val2_2);
                if ((double) val2_1 > (double) val2_2)
                    return new float?();
            }

            if ((double) Math.Abs(ray.Direction.Y) < 9.99999997475243E-07)
            {
                if ((double) ray.Position.Y < (double) this.Min.Y || (double) ray.Position.Y > (double) this.Max.Y)
                    return new float?();
            }
            else
            {
                float num1 = 1f / ray.Direction.Y;
                float val1_1 = (this.Min.Y - ray.Position.Y) * num1;
                float val1_2 = (this.Max.Y - ray.Position.Y) * num1;
                if ((double) val1_1 > (double) val1_2)
                {
                    float num2 = val1_1;
                    val1_1 = val1_2;
                    val1_2 = num2;
                }

                val2_1 = Math.Max(val1_1, val2_1);
                val2_2 = Math.Min(val1_2, val2_2);
                if ((double) val2_1 > (double) val2_2)
                    return new float?();
            }

            if ((double) Math.Abs(ray.Direction.Z) < 9.99999997475243E-07)
            {
                if ((double) ray.Position.Z < (double) this.Min.Z || (double) ray.Position.Z > (double) this.Max.Z)
                    return new float?();
            }
            else
            {
                float num1 = 1f / ray.Direction.Z;
                float val1_1 = (this.Min.Z - ray.Position.Z) * num1;
                float val1_2 = (this.Max.Z - ray.Position.Z) * num1;
                if ((double) val1_1 > (double) val1_2)
                {
                    float num2 = val1_1;
                    val1_1 = val1_2;
                    val1_2 = num2;
                }

                val2_1 = Math.Max(val1_1, val2_1);
                float num3 = Math.Min(val1_2, val2_2);
                if ((double) val2_1 > (double) num3)
                    return new float?();
            }

            return new float?(val2_1);
        }

        public void Intersects(ref Ray ray, out float? result)
        {
            result = new float?(0.0f);
            float val2_1 = 0.0f;
            float val2_2 = float.MaxValue;
            if ((double) Math.Abs(ray.Direction.X) < 9.99999997475243E-07)
            {
                if ((double) ray.Position.X < (double) this.Min.X || (double) ray.Position.X > (double) this.Max.X)
                    return;
            }
            else
            {
                float num1 = 1f / ray.Direction.X;
                float val1_1 = (this.Min.X - ray.Position.X) * num1;
                float val1_2 = (this.Max.X - ray.Position.X) * num1;
                if ((double) val1_1 > (double) val1_2)
                {
                    float num2 = val1_1;
                    val1_1 = val1_2;
                    val1_2 = num2;
                }

                val2_1 = Math.Max(val1_1, val2_1);
                val2_2 = Math.Min(val1_2, val2_2);
                if ((double) val2_1 > (double) val2_2)
                    return;
            }

            if ((double) Math.Abs(ray.Direction.Y) < 9.99999997475243E-07)
            {
                if ((double) ray.Position.Y < (double) this.Min.Y || (double) ray.Position.Y > (double) this.Max.Y)
                    return;
            }
            else
            {
                float num1 = 1f / ray.Direction.Y;
                float val1_1 = (this.Min.Y - ray.Position.Y) * num1;
                float val1_2 = (this.Max.Y - ray.Position.Y) * num1;
                if ((double) val1_1 > (double) val1_2)
                {
                    float num2 = val1_1;
                    val1_1 = val1_2;
                    val1_2 = num2;
                }

                val2_1 = Math.Max(val1_1, val2_1);
                val2_2 = Math.Min(val1_2, val2_2);
                if ((double) val2_1 > (double) val2_2)
                    return;
            }

            if ((double) Math.Abs(ray.Direction.Z) < 9.99999997475243E-07)
            {
                if ((double) ray.Position.Z < (double) this.Min.Z || (double) ray.Position.Z > (double) this.Max.Z)
                    return;
            }
            else
            {
                float num1 = 1f / ray.Direction.Z;
                float val1_1 = (this.Min.Z - ray.Position.Z) * num1;
                float val1_2 = (this.Max.Z - ray.Position.Z) * num1;
                if ((double) val1_1 > (double) val1_2)
                {
                    float num2 = val1_1;
                    val1_1 = val1_2;
                    val1_2 = num2;
                }

                val2_1 = Math.Max(val1_1, val2_1);
                float num3 = Math.Min(val1_2, val2_2);
                if ((double) val2_1 > (double) num3)
                    return;
            }

            result = new float?(val2_1);
        }

        public static BoundingBox Join(ref BoundingBox a, ref BoundingBox b)
        {
            return new BoundingBox(Vector3.Min(a.Min, b.Min), Vector3.Max(a.Max, b.Max));
        }
    }
}
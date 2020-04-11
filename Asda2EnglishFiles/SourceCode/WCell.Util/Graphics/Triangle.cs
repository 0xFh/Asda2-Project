using System.Collections.Generic;

namespace WCell.Util.Graphics
{
    public struct Triangle
    {
        public Vector3 Point1;
        public Vector3 Point2;
        public Vector3 Point3;

        public Triangle(Vector3 point1, Vector3 point2, Vector3 point3)
        {
            this.Point1 = point1;
            this.Point2 = point2;
            this.Point3 = point3;
        }

        public Vector3 Min
        {
            get { return Vector3.Min(Vector3.Min(this.Point1, this.Point2), this.Point3); }
        }

        public Vector3 Max
        {
            get { return Vector3.Max(Vector3.Max(this.Point1, this.Point2), this.Point3); }
        }

        public List<int> Indices
        {
            get { return new List<int>() {0, 1, 2}; }
        }

        public IEnumerable<Vector3> Vertices
        {
            get
            {
                yield return this.Point1;
                yield return this.Point2;
                yield return this.Point3;
            }
        }

        /// <summary>
        /// Computes the normal of this triangle *without normalizing it*
        /// </summary>
        public Vector3 CalcNormal()
        {
            return Vector3.Cross(this.Point3 - this.Point1, this.Point2 - this.Point1);
        }

        /// <summary>Computes the normalized normal of this triangle</summary>
        public Vector3 CalcNormalizedNormal()
        {
            Vector3 vector3 = Vector3.Cross(this.Point3 - this.Point1, this.Point2 - this.Point1);
            vector3.Normalize();
            return vector3;
        }
    }
}
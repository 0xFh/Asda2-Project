namespace WCell.Util.Graphics
{
    /// <summary>Orientated Bounding Box</summary>
    public class OBB
    {
        /// <summary>Vector3 center of this OBB</summary>
        public Vector3 Center;

        /// <summary>
        /// Bounds of this OBB.
        /// A single Vector 3 representing the distance from the center point to the edge of the box on all three axis as if it were an AABB.
        /// </summary>
        public Vector3 Extent;

        public Matrix InverseRotation;
        private BoundingBox bounds;

        /// <summary>The bounds of the Building in World Space</summary>
        public BoundingBox Bounds
        {
            get { return this.bounds; }
            set
            {
                this.Center = (this.bounds.Min + this.bounds.Max) / 2f;
                this.Extent = (this.bounds.Max - this.bounds.Min) / 2f;
                this.bounds = value;
            }
        }

        /// <summary>Constructor</summary>
        public OBB()
        {
        }

        public OBB(BoundingBox localBounds, Matrix rotation)
        {
            this.Bounds = localBounds;
            this.InverseRotation = Matrix.Invert(rotation);
        }

        /// <summary>Checks if a ray intersects the bounding box.</summary>
        /// <returns>null if no intersection, else the time of first contact.</returns>
        public float? Intersects(Ray ray)
        {
            return this.Bounds.Intersects(ray);
        }

        /// <summary>Checks if a ray intersects the bounding box.</summary>
        public void Intersects(ref Ray ray, out float? results)
        {
            this.Bounds.Intersects(ref ray, out results);
        }
    }
}
using WCell.Util.Graphics;

namespace WCell.Util
{
    /// <summary>Adds functonality to the BoundingBox class</summary>
    public static class BoxExtensions
    {
        /// <summary>Calculates the vector at the center of the box</summary>
        /// <returns>A Vector3 that points to the center of the BoundingBox.</returns>
        public static Vector3 Center(this BoundingBox box)
        {
            return (box.Min + box.Max) * 0.5f;
        }

        /// <summary>Returns Box.Max in Box coordinates</summary>
        public static Vector3 Extents(this BoundingBox box)
        {
            return box.Max - box.Center();
        }
    }
}
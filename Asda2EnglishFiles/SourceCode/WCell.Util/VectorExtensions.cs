using WCell.Util.Graphics;

namespace WCell.Util
{
    public static class VectorExtensions
    {
        public static float[] ToFloatArray(this Vector3 vector)
        {
            return new float[3] {vector.X, vector.Y, vector.Z};
        }
    }
}
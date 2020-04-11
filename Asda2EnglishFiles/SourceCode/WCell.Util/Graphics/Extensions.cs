namespace WCell.Util.Graphics
{
    public static class Extensions
    {
        public static bool HasAnyFlag(this IntersectionType flags, IntersectionType otherFlags)
        {
            return (flags & otherFlags) != IntersectionType.NoIntersection;
        }
    }
}
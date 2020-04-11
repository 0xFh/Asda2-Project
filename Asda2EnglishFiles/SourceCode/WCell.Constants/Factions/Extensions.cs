namespace WCell.Constants.Factions
{
    public static class Extensions
    {
        public static bool HasAnyFlag(this FactionGroupMask flags, FactionGroupMask otherFlags)
        {
            return (flags & otherFlags) != FactionGroupMask.None;
        }

        public static bool HasAnyFlag(this FactionTemplateFlags flags, FactionTemplateFlags otherFlags)
        {
            return (flags & otherFlags) != FactionTemplateFlags.None;
        }

        public static bool HasAnyFlag(this FactionFlags flags, FactionFlags otherFlags)
        {
            return (flags & otherFlags) != FactionFlags.None;
        }
    }
}
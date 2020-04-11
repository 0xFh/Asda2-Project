namespace WCell.Constants.Relations
{
    public static class Extensions
    {
        public static bool HasAnyFlag(this RelationTypeFlag flags, RelationTypeFlag otherFlags)
        {
            return (flags & otherFlags) != RelationTypeFlag.None;
        }
    }
}
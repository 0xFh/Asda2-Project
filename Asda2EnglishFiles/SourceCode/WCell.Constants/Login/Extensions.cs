namespace WCell.Constants.Login
{
    public static class Extensions
    {
        public static bool HasAnyFlag(this RealmServerType flags, RealmServerType otherFlags)
        {
            return (flags & otherFlags) != RealmServerType.Normal;
        }

        public static bool HasAnyFlag(this CharEnumFlags flags, CharEnumFlags otherFlags)
        {
            return (flags & otherFlags) != CharEnumFlags.None;
        }
    }
}
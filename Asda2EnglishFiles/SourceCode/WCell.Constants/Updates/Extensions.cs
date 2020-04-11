namespace WCell.Constants.Updates
{
    public static class Extensions
    {
        public static bool HasAnyFlag(this ObjectTypes flags, ObjectTypes otherFlags)
        {
            return (flags & otherFlags) != ObjectTypes.None;
        }

        public static bool HasAnyFlag(this ObjectTypeCustom flags, ObjectTypeCustom otherFlags)
        {
            return (flags & otherFlags) != ObjectTypeCustom.None;
        }

        public static bool HasAnyFlag(this UpdateFieldFlags flags, UpdateFieldFlags otherFlags)
        {
            return (flags & otherFlags) != UpdateFieldFlags.None;
        }

        public static bool HasAnyFlag(this UpdateFlags flags, UpdateFlags otherFlags)
        {
            return (flags & otherFlags) != (UpdateFlags) 0;
        }
    }
}
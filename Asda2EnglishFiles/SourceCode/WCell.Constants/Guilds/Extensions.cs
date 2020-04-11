namespace WCell.Constants.Guilds
{
    public static class Extensions
    {
        public static bool HasAnyFlag(this GuildPrivileges flags, GuildPrivileges otherFlags)
        {
            return (flags & otherFlags) != GuildPrivileges.None;
        }

        public static bool HasAnyFlag(this GuildBankTabPrivileges flags, GuildBankTabPrivileges otherFlags)
        {
            return (flags & otherFlags) != GuildBankTabPrivileges.None;
        }
    }
}
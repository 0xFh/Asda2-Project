namespace WCell.Constants
{
    public enum CalendarErrors : uint
    {
        GuildEventsExceeded = 1,
        EventsExceeded = 2,
        SelfInvitesExceeded = 3,
        OtherInvitesExceeded = 4,
        NoPermissions = 5,
        EventInvalid = 6,
        NotInvited = 7,
        InternalError = 8,
        PlayerNotInGuild = 9,
        AlreadyInvitedToEvent = 10, // 0x0000000A
        PlayerNotFound = 11, // 0x0000000B
        NotAllied = 12, // 0x0000000C
        PlayerIsIgnoringYou = 13, // 0x0000000D
        InvitesExceeded = 14, // 0x0000000E
        InvalidDate = 16, // 0x00000010
        InvalidTime = 17, // 0x00000011
        NeedsTitle = 19, // 0x00000013
        EventPassed = 20, // 0x00000014
        EventLocked = 21, // 0x00000015
        DeleteCreatorFailed = 22, // 0x00000016
        SystemDisabled = 24, // 0x00000018
        RestrictedAccount = 25, // 0x00000019
        ArenaEventsExceeded = 26, // 0x0000001A
        RestrictedLevel = 27, // 0x0000001B
        UserSquelched = 28, // 0x0000001C
        NoInvite = 29, // 0x0000001D
        WrongServer = 36, // 0x00000024
        InviteWrongServer = 37, // 0x00000025
        NoGuildInvites = 38, // 0x00000026
        InvalidSignup = 39, // 0x00000027
        NoModerator = 40, // 0x00000028
    }
}
namespace WCell.Constants.Chat
{
    public enum ChannelNotification : byte
    {
        PlayerJoined = 0,
        PlayerLeft = 1,
        YouJoined = 2,
        YouLeft = 3,
        WrongPassword = 4,
        NotOnChannel = 5,
        NotModerator = 6,
        PasswordChanged = 7,
        OwnerChanged = 8,
        PlayerNotOnChannel = 9,
        NotOwner = 10, // 0x0A
        CurrentOwner = 11, // 0x0B
        Moderator = 12, // 0x0C
        Mute = 12, // 0x0C
        Announcing = 13, // 0x0D
        NotAnnouncing = 14, // 0x0E
        Moderated = 15, // 0x0F
        NotModerated = 16, // 0x10
        YouAreMuted = 17, // 0x11
        Kicked = 18, // 0x12
        YouAreBanned = 19, // 0x13
        Banned = 20, // 0x14
        Unbanned = 21, // 0x15
        AlreadyOnChannel = 23, // 0x17
        BeenInvitedToChannel = 24, // 0x18
        InviteWrongFaction = 25, // 0x19
        WrongAlliance = 26, // 0x1A
        InvalidChannelName = 27, // 0x1B
        ChannelIsNotModerated = 28, // 0x1C
        HaveInvitedToChannel = 29, // 0x1D
        CannotInviteBannedPlayer = 30, // 0x1E
        ChatThrottledNotice = 31, // 0x1F
        NotInCorrectAreaForChannel = 32, // 0x20
        NotInLFGQueue = 33, // 0x21
        VoiceEnabled = 34, // 0x22
        VoiceDisabled = 35, // 0x23
    }
}
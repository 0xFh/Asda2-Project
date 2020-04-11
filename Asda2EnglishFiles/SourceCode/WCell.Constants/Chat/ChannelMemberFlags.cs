using System;

namespace WCell.Constants.Chat
{
    /// <summary>Per player</summary>
    [Flags]
    public enum ChannelMemberFlags
    {
        None = 0,
        Owner = 1,
        Moderator = 2,
        Voiced = 4,
        Muted = 8,
        Custom = 16, // 0x00000010
        VoiceMuted = 32, // 0x00000020
    }
}
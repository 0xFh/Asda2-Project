using System;

namespace WCell.Constants.Chat
{
    /// <summary>
    /// Same as <see cref="T:WCell.Constants.Chat.ChatChannelFlagsClient" /> but according to how its read from DBC files.
    /// The client actually expects the format defined in <see cref="T:WCell.Constants.Chat.ChatChannelFlagsClient" />.
    /// </summary>
    [Flags]
    public enum ChatChannelFlags : uint
    {
        None = 0,
        AutoJoin = 1,
        ZoneSpecific = 2,
        Global = 4,
        Trade = 8,
        CityOnly = 32, // 0x00000020
        Defense = 65536, // 0x00010000
        RequiresUnguilded = 131072, // 0x00020000
        LookingForGroup = 262144, // 0x00040000
    }
}
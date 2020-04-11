namespace WCell.Constants.Guilds
{
    /// <summary>Results of a guild command request</summary>
    public enum GuildResult
    {
        SUCCESS = 0,
        INTERNAL = 1,
        ALREADY_IN_GUILD = 2,
        ALREADY_IN_GUILD_S = 3,
        INVITED_TO_GUILD = 4,
        ALREADY_INVITED_TO_GUILD = 5,
        NAME_INVALID = 6,
        NAME_EXISTS = 7,
        LEADER_LEAVE = 8,
        PERMISSIONS = 8,
        PLAYER_NOT_IN_GUILD = 9,
        PLAYER_NOT_IN_GUILD_S = 10, // 0x0000000A
        PLAYER_NOT_FOUND = 11, // 0x0000000B
        NOT_ALLIED = 12, // 0x0000000C
        PlayerRankTooHigh = 13, // 0x0000000D
        PLAYER_ALREADY_NOOB = 14, // 0x0000000E
        TEMPORARY_ERROR = 17, // 0x00000011
        GUILD_RANK_IN_USE = 18, // 0x00000012
        PLAYER_IGNORING_YOU = 19, // 0x00000013
    }
}
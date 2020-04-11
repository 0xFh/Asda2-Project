namespace WCell.Constants.ArenaTeams
{
    public enum ArenaTeamResult
    {
        INTERNAL = 1,
        ALREADY_IN_ARENA_TEAM = 2,
        ALREADY_IN_ARENA_TEAM_S = 3,
        INVITED_TO_ARENA_TEAM = 4,
        ALREADY_INVITED_TO_ARENA_TEAM_S = 5,
        NAME_INVALID = 6,
        NAME_EXISTS = 7,
        LEADER_LEAVE = 8,
        PERMISSIONS = 8,
        PLAYER_NOT_IN_TEAM = 9,
        PLAYER_NOT_IN_TEAM_SS = 10, // 0x0000000A
        PLAYER_NOT_FOUND = 11, // 0x0000000B
        NOT_ALLIED = 12, // 0x0000000C
        IGNORING_YOU = 19, // 0x00000013
        TARGET_TOO_LOW = 21, // 0x00000015
        TARGET_TOO_HIGH = 22, // 0x00000016
        TOO_MANY_MEMBERS = 23, // 0x00000017
        NOT_FOUND = 27, // 0x0000001B
        LOCKED = 30, // 0x0000001E
    }
}
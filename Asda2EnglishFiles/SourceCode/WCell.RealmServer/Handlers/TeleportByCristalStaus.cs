namespace WCell.RealmServer.Handlers
{
    public enum TeleportByCristalStaus
    {
        ServerDown = 0,
        Ok = 1,
        ServerFull = 2,
        NotMove = 8,
        NotGold = 12, // 0x0000000C
        NotRegisterOnWar = 13, // 0x0000000D
        TakingPlace = 14, // 0x0000000E
        OnceDayOnWar = 15, // 0x0000000F
        SelectGameToEnter = 16, // 0x00000010
        CandidateNotEnterOnWar = 17, // 0x00000011
        FactionIsStrange = 18, // 0x00000012
        BattlefieldMapIsStrange = 19, // 0x00000013
        BattlefieldInfoIsStrange = 20, // 0x00000014
        DueWarPlayers = 21, // 0x00000015
        CantEnterWarCauseLowPlayersInOtherFaction = 22, // 0x00000016
        WaveIsEnding = 31, // 0x0000001F
        NotEnterUntilPlayers = 32, // 0x00000020
        RejoinNot = 33, // 0x00000021
        NotRegisterWave = 34, // 0x00000022
        NoWaveInfo = 35, // 0x00000023
        NotGuildInfo = 36, // 0x00000024
    }
}
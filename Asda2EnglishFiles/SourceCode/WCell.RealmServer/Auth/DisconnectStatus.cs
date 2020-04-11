namespace WCell.RealmServer.Auth
{
    public enum DisconnectStatus
    {
        EmptyValue = 20, // 0x00000014
        CountryBlocked = 21, // 0x00000015
        FraudUserIp = 22, // 0x00000016
        InternalError = 24, // 0x00000018
        UnlachingHasValue = 25, // 0x00000019
        NoUserInfo = 26, // 0x0000001A
        WrongPassword = 27, // 0x0000001B
        ServerOnMaintance = 28, // 0x0000001C
        NotCbtUser = 29, // 0x0000001D
        NotServiceArea = 30, // 0x0000001E
        ExceedMaxNuberOfConnectionToThisIp = 31, // 0x0000001F
        U1 = 32, // 0x00000020
        WelcomeBack = 33, // 0x00000021
        U2 = 40, // 0x00000028
        AccountBanned = 103, // 0x00000067
        DeletedAccount = 104, // 0x00000068
        U3 = 105, // 0x00000069
        DataDoesnotExsist = 106, // 0x0000006A
        Timeout = 107, // 0x0000006B
    }
}
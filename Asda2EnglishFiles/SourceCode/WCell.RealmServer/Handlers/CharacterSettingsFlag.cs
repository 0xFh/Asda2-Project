namespace WCell.RealmServer.Handlers
{
    public enum CharacterSettingsFlag
    {
        EnableWishpers = 5,
        EnableSoulmateRequest = 7,
        EnableFriendRequest = 8,
        EnablePartyRequest = 9,
        EnableGuildRequest = 10, // 0x0000000A
        EnableGeneralTradeRequest = 11, // 0x0000000B
        EnableGearTradeRequest = 12, // 0x0000000C
        DisplayMonstrHelath = 13, // 0x0000000D
        ShowSelfNameAndHealth = 14, // 0x0000000E
        DisplayHemlet = 15, // 0x0000000F
    }
}
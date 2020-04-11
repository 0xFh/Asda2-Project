namespace WCell.Constants
{
    /// <summary>Realm Auth Proof Error Codes</summary>
    public enum AccountStatus : byte
    {
        Success = 1,
        WrongLoginOrPass = 2,
        AccountInUse = 4,
        CloseClient = 5,
        AccountBanned = 11, // 0x0B
    }
}
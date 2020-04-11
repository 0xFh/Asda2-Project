using System;

namespace WCell.Constants.Guilds
{
    [Flags]
    public enum GuildBankTabPrivileges : uint
    {
        None = 0,
        ViewTab = 1,
        PutItem = 2,
        UpdateText = 4,
        DepositItem = PutItem | ViewTab, // 0x00000003
        Full = 255, // 0x000000FF
    }
}
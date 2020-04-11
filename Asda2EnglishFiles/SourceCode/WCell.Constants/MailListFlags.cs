using System;

namespace WCell.Constants
{
    [Flags]
    public enum MailListFlags : uint
    {
        NotRead = 0,
        Read = 1,
        Delete = 2,
        Auction = 4,
        Return = 16, // 0x00000010
    }
}
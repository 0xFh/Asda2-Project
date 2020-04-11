namespace WCell.Constants
{
    public enum CalendarEventFlags
    {
        Player = 1,
        System = 4,
        Holiday = 8,
        Locked = 16, // 0x00000010
        AutoApprove = 32, // 0x00000020
        GuildAnnouncement = 64, // 0x00000040
        RaidLockout = 128, // 0x00000080
        RaidReset = 512, // 0x00000200
        GuildEvent = 1024, // 0x00000400
    }
}
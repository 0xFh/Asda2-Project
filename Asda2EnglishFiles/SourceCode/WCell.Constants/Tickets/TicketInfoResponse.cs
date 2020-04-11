namespace WCell.Constants.Tickets
{
    public enum TicketInfoResponse : uint
    {
        Fail = 1,
        Saved = 2,
        Pending = 6,
        Deleted = 9,
        NoTicket = 10, // 0x0000000A
    }
}
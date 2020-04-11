namespace WCell.Constants
{
    public enum MailError
    {
        OK = 0,
        BAG_FULL = 1,
        CANNOT_SEND_TO_SELF = 2,
        NOT_ENOUGH_MONEY = 3,
        RECIPIENT_NOT_FOUND = 4,
        NOT_YOUR_ALLIANCE = 5,
        INTERNAL_ERROR = 6,
        DISABLED_FOR_TRIAL_ACCOUNT = 14, // 0x0000000E
        RECIPIENT_CAP_REACHED = 15, // 0x0000000F
        CANT_SEND_WRAPPED_COD = 16, // 0x00000010
        MAIL_AND_CHAT_SUSPENDED = 17, // 0x00000011
    }
}
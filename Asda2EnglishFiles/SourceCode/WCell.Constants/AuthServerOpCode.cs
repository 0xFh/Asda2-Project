namespace WCell.Constants
{
    /// <summary>Auth Packet Opcodes</summary>
    public enum AuthServerOpCode : byte
    {
        AUTH_LOGON_CHALLENGE = 0,
        AUTH_LOGON_PROOF = 1,
        AUTH_RECONNECT_CHALLENGE = 2,
        AUTH_RECONNECT_PROOF = 3,
        REALM_LIST = 16, // 0x10
        XFER_INITIATE = 48, // 0x30
        XFER_DATA = 49, // 0x31
        XFER_ACCEPT = 50, // 0x32
        XFER_RESUME = 51, // 0x33
        XFER_CANCEL = 52, // 0x34
        Maximum = 100, // 0x64
        Unknown = 255, // 0xFF
    }
}
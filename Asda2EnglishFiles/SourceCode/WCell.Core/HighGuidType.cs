namespace WCell.Core
{
    public enum HighGuidType : byte
    {
        NoEntry = 0,
        GameObject = 16, // 0x10
        Transport = 32, // 0x20
        Unit = 48, // 0x30
        Pet = 64, // 0x40
        Vehicle = 80, // 0x50
        MapObjectTransport = 192, // 0xC0
    }
}
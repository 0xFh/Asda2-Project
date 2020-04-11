using System;

namespace WCell.Constants.Items
{
    [Flags]
    public enum SocketColor : uint
    {
        None = 0,
        Meta = 1,
        Red = 2,
        Yellow = 4,
        Blue = 8,
    }
}
using System;

namespace WCell.Util.Graphics
{
    [Flags]
    internal enum OutFlags : byte
    {
        None = 0,
        Top = 1,
        Bottom = 2,
        Left = 4,
        Right = 8,
    }
}
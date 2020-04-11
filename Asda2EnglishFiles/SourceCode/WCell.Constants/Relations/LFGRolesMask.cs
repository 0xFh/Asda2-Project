using System;

namespace WCell.Constants.Relations
{
    [Flags]
    public enum LFGRolesMask : byte
    {
        None = 0,
        Leader = 1,
        Tank = 2,
        Healer = 4,
        Damage = 8,
    }
}
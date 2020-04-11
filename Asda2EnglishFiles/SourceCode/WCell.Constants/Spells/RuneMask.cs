using System;

namespace WCell.Constants.Spells
{
    [Flags]
    public enum RuneMask : byte
    {
        Blood = 1,
        Unholy = 2,
        Frost = 4,
        Death = 8,
        End = Death | Blood, // 0x09
    }
}
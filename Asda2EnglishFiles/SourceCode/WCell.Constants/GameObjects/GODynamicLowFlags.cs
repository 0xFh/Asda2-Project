using System;

namespace WCell.Constants.GameObjects
{
    [Flags]
    public enum GODynamicLowFlags : ushort
    {
        None = 0,
        Clickable = 1,
        Animated = 2,
        NotClickable = 4,
        Sparkle = 8,
    }
}
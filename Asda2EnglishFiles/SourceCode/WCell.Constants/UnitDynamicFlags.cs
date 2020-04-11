using System;

namespace WCell.Constants
{
    [Flags]
    public enum UnitDynamicFlags
    {
        None = 0,
        Lootable = 1,
        TrackUnit = 2,
        TaggedByOther = 4,
        TaggedByMe = 8,
        SpecialInfo = 16, // 0x00000010
        Dead = 32, // 0x00000020
        ReferAFriendLinked = 64, // 0x00000040
        IsTappedByAllThreatList = 128, // 0x00000080
    }
}
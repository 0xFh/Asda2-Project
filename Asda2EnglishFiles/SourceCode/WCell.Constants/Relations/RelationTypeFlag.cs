using System;

namespace WCell.Constants.Relations
{
    [Flags]
    public enum RelationTypeFlag : uint
    {
        None = 0,
        Friend = 1,
        Ignore = 2,
        Muted = 4,
        RecruitAFriend = 8,
    }
}
using System;

namespace WCell.Constants
{
    [Flags]
    public enum GroupMemberFlags : byte
    {
        Normal = 0,
        Assistant = 1,
        MainTank = 2,
        MainAssistant = 4,
    }
}
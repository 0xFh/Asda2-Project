using System;

namespace WCell.Constants
{
    [Flags]
    public enum CharacterStatus : byte
    {
        OFFLINE = 0,
        ONLINE = 1,
        AFK = 2,
        DND = 4,
    }
}
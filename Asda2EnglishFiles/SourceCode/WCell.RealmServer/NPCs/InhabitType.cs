using System;

namespace WCell.RealmServer.NPCs
{
    [Flags]
    public enum InhabitType
    {
        Ground = 1,
        Water = 2,
        Amphibious = Water | Ground, // 0x00000003
        Air = 4,
        Anywhere = Air | Amphibious, // 0x00000007
    }
}
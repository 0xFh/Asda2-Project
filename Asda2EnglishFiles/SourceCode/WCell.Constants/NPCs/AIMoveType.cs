using System;

namespace WCell.Constants.NPCs
{
    [Flags]
    public enum AIMoveType
    {
        Walk = 0,
        Run = 1,
        Sprint = 2,
        Fly = Sprint | Run, // 0x00000003
    }
}
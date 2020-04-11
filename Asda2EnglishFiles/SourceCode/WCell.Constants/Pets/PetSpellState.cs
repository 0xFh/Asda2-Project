using System;

namespace WCell.Constants.Pets
{
    [Flags]
    public enum PetSpellState : ushort
    {
        None = 0,
        Passive = 1,
        AutoCast = 64, // 0x0040
        Castable = 129, // 0x0081
        Enabled = Castable | AutoCast, // 0x00C1
    }
}
using System;

namespace WCell.Constants.Pets
{
    [Flags]
    public enum PetFlags : ushort
    {
        None = 0,
        Stabled = 1,
    }
}
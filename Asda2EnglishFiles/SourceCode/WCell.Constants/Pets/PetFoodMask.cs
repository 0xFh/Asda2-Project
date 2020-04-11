using System;

namespace WCell.Constants.Pets
{
    [Flags]
    public enum PetFoodMask : uint
    {
        Meat = 1,
        Fish = 2,
        Cheese = 4,
        Bread = 8,
        Fungus = 16, // 0x00000010
        Fruit = 32, // 0x00000020
        RawMeat = 64, // 0x00000040
        RawFish = 128, // 0x00000080
    }
}
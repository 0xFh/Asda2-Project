using System;

namespace WCell.Constants.Items
{
    [Flags]
    public enum ItemBagFamilyMask
    {
        None = 0,
        Arrows = 1,
        Bullets = 2,
        SoulShards = 4,
        Leatherworking = 8,
        Unused = 16, // 0x00000010
        Herbs = 32, // 0x00000020
        Enchanting = 64, // 0x00000040
        Engineering = 128, // 0x00000080
        Keys = 256, // 0x00000100
        Gems = 512, // 0x00000200
        Mining = 1024, // 0x00000400
        Soulbound = 2048, // 0x00000800
        VanityPets = 4096, // 0x00001000
        CurrencyTokens = 8192, // 0x00002000
        QuestItems = 16384, // 0x00004000
    }
}
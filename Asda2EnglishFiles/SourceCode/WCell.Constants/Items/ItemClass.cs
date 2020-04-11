namespace WCell.Constants.Items
{
    public enum ItemClass
    {
        Consumable = 0,
        Container = 1,
        Weapon = 2,
        Jewelry = 3,
        Armor = 4,
        Reagent = 5,
        Projectile = 6,
        TradeGoods = 7,
        Generic = 8,
        Recipe = 9,
        Money = 10, // 0x0000000A
        Quiver = 11, // 0x0000000B
        Quest = 12, // 0x0000000C
        Key = 13, // 0x0000000D
        Permanent = 14, // 0x0000000E
        Miscellaneous = 15, // 0x0000000F
        Glyph = 16, // 0x00000010
        End = 17, // 0x00000011
        None = 255, // 0x000000FF
    }
}
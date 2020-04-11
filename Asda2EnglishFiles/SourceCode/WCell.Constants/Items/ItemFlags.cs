using System;

namespace WCell.Constants.Items
{
    /// <summary>Used in the ITEMFLAGS updatefield</summary>
    [Flags]
    public enum ItemFlags : uint
    {
        None = 0,
        Soulbound = 1,
        Conjured = 2,
        Openable = 4,
        GiftWrapped = 8,
        Totem = 32, // 0x00000020
        TriggersSpell = 64, // 0x00000040
        NoEquipCooldown = 128, // 0x00000080
        Wand = 256, // 0x00000100
        Usable = TriggersSpell, // 0x00000040
        WrappingPaper = 512, // 0x00000200
        Producer = 1024, // 0x00000400
        MultiLoot = 2048, // 0x00000800
        BriefSpellEffect = 4096, // 0x00001000
        Refundable = BriefSpellEffect, // 0x00001000
        Charter = 8192, // 0x00002000
        Refundable2 = 32768, // 0x00008000
        Readable = 16384, // 0x00004000
        PVPItem = Refundable2, // 0x00008000
        Expires = 65536, // 0x00010000
        Prospectable = 262144, // 0x00040000
        UniqueEquipped = 524288, // 0x00080000
        ThrownWeapon = 4194304, // 0x00400000
        AccountBound = 134217728, // 0x08000000
        EnchantScroll = 268435456, // 0x10000000
        Millable = 536870912, // 0x20000000
    }
}
using System;

namespace WCell.Constants.Spells
{
    [Flags]
    public enum SpellTargetFlags : uint
    {
        Self = 0,
        SpellTargetFlag_Dynamic_0x1 = 1,
        Unit = 2,
        SpellTargetFlag_Dynamic_0x4 = 4,
        SpellTargetFlag_Dynamic_0x8 = 8,
        Item = 16, // 0x00000010
        SourceLocation = 32, // 0x00000020
        DestinationLocation = 64, // 0x00000040
        UnkObject_0x80 = 128, // 0x00000080
        UnkUnit_0x100 = 256, // 0x00000100
        PvPCorpse = 512, // 0x00000200
        UnitCorpse = 1024, // 0x00000400
        GameObject = 2048, // 0x00000800
        TradeItem = 4096, // 0x00001000
        String = 8192, // 0x00002000
        OpenObject = 16384, // 0x00004000
        Corpse = 32768, // 0x00008000
        SpellTargetFlag_Dynamic_0x10000 = 65536, // 0x00010000
        Glyph = 131072, // 0x00020000
        Flag_0x200000 = 2097152, // 0x00200000
        WorldObject = SpellTargetFlag_Dynamic_0x10000 | Corpse | GameObject | PvPCorpse | Unit, // 0x00018A02
        AnyItem = TradeItem | Item, // 0x00001010
    }
}
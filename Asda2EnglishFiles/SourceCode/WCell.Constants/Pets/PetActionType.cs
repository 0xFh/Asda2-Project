using System;

namespace WCell.Constants.Pets
{
    [Flags]
    public enum PetActionType : byte
    {
        CastSpell = 1,
        CastSpell2 = 8,
        CastSpell3 = CastSpell2 | CastSpell, // 0x09
        CastSpell4 = 10, // 0x0A
        CastSpell5 = CastSpell4 | CastSpell, // 0x0B
        CastSpell6 = 12, // 0x0C
        CastSpell7 = CastSpell6 | CastSpell, // 0x0D
        CastSpell8 = 14, // 0x0E
        CastSpell9 = CastSpell8 | CastSpell, // 0x0F
        CastSpell10 = 16, // 0x10
        CastSpell11 = CastSpell10 | CastSpell, // 0x11
        SetMode = 6,
        SetAction = SetMode | CastSpell, // 0x07
        IsAutoCastEnabled = 64, // 0x40
        IsAutoCastAllowed = 128, // 0x80
        DefaultSpellSetting = IsAutoCastAllowed | IsAutoCastEnabled | CastSpell, // 0xC1
    }
}
using System;

namespace WCell.Constants.Spells
{
    [Flags]
    public enum SpellAttributes : uint
    {
        None = 0,
        Attr_0_0x1 = 1,
        Ranged = 2,
        OnNextMelee = 4,
        Unused_AttrFlag0x8 = 8,
        IsAbility = 16, // 0x00000010
        IsTradeSkill = 32, // 0x00000020
        Passive = 64, // 0x00000040
        InvisibleAura = 128, // 0x00000080
        Attr_8_0x100 = 256, // 0x00000100
        TempWeaponEnchant = 512, // 0x00000200
        OnNextMelee_2 = 1024, // 0x00000400
        Attr_11_0x800 = 2048, // 0x00000800
        OnlyUsableInDaytime = 4096, // 0x00001000
        OnlyUsableAtNight = 8192, // 0x00002000
        OnlyUsableIndoors = 16384, // 0x00004000
        OnlyUsableOutdoors = 32768, // 0x00008000
        NotWhileShapeshifted = 65536, // 0x00010000
        RequiresStealth = 131072, // 0x00020000
        Attr_18_0x40000 = 262144, // 0x00040000
        ScaleDamageWithCasterLevel = 524288, // 0x00080000
        StopsAutoAttack = 1048576, // 0x00100000
        CannotDodgeBlockParry = 2097152, // 0x00200000
        Attr_22_0x400000 = 4194304, // 0x00400000
        CastableWhileDead = 8388608, // 0x00800000
        CastableWhileMounted = 16777216, // 0x01000000
        StartCooldownAfterEffectFade = 33554432, // 0x02000000
        Attr_26_0x4000000 = 67108864, // 0x04000000
        CastableWhileSitting = 134217728, // 0x08000000
        CannotBeCastInCombat = 268435456, // 0x10000000
        UnaffectedByInvulnerability = 536870912, // 0x20000000
        MovementImpairing = 1073741824, // 0x40000000
        CannotRemove = 2147483648, // 0x80000000
    }
}
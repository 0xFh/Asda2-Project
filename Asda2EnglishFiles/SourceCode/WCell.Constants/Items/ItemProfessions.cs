using WCell.Constants.Skills;

namespace WCell.Constants.Items
{
    public static class ItemProfessions
    {
        public static readonly ItemSubClassMask[] ArmorProfessionSubClasses = new ItemSubClassMask[789];
        public static readonly ItemSubClassMask[] WeaponProfessionSubClasses = new ItemSubClassMask[789];

        public static readonly SkillId[] WeaponSubClassProfessions = new SkillId[21]
        {
            SkillId.Axes,
            SkillId.TwoHandedAxes,
            SkillId.Bows,
            SkillId.Guns,
            SkillId.Maces,
            SkillId.TwoHandedMaces,
            SkillId.Polearms,
            SkillId.Swords,
            SkillId.TwoHandedSwords,
            SkillId.None,
            SkillId.Staves,
            SkillId.None,
            SkillId.None,
            SkillId.FistWeapons,
            SkillId.None,
            SkillId.Daggers,
            SkillId.Thrown,
            SkillId.None,
            SkillId.Crossbows,
            SkillId.Wands,
            SkillId.Fishing
        };

        public static readonly SkillId[] ArmorSubClassProfessions = new SkillId[7]
        {
            SkillId.None,
            SkillId.Cloth,
            SkillId.Leather,
            SkillId.Mail,
            SkillId.PlateMail,
            SkillId.None,
            SkillId.Shield
        };

        static ItemProfessions()
        {
            ItemProfessions.WeaponProfessionSubClasses[44] = ItemSubClassMask.WeaponAxe;
            ItemProfessions.WeaponProfessionSubClasses[45] = ItemSubClassMask.WeaponBow;
            ItemProfessions.WeaponProfessionSubClasses[226] = ItemSubClassMask.WeaponCrossbow;
            ItemProfessions.WeaponProfessionSubClasses[173] = ItemSubClassMask.WeaponDagger;
            ItemProfessions.WeaponProfessionSubClasses[356] = ItemSubClassMask.WeaponFishingPole;
            ItemProfessions.WeaponProfessionSubClasses[162] = ItemSubClassMask.WeaponFist;
            ItemProfessions.WeaponProfessionSubClasses[46] = ItemSubClassMask.WeaponGun;
            ItemProfessions.WeaponProfessionSubClasses[43] = ItemSubClassMask.WeaponOneHandSword;
            ItemProfessions.WeaponProfessionSubClasses[229] = ItemSubClassMask.WeaponPolearm;
            ItemProfessions.WeaponProfessionSubClasses[136] = ItemSubClassMask.WeaponStaff;
            ItemProfessions.WeaponProfessionSubClasses[176] = ItemSubClassMask.WeaponThrown;
            ItemProfessions.WeaponProfessionSubClasses[172] = ItemSubClassMask.WeaponTwoHandAxe;
            ItemProfessions.WeaponProfessionSubClasses[160] = ItemSubClassMask.WeaponTwoHandMace;
            ItemProfessions.WeaponProfessionSubClasses[55] = ItemSubClassMask.WeaponTwoHandSword;
            ItemProfessions.WeaponProfessionSubClasses[228] = ItemSubClassMask.WeaponWand;
            ItemProfessions.ArmorProfessionSubClasses[415] = ItemSubClassMask.WeaponTwoHandAxe;
            ItemProfessions.ArmorProfessionSubClasses[415] = ItemSubClassMask.WeaponTwoHandAxe;
            ItemProfessions.ArmorProfessionSubClasses[414] = ItemSubClassMask.WeaponBow;
            ItemProfessions.ArmorProfessionSubClasses[413] = ItemSubClassMask.WeaponGun;
            ItemProfessions.ArmorProfessionSubClasses[293] = ItemSubClassMask.WeaponPolearm;
            ItemProfessions.ArmorProfessionSubClasses[433] = ItemSubClassMask.Shield;
        }
    }
}
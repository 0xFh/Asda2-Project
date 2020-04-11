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
      WeaponProfessionSubClasses[44] = ItemSubClassMask.WeaponAxe;
      WeaponProfessionSubClasses[45] = ItemSubClassMask.WeaponBow;
      WeaponProfessionSubClasses[226] = ItemSubClassMask.WeaponCrossbow;
      WeaponProfessionSubClasses[173] = ItemSubClassMask.WeaponDagger;
      WeaponProfessionSubClasses[356] = ItemSubClassMask.WeaponFishingPole;
      WeaponProfessionSubClasses[162] = ItemSubClassMask.WeaponFist;
      WeaponProfessionSubClasses[46] = ItemSubClassMask.WeaponGun;
      WeaponProfessionSubClasses[43] = ItemSubClassMask.WeaponOneHandSword;
      WeaponProfessionSubClasses[229] = ItemSubClassMask.WeaponPolearm;
      WeaponProfessionSubClasses[136] = ItemSubClassMask.WeaponStaff;
      WeaponProfessionSubClasses[176] = ItemSubClassMask.WeaponThrown;
      WeaponProfessionSubClasses[172] = ItemSubClassMask.WeaponTwoHandAxe;
      WeaponProfessionSubClasses[160] = ItemSubClassMask.WeaponTwoHandMace;
      WeaponProfessionSubClasses[55] = ItemSubClassMask.WeaponTwoHandSword;
      WeaponProfessionSubClasses[228] = ItemSubClassMask.WeaponWand;
      ArmorProfessionSubClasses[415] = ItemSubClassMask.WeaponTwoHandAxe;
      ArmorProfessionSubClasses[415] = ItemSubClassMask.WeaponTwoHandAxe;
      ArmorProfessionSubClasses[414] = ItemSubClassMask.WeaponBow;
      ArmorProfessionSubClasses[413] = ItemSubClassMask.WeaponGun;
      ArmorProfessionSubClasses[293] = ItemSubClassMask.WeaponPolearm;
      ArmorProfessionSubClasses[433] = ItemSubClassMask.Shield;
    }
  }
}
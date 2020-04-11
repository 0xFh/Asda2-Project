using System;

namespace WCell.Constants
{
    [Serializable]
    public enum SkillCategory
    {
        Invalid = -1,
        Attribute = 5,
        WeaponProficiency = 6,
        ClassSkill = 7,
        ArmorProficiency = 8,
        SecondarySkill = 9,
        Language = 10, // 0x0000000A
        Profession = 11, // 0x0000000B
        NotDisplayed = 12, // 0x0000000C
    }
}
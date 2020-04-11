using System.Runtime.Serialization;

namespace WCell.Constants
{
    /// <summary>The Ids for the different races</summary>
    /// <remarks>Values come from column 1 of ChrClasses.dbc</remarks>
    [DataContract]
    public enum ClassId : uint
    {
        [EnumMember] NoClass = 0,
        [EnumMember] OHS = 1,
        [EnumMember] Spear = 2,
        [EnumMember] THS = 3,
        [EnumMember] Crossbow = 4,
        [EnumMember] Bow = 5,
        [EnumMember] Balista = 6,
        [EnumMember] AtackMage = 7,
        [EnumMember] SupportMage = 8,
        [EnumMember] HealMage = 9,
        Druid = 11, // 0x0000000B
        End = 12, // 0x0000000C
    }
}
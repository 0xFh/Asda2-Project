using System.Runtime.Serialization;

namespace WCell.Intercommunication
{
    [DataContract]
    public enum ClassIdContract
    {
        [EnumMember] NoClass,
        [EnumMember] OHS,
        [EnumMember] Spear,
        [EnumMember] THS,
        [EnumMember] Crossbow,
        [EnumMember] Bow,
        [EnumMember] Balista,
        [EnumMember] AtackMage,
        [EnumMember] SupportMage,
        [EnumMember] HealMage,
    }
}
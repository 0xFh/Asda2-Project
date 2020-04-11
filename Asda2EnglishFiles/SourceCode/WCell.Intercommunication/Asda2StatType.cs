using System.Runtime.Serialization;

namespace WCell.Intercommunication
{
    [DataContract]
    public enum Asda2StatType
    {
        [EnumMember] Strength,
        [EnumMember] Dexterity,
        [EnumMember] Stamina,
        [EnumMember] Luck,
        [EnumMember] Intelect,
        [EnumMember] Spirit,
    }
}
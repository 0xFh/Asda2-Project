using System.Runtime.Serialization;

namespace WCell.Intercommunication
{
    [DataContract]
    public enum LogoutEnum
    {
        [EnumMember] Ok,
        [EnumMember] SomeOneAttakingYou,
        [EnumMember] YouMustLeaveWar,
        [EnumMember] Error,
    }
}
using System.Runtime.Serialization;

namespace WCell.Intercommunication
{
    [DataContract]
    public enum StartPkModeEnum
    {
        [EnumMember] Ok,
        [EnumMember] YouAlreadyPk,
        [EnumMember] YouMustLeaveGroup,
        [EnumMember] YouMustLeaveClan,
        [EnumMember] YouMustLeaveWar,
        [EnumMember] Error,
    }
}
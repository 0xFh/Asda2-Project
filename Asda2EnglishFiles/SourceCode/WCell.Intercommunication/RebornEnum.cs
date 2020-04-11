using System.Runtime.Serialization;

namespace WCell.Intercommunication
{
    [DataContract]
    public enum RebornEnum
    {
        [EnumMember] Ok,
        [EnumMember] YouMustReachAtLeast80Level,
        [EnumMember] YouMustLeaveWar,
        [EnumMember] YouMustPutOffCloses,
        [EnumMember] Error,
    }
}
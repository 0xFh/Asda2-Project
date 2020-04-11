using System.Runtime.Serialization;

namespace WCell.Intercommunication
{
    [DataContract]
    public enum ErrorTeleportationEnum
    {
        [EnumMember] Ok,
        [EnumMember] WaitingForTeleportation,
        [EnumMember] SomeOneAttakingYou,
        [EnumMember] Error,
        [EnumMember] CantDoItOnWar,
    }
}
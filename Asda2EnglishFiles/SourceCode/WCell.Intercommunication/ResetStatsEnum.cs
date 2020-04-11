using System.Runtime.Serialization;

namespace WCell.Intercommunication
{
    [DataContract]
    public enum ResetStatsEnum
    {
        [EnumMember] Ok,
        [EnumMember] NotEnoughtMoney,
        [EnumMember] Error,
    }
}
using System.Runtime.Serialization;

namespace WCell.Intercommunication
{
    [DataContract]
    public enum AuthorizeStatus
    {
        [EnumMember] Ok,
        [EnumMember] WrongLoginOrPass,
        [EnumMember] AlreadyConnected,
        [EnumMember] ServerIsBisy,
    }
}
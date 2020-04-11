using System.Runtime.Serialization;

namespace WCell.Intercommunication
{
    [DataContract]
    public enum RegisterStatus
    {
        [EnumMember] Ok,
        [EnumMember] DuplicateLogin,
        [EnumMember] BadPassword,
        [EnumMember] WrongCaptcha,
        [EnumMember] Error,
    }
}
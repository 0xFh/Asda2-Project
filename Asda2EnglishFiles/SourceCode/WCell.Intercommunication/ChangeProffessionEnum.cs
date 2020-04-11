using System.Runtime.Serialization;

namespace WCell.Intercommunication
{
    [DataContract]
    public enum ChangeProffessionEnum
    {
        [EnumMember] Ok,
        [EnumMember] YouAlreadyHaveChangedProffession,
        [EnumMember] YourLevelIsNotEnoght,
        [EnumMember] Error,
    }
}
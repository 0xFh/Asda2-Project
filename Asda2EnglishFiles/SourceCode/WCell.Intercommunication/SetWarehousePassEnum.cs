using System.Runtime.Serialization;

namespace WCell.Intercommunication
{
    [DataContract]
    public enum SetWarehousePassEnum
    {
        [EnumMember] Ok,
        [EnumMember] PassCantBeEmpty,
        [EnumMember] WrongOldPass,
        [EnumMember] Error,
    }
}
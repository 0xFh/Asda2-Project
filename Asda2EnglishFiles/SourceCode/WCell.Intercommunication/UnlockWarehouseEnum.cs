using System.Runtime.Serialization;

namespace WCell.Intercommunication
{
    [DataContract]
    public enum UnlockWarehouseEnum
    {
        [EnumMember] Ok,
        [EnumMember] WrongPass,
        [EnumMember] Error,
    }
}
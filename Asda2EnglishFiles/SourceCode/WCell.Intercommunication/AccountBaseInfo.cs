using System.Runtime.Serialization;

namespace WCell.Intercommunication
{
    [DataContract]
    public class AccountBaseInfo
    {
        [DataMember] public string Login;
        [DataMember] public string LastIp;
        [DataMember] public string Status;
    }
}
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WCell.Intercommunication
{
    [DataContract]
    public class AccountFullInfo : AccountBaseInfo
    {
        [DataMember] public List<CharacterBaseInfo> Characters;
    }
}
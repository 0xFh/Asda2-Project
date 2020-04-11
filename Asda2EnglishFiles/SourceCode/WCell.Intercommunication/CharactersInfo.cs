using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WCell.Intercommunication
{
    [DataContract]
    public class CharactersInfo
    {
        [DataMember] public int TotalCharacters;
        [DataMember] public int TotalOnlineCharacters;
        [DataMember] public List<CharacterBaseInfo> Characters;
    }
}
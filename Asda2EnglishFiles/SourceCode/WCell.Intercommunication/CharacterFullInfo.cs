using System.Runtime.Serialization;

namespace WCell.Intercommunication
{
    [DataContract]
    public class CharacterFullInfo : CharacterBaseInfo
    {
        [DataMember] public uint AccId;
    }
}
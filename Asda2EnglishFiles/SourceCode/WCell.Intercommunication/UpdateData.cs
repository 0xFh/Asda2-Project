using System.Runtime.Serialization;

namespace WCell.Intercommunication
{
    [DataContract]
    public class UpdateData
    {
        [DataMember] public int Online;
        [DataMember] public string Login;
        [DataMember] public string CurrentCharacterName;
        [DataMember] public int MaxCharacterHealth;
        [DataMember] public int CurCharacterHealth;
        [DataMember] public int MaxCharacterMana;
        [DataMember] public int CurCharacterMana;
        [DataMember] public byte CurCharacterLevel;
        [DataMember] public uint CurCharacerMoney;
        [DataMember] public byte CurCharacterMap;
        [DataMember] public short CurCharacterX;
        [DataMember] public short CurCharacterY;
        [DataMember] public int Agility;
        [DataMember] public int Luck;
        [DataMember] public int Spirit;
        [DataMember] public int Strenght;
        [DataMember] public int Stamina;
        [DataMember] public int Intellect;
        [DataMember] public int FreePoints;
        [DataMember] public int ResetsCount;
        [DataMember] public int FishingLevel;
        [DataMember] public short FactionId;
        [DataMember] public byte CraftLevel;
        [DataMember] public bool IsAdmin;
    }
}
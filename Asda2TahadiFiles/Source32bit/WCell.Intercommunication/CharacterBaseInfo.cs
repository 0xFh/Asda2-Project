using System.Runtime.Serialization;

namespace WCell.Intercommunication
{
  [DataContract]
  public class CharacterBaseInfo
  {
    [DataMember]public uint Id;
    [DataMember]public string Name;
    [DataMember]public byte Level;
    [DataMember]public ClassIdContract ClassId;

    public string Info
    {
      get
      {
        return string.Format("[{0}] [{1}] [level:{2}] [{3}]", (object) Id, (object) Name,
          (object) Level, (object) ClassId);
      }
    }
  }
}
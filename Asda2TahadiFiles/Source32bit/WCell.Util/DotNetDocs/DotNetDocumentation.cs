using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace WCell.Util.DotNetDocs
{
  [XmlRoot("doc")]
  public class DotNetDocumentation : XmlFile<DotNetDocumentation>
  {
    private static readonly Dictionary<char, MemberType> TypeMap = new Dictionary<char, MemberType>();

    static DotNetDocumentation()
    {
      TypeMap.Add('T', MemberType.Type);
      TypeMap.Add('F', MemberType.Field);
      TypeMap.Add('P', MemberType.Property);
      TypeMap.Add('M', MemberType.Method);
      TypeMap.Add('E', MemberType.Event);
    }

    public static MemberType GetMemberType(char shortcut)
    {
      MemberType memberType;
      if(!TypeMap.TryGetValue(shortcut, out memberType))
        throw new Exception("Undefined Type-shortcut: " + shortcut);
      return memberType;
    }

    protected override void OnLoad()
    {
    }

    [XmlElement("assembly")]
    public string Assembly { get; set; }

    [XmlElement("member")]
    [XmlArray("members")]
    public DocEntry[] Members { get; set; }
  }
}
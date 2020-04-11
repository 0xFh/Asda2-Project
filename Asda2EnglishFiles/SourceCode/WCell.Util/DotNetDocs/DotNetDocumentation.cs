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
            DotNetDocumentation.TypeMap.Add('T', MemberType.Type);
            DotNetDocumentation.TypeMap.Add('F', MemberType.Field);
            DotNetDocumentation.TypeMap.Add('P', MemberType.Property);
            DotNetDocumentation.TypeMap.Add('M', MemberType.Method);
            DotNetDocumentation.TypeMap.Add('E', MemberType.Event);
        }

        public static MemberType GetMemberType(char shortcut)
        {
            MemberType memberType;
            if (!DotNetDocumentation.TypeMap.TryGetValue(shortcut, out memberType))
                throw new Exception("Undefined Type-shortcut: " + (object) shortcut);
            return memberType;
        }

        protected override void OnLoad()
        {
        }

        [XmlElement("assembly")] public string Assembly { get; set; }

        [XmlElement("member")]
        [XmlArray("members")]
        public DocEntry[] Members { get; set; }
    }
}
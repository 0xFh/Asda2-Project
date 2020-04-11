using System.Xml.Serialization;

namespace WCell.Util.DotNetDocs
{
    /// <summary>
    /// 
    /// </summary>
    public class DocEntry
    {
        private string m_FullName;
        private string m_Name;
        private MemberType m_type;

        public MemberType MemberType
        {
            get { return this.m_type; }
        }

        [XmlElement("name")]
        public string FullName
        {
            get { return this.m_FullName; }
            set
            {
                this.m_FullName = value;
                this.m_type = DotNetDocumentation.GetMemberType(this.m_FullName[0]);
                int num = this.m_FullName.IndexOf('(');
                if (num < 0)
                    num = this.m_FullName.Length;
                this.m_Name = this.m_FullName.Substring(2, num - 1);
            }
        }

        [XmlElement("summary")] public string Summary { get; set; }

        [XmlElement("remarks")] public string Remarks { get; set; }

        [XmlElement("returns")] public string Returns { get; set; }

        [XmlElement("value")] public string Value { get; set; }

        [XmlElement("exceptions")] public string[] Exceptions { get; set; }

        [XmlElement("see")] public string[] See { get; set; }

        [XmlElement("seealso")] public string[] SeeAlso { get; set; }
    }
}
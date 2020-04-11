using System.Xml.Serialization;

namespace WCell.Util.DB.Xml
{
    public class DefVersion
    {
        [XmlAttribute] public string Table { get; set; }

        [XmlAttribute] public string Column { get; set; }

        [XmlAttribute] public float MinVersion { get; set; }

        [XmlAttribute] public float MaxVersion { get; set; }

        public bool IsValid
        {
            get { return !string.IsNullOrEmpty(this.Column) && !string.IsNullOrEmpty(this.Table); }
        }

        public override string ToString()
        {
            return "Table: " + this.Table + ", Column: " + this.Column;
        }
    }
}
using System.Xml.Serialization;

namespace WCell.Util.DB.Xml
{
    public class NestedArrayFieldDefinition : DataFieldDefinition, IArray
    {
        [XmlAttribute] public string Table { get; set; }

        [XmlElement("Flat", typeof(FlatArrayFieldDefinition))]
        public FlatArrayFieldDefinition[] Segments { get; set; }

        public override DataFieldType DataFieldType
        {
            get { return DataFieldType.NestedArray; }
        }
    }
}
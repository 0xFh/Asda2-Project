using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace WCell.Util.DB.Xml
{
    public class XmlDataHolderDefinition : IHasDataFieldDefinitions
    {
        [XmlAttribute] public string Name { get; set; }

        [XmlElement("DefaultTable")] public string[] DefaultTables { get; set; }

        [XmlElement("Nested", typeof(NestedSimpleFieldDefinition))]
        [XmlElement("FlatArray", typeof(FlatArrayFieldDefinition))]
        [XmlElement("Flat", typeof(SimpleFlatFieldDefinition))]
        [XmlElement("NestedArray", typeof(NestedArrayFieldDefinition))]
        public DataFieldDefinition[] Fields { get; set; }

        public string DataHolderName
        {
            get { return this.Name; }
        }

        public IEnumerator GetEnumerator()
        {
            return this.Fields.GetEnumerator();
        }

        public override string ToString()
        {
            return this.Name + " (" + ((IEnumerable<string>) this.DefaultTables).ToString<string>(", ") + ")";
        }
    }
}
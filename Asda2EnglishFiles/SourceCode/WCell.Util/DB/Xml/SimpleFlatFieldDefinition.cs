using System.Xml.Serialization;

namespace WCell.Util.DB.Xml
{
    /// <summary>Single column flat field</summary>
    public class SimpleFlatFieldDefinition : DataFieldDefinition, IFlatField, IDataFieldDefinition
    {
        private string m_Column;

        public SimpleFlatFieldDefinition()
        {
        }

        public SimpleFlatFieldDefinition(string table, string column)
        {
            this.Table = table;
            this.Column = column;
        }

        public SimpleFlatFieldDefinition(string table, string column, string defaultVal)
        {
            this.Table = table;
            this.Column = column;
            this.DefaultStringValue = defaultVal;
        }

        /// <summary>
        /// Optional. By default the first specified table
        /// for the containing DataHolder is used.
        /// </summary>
        [XmlAttribute]
        public string Table { get; set; }

        /// <summary>
        /// The column from which to copy the value to this Field.
        /// </summary>
        [XmlAttribute]
        public string Column
        {
            get { return this.m_Column; }
            set { this.m_Column = value; }
        }

        /// <summary>
        /// The column from which to copy the value to this Field.
        /// </summary>
        [XmlAttribute("DefaultValue")]
        public string DefaultStringValue { get; set; }

        public override DataFieldType DataFieldType
        {
            get { return DataFieldType.FlatSimple; }
        }
    }
}
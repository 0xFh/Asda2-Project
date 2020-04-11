using System.Xml.Serialization;

namespace WCell.Util.DB.Xml
{
    public abstract class BaseFieldArrayDefinition : DataFieldDefinition
    {
        /// <summary>
        /// Specify an array through a pattern where the <see cref="T:WCell.Util.DB.Patterns" />
        /// class defines possible constants.
        /// </summary>
        [XmlAttribute]
        public string Pattern { get; set; }

        /// <summary>Offset for pattern</summary>
        [XmlAttribute]
        public int Offset { get; set; }

        [XmlAttribute] public string Table { get; set; }

        /// <summary>
        /// An alternative way:
        /// Specify all columns of the Array explicitely
        /// </summary>
        [XmlElement("Column")]
        public Column[] ExpliciteColumns { get; set; }
    }
}
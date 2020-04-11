using System.Xml.Serialization;
using WCell.Util.Data;

namespace WCell.Util.DB.Xml
{
    public abstract class DataFieldDefinition : IDataFieldDefinition
    {
        private string m_Name;

        /// <summary>The name of the DataField it belongs to</summary>
        [XmlAttribute]
        public string Name
        {
            get
            {
                this.EnsureName();
                return this.m_Name;
            }
            set { this.m_Name = value; }
        }

        public void EnsureName()
        {
            if (string.IsNullOrEmpty(this.m_Name))
                throw new DataHolderException(
                    "DataHolder-definition contained empty field-definitions without Name - Name is required.",
                    new object[0]);
        }

        public abstract DataFieldType DataFieldType { get; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
using System.Xml.Serialization;

namespace WCell.Util
{
    public abstract class XmlFileBase
    {
        /// <summary>The file name of the configuration file.</summary>
        protected string m_filename;

        protected XmlFileBase m_parentConfig;

        [XmlIgnore]
        public string FileName
        {
            get { return this.m_filename; }
            set { this.m_filename = value; }
        }

        [XmlIgnore]
        public string ActualFile
        {
            get
            {
                if (this.m_parentConfig != null)
                    return this.m_parentConfig.FileName;
                return this.m_filename;
            }
        }

        public abstract void Save();

        public abstract void SaveAs(string filename);

        protected abstract void OnLoad();
    }
}
using System.IO;

namespace WCell.Util
{
    public class FileStreamTarget : IStreamTarget
    {
        private string m_path;
        private IndentTextWriter m_Writer;

        public FileStreamTarget(string name)
        {
            this.m_path = Path.GetFullPath(name);
        }

        public string Name
        {
            get { return this.m_path; }
            set
            {
                if (this.m_Writer != null)
                    this.m_Writer.Close();
                this.m_path = value;
                this.m_Writer = new IndentTextWriter(this.m_path);
            }
        }

        public IndentTextWriter Writer
        {
            get { return this.m_Writer; }
        }

        public void Flush()
        {
            this.m_Writer.Flush();
        }

        /// <summary>
        /// Opens a new StreamWriter to the given Path if not already opened.
        /// </summary>
        public void Open()
        {
            if (this.m_Writer != null)
                this.Close();
            this.m_Writer = new IndentTextWriter(this.m_path);
        }

        public void Close()
        {
            this.m_Writer.Close();
        }
    }
}
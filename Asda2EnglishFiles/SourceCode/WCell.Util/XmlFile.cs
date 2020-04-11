using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace WCell.Util
{
    [Serializable]
    public class XmlFile<T> : XmlFileBase where T : XmlFileBase
    {
        protected XmlFile()
        {
        }

        /// <summary>Constructor.</summary>
        /// <param name="fileName">The name of the configuration file.</param>
        public XmlFile(string fileName)
        {
            this.m_filename = fileName;
        }

        public XmlFile(XmlFileBase parentConfig)
        {
            this.m_parentConfig = parentConfig;
        }

        /// <summary>Returns whether or not the file exists</summary>
        public virtual bool FileExists(string path)
        {
            return File.Exists((string.IsNullOrEmpty(path) ? "" : path + "\\") + this.m_filename);
        }

        /// <summary>Writes the configuration file to disk.</summary>
        public override void Save()
        {
            if (this.m_parentConfig != null)
            {
                this.m_parentConfig.Save();
            }
            else
            {
                XmlSerializer xmlSerializer = new XmlSerializer(this.GetType());
                string directoryName = Path.GetDirectoryName(this.m_filename);
                if (directoryName.Length > 0 && !Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
                using (TextWriter textWriter = (TextWriter) new StreamWriter(this.m_filename, false, Encoding.UTF8))
                    xmlSerializer.Serialize(textWriter, (object) this);
            }
        }

        /// <summary>
        /// Writes the configuration file to disk with the specified name.
        /// </summary>
        /// <param name="fileName">The name of the file on disk to write to.</param>
        public override void SaveAs(string fileName)
        {
            this.m_filename = fileName;
            this.Save();
        }

        /// <summary>
        /// Writes the configuration file to disk with the specified name.
        /// </summary>
        /// <param name="fileName">The name of the file on disk to write to.</param>
        /// <param name="location">The directory to write the file to.</param>
        public virtual void SaveAs(string fileName, string location)
        {
            if (string.IsNullOrEmpty(location))
                throw new ArgumentException("Location cannot be be null or empty!", nameof(location));
            this.m_filename = fileName;
            XmlSerializer xmlSerializer = new XmlSerializer(this.GetType());
            if (!Directory.Exists(location))
                Directory.CreateDirectory(location);
            if ((int) location[location.Length - 1] != (int) Path.DirectorySeparatorChar)
                location += Path.DirectorySeparatorChar;
            location += this.m_filename;
            using (TextWriter textWriter = (TextWriter) new StreamWriter(location, false, Encoding.UTF8))
            {
                xmlSerializer.Serialize(textWriter, (object) this);
                textWriter.Close();
            }
        }

        protected override void OnLoad()
        {
        }

        /// <summary>
        /// Returns the serialized XML of this XmlConfig for further processing, etc.
        /// </summary>
        public override string ToString()
        {
            return this.FileName;
        }

        public static T Load(string filename)
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));
            T cfg;
            using (XmlReader rdr = XmlReader.Create(filename))
            {
                try
                {
                    cfg = (T) ((object) ser.Deserialize(rdr));
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to read XML file: " + filename, e);
                }
            }

            cfg.FileName = filename;
            ((XmlFile<T>) ((object) cfg)).OnLoad();
            return cfg;
        }

        public static ICollection<T> LoadAll(string dir)
        {
            List<T> objList = new List<T>();
            XmlFile<T>.LoadAll(dir, (ICollection<T>) objList);
            return (ICollection<T>) objList;
        }

        public static ICollection<T> LoadAll(DirectoryInfo dir)
        {
            List<T> objList = new List<T>();
            XmlFile<T>.LoadAll(dir, (ICollection<T>) objList);
            return (ICollection<T>) objList;
        }

        public static void LoadAll(string dir, ICollection<T> cfgs)
        {
            XmlFile<T>.LoadAll(new DirectoryInfo(dir), cfgs);
        }

        public static void LoadAll(DirectoryInfo dir, ICollection<T> cfgs)
        {
            foreach (FileSystemInfo fileSystemInfo in dir.GetFileSystemInfos())
            {
                if (fileSystemInfo is DirectoryInfo)
                    XmlFile<T>.LoadAll((DirectoryInfo) fileSystemInfo, cfgs);
                else if (fileSystemInfo.Extension.EndsWith("xml", StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        T obj = XmlFile<T>.Load(fileSystemInfo.FullName);
                        cfgs.Add(obj);
                    }
                    catch (Exception ex)
                    {
                        Exception exception =
                            new Exception("Error when loading XML-file: " + (object) fileSystemInfo, ex);
                        Debugger.Break();
                        throw exception;
                    }
                }
            }
        }
    }
}
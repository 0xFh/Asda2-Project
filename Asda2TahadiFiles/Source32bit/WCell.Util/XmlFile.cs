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
      m_filename = fileName;
    }

    public XmlFile(XmlFileBase parentConfig)
    {
      m_parentConfig = parentConfig;
    }

    /// <summary>Returns whether or not the file exists</summary>
    public virtual bool FileExists(string path)
    {
      return File.Exists((string.IsNullOrEmpty(path) ? "" : path + "\\") + m_filename);
    }

    /// <summary>Writes the configuration file to disk.</summary>
    public override void Save()
    {
      if(m_parentConfig != null)
      {
        m_parentConfig.Save();
      }
      else
      {
        XmlSerializer xmlSerializer = new XmlSerializer(GetType());
        string directoryName = Path.GetDirectoryName(m_filename);
        if(directoryName.Length > 0 && !Directory.Exists(directoryName))
          Directory.CreateDirectory(directoryName);
        using(TextWriter textWriter = new StreamWriter(m_filename, false, Encoding.UTF8))
          xmlSerializer.Serialize(textWriter, this);
      }
    }

    /// <summary>
    /// Writes the configuration file to disk with the specified name.
    /// </summary>
    /// <param name="fileName">The name of the file on disk to write to.</param>
    public override void SaveAs(string fileName)
    {
      m_filename = fileName;
      Save();
    }

    /// <summary>
    /// Writes the configuration file to disk with the specified name.
    /// </summary>
    /// <param name="fileName">The name of the file on disk to write to.</param>
    /// <param name="location">The directory to write the file to.</param>
    public virtual void SaveAs(string fileName, string location)
    {
      if(string.IsNullOrEmpty(location))
        throw new ArgumentException("Location cannot be be null or empty!", nameof(location));
      m_filename = fileName;
      XmlSerializer xmlSerializer = new XmlSerializer(GetType());
      if(!Directory.Exists(location))
        Directory.CreateDirectory(location);
      if(location[location.Length - 1] != Path.DirectorySeparatorChar)
        location += Path.DirectorySeparatorChar;
      location += m_filename;
      using(TextWriter textWriter = new StreamWriter(location, false, Encoding.UTF8))
      {
        xmlSerializer.Serialize(textWriter, this);
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
      return FileName;
    }

    public static T Load(string filename)
    {
      XmlSerializer ser = new XmlSerializer(typeof(T));
      T cfg;
      using(XmlReader rdr = XmlReader.Create(filename))
      {
        try
        {
          cfg = (T) ser.Deserialize(rdr);
        }
        catch(Exception e)
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
      LoadAll(dir, objList);
      return objList;
    }

    public static ICollection<T> LoadAll(DirectoryInfo dir)
    {
      List<T> objList = new List<T>();
      LoadAll(dir, objList);
      return objList;
    }

    public static void LoadAll(string dir, ICollection<T> cfgs)
    {
      LoadAll(new DirectoryInfo(dir), cfgs);
    }

    public static void LoadAll(DirectoryInfo dir, ICollection<T> cfgs)
    {
      foreach(FileSystemInfo fileSystemInfo in dir.GetFileSystemInfos())
      {
        if(fileSystemInfo is DirectoryInfo)
          LoadAll((DirectoryInfo) fileSystemInfo, cfgs);
        else if(fileSystemInfo.Extension.EndsWith("xml", StringComparison.InvariantCultureIgnoreCase))
        {
          try
          {
            T obj = Load(fileSystemInfo.FullName);
            cfgs.Add(obj);
          }
          catch(Exception ex)
          {
            Exception exception =
              new Exception("Error when loading XML-file: " + fileSystemInfo, ex);
            Debugger.Break();
            throw exception;
          }
        }
      }
    }
  }
}
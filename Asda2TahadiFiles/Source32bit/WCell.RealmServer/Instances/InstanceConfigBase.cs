using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using WCell.Util;
using WCell.Util.NLog;

namespace WCell.RealmServer.Instances
{
  public abstract class InstanceConfigBase<T, E> : XmlFile<T>, IInstanceConfig
    where T : XmlFileBase, IInstanceConfig, new() where E : IComparable
  {
    [XmlIgnore]private Dictionary<E, InstanceConfigEntry<E>> m_Settings =
      new Dictionary<E, InstanceConfigEntry<E>>();

    private static string filename;
    [XmlIgnore]private InstanceConfigEntry<E>[] m_entries;

    public static string Filename
    {
      get { return filename; }
    }

    protected static T LoadSettings(string fileName)
    {
      filename = RealmServerConfiguration.GetContentPath(fileName);
      T obj = !File.Exists(filename)
        ? Activator.CreateInstance<T>()
        : Load(filename);
      obj.Setup();
      try
      {
        obj.SaveAs(filename);
      }
      catch(Exception ex)
      {
        LogUtil.WarnException(ex, "Unable to save Configuration file");
      }

      return obj;
    }

    [XmlElement("Setting")]
    public InstanceConfigEntry<E>[] Entries
    {
      get { return m_entries; }
      set
      {
        m_entries = value;
        SortSettings();
      }
    }

    [XmlIgnore]
    public Dictionary<E, InstanceConfigEntry<E>> Settings
    {
      get { return m_Settings; }
      set { m_Settings = value; }
    }

    [XmlIgnore]
    public abstract IEnumerable<E> SortedIds { get; }

    public InstanceConfigEntry<E> GetSetting(E id)
    {
      InstanceConfigEntry<E> instanceConfigEntry;
      m_Settings.TryGetValue(id, out instanceConfigEntry);
      return instanceConfigEntry;
    }

    protected abstract void InitSetting(InstanceConfigEntry<E> configEntry);

    public void Setup()
    {
      if(Entries == null)
      {
        SortSettings();
      }
      else
      {
        foreach(InstanceConfigEntry<E> configEntry in Settings.Values)
        {
          if(configEntry != null && configEntry.TypeName.Trim().Length > 0)
            InitSetting(configEntry);
        }
      }
    }

    private void SortSettings()
    {
      if(Entries != null)
      {
        foreach(InstanceConfigEntry<E> entry in Entries)
        {
          if(entry != null)
            Settings[entry.Name] = entry;
        }
      }

      CreateStubs();
      m_entries = m_Settings.Values.ToArray();
      Array.Sort(m_entries);
    }

    private void CreateStubs()
    {
      CreateStubs(SortedIds);
    }

    private void CreateStubs(IEnumerable<E> sortedIds)
    {
      foreach(E sortedId in sortedIds)
      {
        if(GetSetting(sortedId) == null)
          Settings[sortedId] = new InstanceConfigEntry<E>(sortedId, " ");
      }
    }
  }
}
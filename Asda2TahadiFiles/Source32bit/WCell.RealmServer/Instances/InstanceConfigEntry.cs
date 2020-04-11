using System;
using System.Xml.Serialization;

namespace WCell.RealmServer.Instances
{
  public class InstanceConfigEntry<E> : IComparable where E : IComparable
  {
    private string m_TypeName;

    public InstanceConfigEntry()
    {
      TypeName = " ";
    }

    public InstanceConfigEntry(E id)
      : this(id, " ")
    {
    }

    public InstanceConfigEntry(E id, string typeName)
    {
      Name = id;
      TypeName = typeName;
    }

    [XmlElement("Name")]
    public E Name { get; set; }

    [XmlElement("Type")]
    public string TypeName
    {
      get { return m_TypeName; }
      set { m_TypeName = value; }
    }

    public int CompareTo(object obj)
    {
      InstanceConfigEntry<E> instanceConfigEntry = obj as InstanceConfigEntry<E>;
      if(instanceConfigEntry != null)
        return Name.CompareTo(instanceConfigEntry.Name);
      return -1;
    }
  }
}
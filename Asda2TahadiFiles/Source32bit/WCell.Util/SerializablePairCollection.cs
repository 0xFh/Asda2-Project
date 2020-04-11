using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using WCell.Util.Xml;

namespace WCell.Util
{
  public class SerializablePairCollection : IXmlSerializable
  {
    public readonly List<KeyValuePair<string, string>> Pairs = new List<KeyValuePair<string, string>>();
    private string m_name;

    public SerializablePairCollection()
    {
    }

    public SerializablePairCollection(string name)
    {
      m_name = name;
    }

    public void Add(string key, string value)
    {
      Pairs.Add(new KeyValuePair<string, string>(key, value));
    }

    public void ReadXml(XmlReader reader)
    {
      m_name = reader.Name;
      while(reader.Read())
      {
        reader.SkipEmptyNodes();
        if(reader.NodeType == XmlNodeType.EndElement)
          break;
        if(reader.NodeType == XmlNodeType.Element)
        {
          reader.Read();
          reader.SkipEmptyNodes();
        }

        if(reader.NodeType != XmlNodeType.Text)
          throw new Exception("Required NodeType: Text - Found: " + reader.NodeType);
        string str = reader.ReadContentAsString();
        Pairs.Add(new KeyValuePair<string, string>(reader.Name, str));
        if(reader.NodeType == XmlNodeType.EndElement)
          reader.ReadInnerXml();
      }
    }

    public void WriteXml(XmlWriter writer)
    {
      Pairs.Sort(
        (pair1, pair2) => pair1.Key.CompareTo(pair2.Key));
      foreach(KeyValuePair<string, string> pair in Pairs)
      {
        writer.WriteWhitespace("\n\t\t\t");
        writer.WriteStartElement(pair.Key);
        writer.WriteValue(pair.Value);
        writer.WriteEndElement();
      }
    }

    public XmlSchema GetSchema()
    {
      throw new NotImplementedException();
    }
  }
}
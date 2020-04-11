using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using WCell.Util.NLog;
using WCell.Util.Xml;

namespace WCell.Util.Strings
{
  public class StringNode<V> : IXmlSerializable where V : class, IXmlSerializable
  {
    internal readonly IDictionary<string, StringNode<V>> Children =
      new SortedList<string, StringNode<V>>();

    protected string m_Name;
    protected V m_Value;
    protected StringTree<V> m_tree;
    protected StringNode<V> m_Parent;
    protected int m_depth;
    protected string m_indent;

    protected internal StringNode(StringTree<V> tree)
    {
      m_tree = tree;
    }

    protected internal StringNode(StringTree<V> tree, string key, V value)
      : this(tree)
    {
      m_Name = key;
      m_Value = value;
    }

    public string Name
    {
      get { return m_Name; }
    }

    public string FullName
    {
      get
      {
        if(m_Parent == null || string.IsNullOrEmpty(m_Parent.Name))
          return m_Name;
        return m_Parent.FullName + m_tree.Seperators[0] + m_Name;
      }
    }

    public V Value
    {
      get { return m_Value; }
      set { m_Value = value; }
    }

    public int ChildCount
    {
      get { return Children.Count; }
    }

    public StringNode<V> Parent
    {
      get { return m_Parent; }
    }

    public string GetQualifiedName(string name)
    {
      return (FullName != null ? FullName + m_tree.Seperators[0] : "") + name;
    }

    public StringNode<V> GetChild(string key)
    {
      StringNode<V> stringNode;
      Children.TryGetValue(key, out stringNode);
      return stringNode;
    }

    public StringNode<V> FindChild(string keyChain)
    {
      return FindChild(keyChain.Split(m_tree.Seperators, StringSplitOptions.RemoveEmptyEntries), 0);
    }

    public StringNode<V> FindChild(string[] keyChain)
    {
      return FindChild(keyChain, 0);
    }

    public StringNode<V> FindChild(string[] keyChain, int index)
    {
      if(index >= keyChain.Length)
        return this;
      return GetChild(keyChain[index])?.FindChild(keyChain, index + 1);
    }

    public StringNode<V> GetOrCreate(string key)
    {
      return GetChild(key) ?? AddChild(key, default(V));
    }

    public V GetValue(string key)
    {
      StringNode<V> child = GetChild(key);
      return child != null ? child.Value : default(V);
    }

    public V FindValue(string keyChain)
    {
      StringNode<V> child = FindChild(keyChain);
      return child != null ? child.Value : default(V);
    }

    public V FindValue(string[] keyChain)
    {
      StringNode<V> child = FindChild(keyChain);
      return child != null ? child.Value : default(V);
    }

    public V FindValue(string[] keyChain, int index)
    {
      StringNode<V> child = FindChild(keyChain, index);
      return child != null ? child.Value : default(V);
    }

    public StringNode<V> AddChild(string key, V value)
    {
      StringNode<V> child = new StringNode<V>(m_tree, key, value);
      AddChild(child);
      return child;
    }

    public StringNode<V> AddChildInChain(string keyChain, V value)
    {
      return AddChildInChain(keyChain.Split(m_tree.Seperators, StringSplitOptions.RemoveEmptyEntries),
        value);
    }

    public StringNode<V> AddChildInChain(string[] keyChain, V value)
    {
      StringNode<V> stringNode = this;
      for(int index = 0; index < keyChain.Length; ++index)
      {
        string key = keyChain[index];
        stringNode = stringNode.GetOrCreate(key);
        if(index == keyChain.Length - 1)
          stringNode.Value = value;
      }

      return stringNode;
    }

    public void AddChild(StringNode<V> child)
    {
      Children.Add(child.Name, child);
      child.m_Parent = this;
      child.m_depth = m_depth + 1;
      child.m_indent = m_indent + "\t";
    }

    public void Remove()
    {
      if(m_Parent == null)
        throw new InvalidOperationException("Cannot remove the Root of a Tree.");
      m_Parent.Children.Remove(m_Name);
      m_Parent = null;
      m_depth = 0;
    }

    /// <summary>
    /// Removes and returns the direct Child with the given key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public StringNode<V> RemoveChild(string key)
    {
      StringNode<V> child = GetChild(key);
      if(child != null)
        child.Remove();
      return child;
    }

    /// <summary>
    /// Removes and returns the direct Child with the given key
    /// </summary>
    /// <returns></returns>
    public StringNode<V> FindAndRemoveChild(string keyChain)
    {
      StringNode<V> child = FindChild(keyChain);
      if(child != null)
        child.Remove();
      return child;
    }

    /// <summary>
    /// Removes and returns the direct Child with the given key
    /// </summary>
    /// <returns></returns>
    public StringNode<V> FindAndRemoveChild(string[] keyChain)
    {
      StringNode<V> child = FindChild(keyChain);
      if(child != null)
        child.Remove();
      return child;
    }

    /// <summary>
    /// Removes and returns the direct Child with the given key
    /// </summary>
    /// <returns></returns>
    public StringNode<V> FindAndRemoveChild(string[] keyChain, int index)
    {
      StringNode<V> child = FindChild(keyChain, index);
      if(child != null)
        child.Remove();
      return child;
    }

    public void ReadXml(XmlReader reader)
    {
      if(Value != null)
      {
        try
        {
          Value.ReadXml(reader);
        }
        catch(Exception ex)
        {
          LogUtil.ErrorException(ex, string.Format("Failed to parse Node: {0}", FullName));
        }
      }

      if(Children.Count > 0)
      {
        List<StringNode<V>> stringNodeList = new List<StringNode<V>>();
        int num = 0;
        while(num < Children.Count)
        {
          reader.Read();
          if(reader.ReadState == ReadState.EndOfFile)
            throw new Exception("Unexpected EOF in Config.");
          reader.SkipEmptyNodes();
          if(reader.NodeType != XmlNodeType.EndElement)
          {
            if(reader.NodeType == XmlNodeType.Element)
            {
              StringNode<V> child = GetChild(reader.Name);
              if(child == null)
              {
                OnInvalidNode(reader);
              }
              else
              {
                ++num;
                stringNodeList.Add(child);
                if(!reader.IsEmptyElement)
                  child.ReadXml(reader);
                else
                  reader.SkipEmptyNodes();
              }
            }
          }
          else
            break;
        }

        IEnumerable<StringNode<V>> stringNodes =
          Children.Values.Except(stringNodeList);
        if(stringNodes.Count() > 0)
          m_tree.OnError("Found {0} missing Node(s): {1}", (object) stringNodes.Count(),
            (object) stringNodes.ToString(", ",
              node => (object) node.FullName));
      }

      reader.SkipEmptyNodes();
      if(reader.IsEmptyElement)
        reader.Read();
      reader.SkipEmptyNodes();
      while(reader.NodeType != XmlNodeType.EndElement)
      {
        OnInvalidNode(reader);
        reader.SkipEmptyNodes();
      }

      reader.ReadEndElement();
    }

    private void OnInvalidNode(XmlReader reader)
    {
      m_tree.OnError("Found invalid Node \"{0}\"", (object) GetQualifiedName(reader.Name));
      if(reader.IsEmptyElement)
        return;
      int num = 1;
      do
      {
        reader.Read();
        if(reader.ReadState == ReadState.EndOfFile)
          throw new Exception("Unexpected EOF in Config.");
        if(reader.NodeType == XmlNodeType.EndElement)
        {
          reader.ReadEndElement();
          --num;
        }
        else if(reader.NodeType == XmlNodeType.Element && !reader.IsEmptyElement)
          ++num;
      } while(num > 0);
    }

    public void WriteXml(XmlWriter writer)
    {
      if(Value != null)
        Value.WriteXml(writer);
      if(Children.Count <= 0)
        return;
      foreach(StringNode<V> stringNode in Children.Values)
      {
        writer.WriteStartElement(stringNode.Name);
        stringNode.WriteXml(writer);
        writer.WriteEndElement();
      }
    }

    public XmlSchema GetSchema()
    {
      throw new NotImplementedException();
    }

    public override string ToString()
    {
      return FullName;
    }
  }
}
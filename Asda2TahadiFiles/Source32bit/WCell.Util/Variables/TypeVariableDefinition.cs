using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using WCell.Util.Xml;

namespace WCell.Util.Variables
{
  public class TypeVariableDefinition : IComparable, IVariableDefinition, IXmlSerializable
  {
    public static readonly object[] EmptyObjectArray = new object[0];
    public static readonly Type GenericListType = typeof(IList<>);
    private const string ENUMERABLE_ITEM_NAME = "Item";
    internal MemberInfo m_Member;
    private bool m_isXmlSerializable;
    private Type m_collectionType;

    /// <summary>
    /// The object that holds the field or property (or null if static)
    /// </summary>
    public readonly object Object;

    public bool Serialized;
    private bool m_readOnly;

    public TypeVariableDefinition()
    {
    }

    public TypeVariableDefinition(string name, MemberInfo member, bool serialized, bool readOnly)
    {
      Name = name;
      Member = member;
      Serialized = serialized;
      m_readOnly = readOnly;
    }

    public TypeVariableDefinition(string name, object obj, MemberInfo member, bool serialized, bool readOnly)
      : this(name, member, serialized, readOnly)
    {
      Object = obj;
    }

    public string Name { get; internal set; }

    public bool IsReadOnly
    {
      get { return m_readOnly; }
      internal set { m_readOnly = value; }
    }

    public bool IsFileOnly { get; internal set; }

    public MemberInfo Member
    {
      get { return m_Member; }
      internal set
      {
        m_Member = value;
        FullName = GetSafeName();
        Type variableType = m_Member.GetVariableType();
        m_isXmlSerializable = variableType.GetInterface("IXmlSerializable") != null;
        if(!(variableType.GetInterface("IEnumerable") != null) || !(variableType != typeof(string)))
          return;
        if(variableType.IsArray)
        {
          m_collectionType = variableType.GetElementType();
        }
        else
        {
          Type type = variableType.GetInterface(GenericListType.Name);
          if(type == null)
            throw new Exception(
              "Cannot create TypeVariableDefinition for IEnumerable, unless it is an Array or implements IList<T>.");
          m_collectionType = type.GetGenericArguments().First();
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private string GetSafeName()
    {
      return m_Member.DeclaringType.FullName.Replace("+", ".").Replace("#", ".") + "." + Name;
    }

    public string FullName { get; private set; }

    public Type VariableType
    {
      get { return m_Member.GetVariableType(); }
    }

    public object Value
    {
      get { return m_Member.GetUnindexedValue(Object); }
      set { m_Member.SetUnindexedValue(Object, value); }
    }

    public string TypeName
    {
      get { return VariableType.Name; }
    }

    public bool TrySet(string strValue)
    {
      if(IsReadOnly)
        return false;
      object obj = TryParse(strValue, VariableType);
      if(obj == null)
        return false;
      Value = obj;
      return true;
    }

    private static object TryParse(string strValue, Type type)
    {
      object obj = null;
      if(StringParser.Parse(strValue, type, ref obj))
        return obj;
      return null;
    }

    public int CompareTo(object obj)
    {
      if(obj is TypeVariableDefinition)
        return ((TypeVariableDefinition) obj).Name.CompareTo(Name);
      return -1;
    }

    public void ReadXml(XmlReader reader)
    {
      object obj = Value;
      try
      {
        Type variableType = m_Member.GetVariableType();
        if(m_isXmlSerializable)
        {
          if(Value == null)
            Value = Activator.CreateInstance(variableType);
          ((IXmlSerializable) Value).ReadXml(reader);
        }
        else if(variableType.IsSimpleType())
        {
          TrySet(reader.ReadString());
        }
        else
        {
          if(!(m_collectionType != null))
            throw new NotImplementedException("Cannot serialize Variable because it has an invalid Type: " +
                                              variableType);
          if(m_Member.GetVariableType().IsArray)
          {
            IList col = new List<object>();
            ReadCollection(reader, col);
            Array instance = Array.CreateInstance(m_collectionType, col.Count);
            for(int index = 0; index < col.Count; ++index)
              ArrayUtil.SetValue(instance, index, col[index]);
            Value = instance;
          }
          else
          {
            IList instance = (IList) Activator.CreateInstance(variableType);
            ReadCollection(reader, instance);
            Value = instance;
          }
        }
      }
      catch(Exception ex)
      {
        Value = obj;
        throw ex;
      }
    }

    private void ReadCollection(XmlReader reader, IList col)
    {
      while(true)
      {
        reader.Read();
        reader.SkipEmptyNodes();
        if(reader.NodeType != XmlNodeType.EndElement)
        {
          if(reader.NodeType == XmlNodeType.Element && reader.Name == "Item")
          {
            object obj = TryParse(reader.ReadString(), m_collectionType);
            if(obj != null)
              col.Add(obj);
          }

          reader.SkipEmptyNodes();
          reader.ReadEndElement();
        }
        else
          break;
      }
    }

    public virtual void WriteXml(XmlWriter writer)
    {
      if(IsReadOnly)
        throw new InvalidOperationException("Tried to write ReadOnly Variable \"" + this +
                                            "\" to XML-Stream");
      if(Value == null)
        throw new ArgumentException("Tried to write null-value to XML: " + this);
      Type variableType = m_Member.GetVariableType();
      if(m_isXmlSerializable)
        ((IXmlSerializable) Value).WriteXml(writer);
      else if(variableType.IsSimpleType())
      {
        writer.WriteString(Value.ToString());
      }
      else
      {
        if(!(m_collectionType != null))
          throw new NotImplementedException("Cannot serialize Variable because it has an invalid Type: " +
                                            variableType);
        writer.WriteCollection((IEnumerable) Value, "Item");
      }
    }

    public XmlSchema GetSchema()
    {
      throw new NotImplementedException(GetType() + " does not support any XmlSchema.");
    }

    public override string ToString()
    {
      return Name + " (" + FullName + ")";
    }
  }
}
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
            this.Name = name;
            this.Member = member;
            this.Serialized = serialized;
            this.m_readOnly = readOnly;
        }

        public TypeVariableDefinition(string name, object obj, MemberInfo member, bool serialized, bool readOnly)
            : this(name, member, serialized, readOnly)
        {
            this.Object = obj;
        }

        public string Name { get; internal set; }

        public bool IsReadOnly
        {
            get { return this.m_readOnly; }
            internal set { this.m_readOnly = value; }
        }

        public bool IsFileOnly { get; internal set; }

        public MemberInfo Member
        {
            get { return this.m_Member; }
            internal set
            {
                this.m_Member = value;
                this.FullName = this.GetSafeName();
                Type variableType = this.m_Member.GetVariableType();
                this.m_isXmlSerializable = variableType.GetInterface("IXmlSerializable") != (Type) null;
                if (!(variableType.GetInterface("IEnumerable") != (Type) null) || !(variableType != typeof(string)))
                    return;
                if (variableType.IsArray)
                {
                    this.m_collectionType = variableType.GetElementType();
                }
                else
                {
                    Type type = variableType.GetInterface(TypeVariableDefinition.GenericListType.Name);
                    if (type == (Type) null)
                        throw new Exception(
                            "Cannot create TypeVariableDefinition for IEnumerable, unless it is an Array or implements IList<T>.");
                    this.m_collectionType = ((IEnumerable<Type>) type.GetGenericArguments()).First<Type>();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetSafeName()
        {
            return this.m_Member.DeclaringType.FullName.Replace("+", ".").Replace("#", ".") + "." + this.Name;
        }

        public string FullName { get; private set; }

        public Type VariableType
        {
            get { return this.m_Member.GetVariableType(); }
        }

        public object Value
        {
            get { return this.m_Member.GetUnindexedValue(this.Object); }
            set { this.m_Member.SetUnindexedValue(this.Object, value); }
        }

        public string TypeName
        {
            get { return this.VariableType.Name; }
        }

        public bool TrySet(string strValue)
        {
            if (this.IsReadOnly)
                return false;
            object obj = TypeVariableDefinition.TryParse(strValue, this.VariableType);
            if (obj == null)
                return false;
            this.Value = obj;
            return true;
        }

        private static object TryParse(string strValue, Type type)
        {
            object obj = (object) null;
            if (StringParser.Parse(strValue, type, ref obj))
                return obj;
            return (object) null;
        }

        public int CompareTo(object obj)
        {
            if (obj is TypeVariableDefinition)
                return ((TypeVariableDefinition) obj).Name.CompareTo(this.Name);
            return -1;
        }

        public void ReadXml(XmlReader reader)
        {
            object obj = this.Value;
            try
            {
                Type variableType = this.m_Member.GetVariableType();
                if (this.m_isXmlSerializable)
                {
                    if (this.Value == null)
                        this.Value = Activator.CreateInstance(variableType);
                    ((IXmlSerializable) this.Value).ReadXml(reader);
                }
                else if (variableType.IsSimpleType())
                {
                    this.TrySet(reader.ReadString());
                }
                else
                {
                    if (!(this.m_collectionType != (Type) null))
                        throw new NotImplementedException("Cannot serialize Variable because it has an invalid Type: " +
                                                          (object) variableType);
                    if (this.m_Member.GetVariableType().IsArray)
                    {
                        IList col = (IList) new List<object>();
                        this.ReadCollection(reader, col);
                        Array instance = Array.CreateInstance(this.m_collectionType, col.Count);
                        for (int index = 0; index < col.Count; ++index)
                            ArrayUtil.SetValue(instance, index, col[index]);
                        this.Value = (object) instance;
                    }
                    else
                    {
                        IList instance = (IList) Activator.CreateInstance(variableType);
                        this.ReadCollection(reader, instance);
                        this.Value = (object) instance;
                    }
                }
            }
            catch (Exception ex)
            {
                this.Value = obj;
                throw ex;
            }
        }

        private void ReadCollection(XmlReader reader, IList col)
        {
            while (true)
            {
                reader.Read();
                reader.SkipEmptyNodes();
                if (reader.NodeType != XmlNodeType.EndElement)
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Item")
                    {
                        object obj = TypeVariableDefinition.TryParse(reader.ReadString(), this.m_collectionType);
                        if (obj != null)
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
            if (this.IsReadOnly)
                throw new InvalidOperationException("Tried to write ReadOnly Variable \"" + (object) this +
                                                    "\" to XML-Stream");
            if (this.Value == null)
                throw new ArgumentException("Tried to write null-value to XML: " + (object) this);
            Type variableType = this.m_Member.GetVariableType();
            if (this.m_isXmlSerializable)
                ((IXmlSerializable) this.Value).WriteXml(writer);
            else if (variableType.IsSimpleType())
            {
                writer.WriteString(this.Value.ToString());
            }
            else
            {
                if (!(this.m_collectionType != (Type) null))
                    throw new NotImplementedException("Cannot serialize Variable because it has an invalid Type: " +
                                                      (object) variableType);
                writer.WriteCollection((IEnumerable) this.Value, "Item");
            }
        }

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException(this.GetType().ToString() + " does not support any XmlSchema.");
        }

        public override string ToString()
        {
            return this.Name + " (" + this.FullName + ")";
        }
    }
}
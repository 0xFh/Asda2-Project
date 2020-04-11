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
            (IDictionary<string, StringNode<V>>) new SortedList<string, StringNode<V>>();

        protected string m_Name;
        protected V m_Value;
        protected StringTree<V> m_tree;
        protected StringNode<V> m_Parent;
        protected int m_depth;
        protected string m_indent;

        protected internal StringNode(StringTree<V> tree)
        {
            this.m_tree = tree;
        }

        protected internal StringNode(StringTree<V> tree, string key, V value)
            : this(tree)
        {
            this.m_Name = key;
            this.m_Value = value;
        }

        public string Name
        {
            get { return this.m_Name; }
        }

        public string FullName
        {
            get
            {
                if (this.m_Parent == null || string.IsNullOrEmpty(this.m_Parent.Name))
                    return this.m_Name;
                return this.m_Parent.FullName + (object) this.m_tree.Seperators[0] + this.m_Name;
            }
        }

        public V Value
        {
            get { return this.m_Value; }
            set { this.m_Value = value; }
        }

        public int ChildCount
        {
            get { return this.Children.Count; }
        }

        public StringNode<V> Parent
        {
            get { return this.m_Parent; }
        }

        public string GetQualifiedName(string name)
        {
            return (this.FullName != null ? this.FullName + (object) this.m_tree.Seperators[0] : "") + name;
        }

        public StringNode<V> GetChild(string key)
        {
            StringNode<V> stringNode;
            this.Children.TryGetValue(key, out stringNode);
            return stringNode;
        }

        public StringNode<V> FindChild(string keyChain)
        {
            return this.FindChild(keyChain.Split(this.m_tree.Seperators, StringSplitOptions.RemoveEmptyEntries), 0);
        }

        public StringNode<V> FindChild(string[] keyChain)
        {
            return this.FindChild(keyChain, 0);
        }

        public StringNode<V> FindChild(string[] keyChain, int index)
        {
            if (index >= keyChain.Length)
                return this;
            return this.GetChild(keyChain[index])?.FindChild(keyChain, index + 1);
        }

        public StringNode<V> GetOrCreate(string key)
        {
            return this.GetChild(key) ?? this.AddChild(key, default(V));
        }

        public V GetValue(string key)
        {
            StringNode<V> child = this.GetChild(key);
            return child != null ? child.Value : default(V);
        }

        public V FindValue(string keyChain)
        {
            StringNode<V> child = this.FindChild(keyChain);
            return child != null ? child.Value : default(V);
        }

        public V FindValue(string[] keyChain)
        {
            StringNode<V> child = this.FindChild(keyChain);
            return child != null ? child.Value : default(V);
        }

        public V FindValue(string[] keyChain, int index)
        {
            StringNode<V> child = this.FindChild(keyChain, index);
            return child != null ? child.Value : default(V);
        }

        public StringNode<V> AddChild(string key, V value)
        {
            StringNode<V> child = new StringNode<V>(this.m_tree, key, value);
            this.AddChild(child);
            return child;
        }

        public StringNode<V> AddChildInChain(string keyChain, V value)
        {
            return this.AddChildInChain(keyChain.Split(this.m_tree.Seperators, StringSplitOptions.RemoveEmptyEntries),
                value);
        }

        public StringNode<V> AddChildInChain(string[] keyChain, V value)
        {
            StringNode<V> stringNode = this;
            for (int index = 0; index < keyChain.Length; ++index)
            {
                string key = keyChain[index];
                stringNode = stringNode.GetOrCreate(key);
                if (index == keyChain.Length - 1)
                    stringNode.Value = value;
            }

            return stringNode;
        }

        public void AddChild(StringNode<V> child)
        {
            this.Children.Add(child.Name, child);
            child.m_Parent = this;
            child.m_depth = this.m_depth + 1;
            child.m_indent = this.m_indent + "\t";
        }

        public void Remove()
        {
            if (this.m_Parent == null)
                throw new InvalidOperationException("Cannot remove the Root of a Tree.");
            this.m_Parent.Children.Remove(this.m_Name);
            this.m_Parent = (StringNode<V>) null;
            this.m_depth = 0;
        }

        /// <summary>
        /// Removes and returns the direct Child with the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public StringNode<V> RemoveChild(string key)
        {
            StringNode<V> child = this.GetChild(key);
            if (child != null)
                child.Remove();
            return child;
        }

        /// <summary>
        /// Removes and returns the direct Child with the given key
        /// </summary>
        /// <returns></returns>
        public StringNode<V> FindAndRemoveChild(string keyChain)
        {
            StringNode<V> child = this.FindChild(keyChain);
            if (child != null)
                child.Remove();
            return child;
        }

        /// <summary>
        /// Removes and returns the direct Child with the given key
        /// </summary>
        /// <returns></returns>
        public StringNode<V> FindAndRemoveChild(string[] keyChain)
        {
            StringNode<V> child = this.FindChild(keyChain);
            if (child != null)
                child.Remove();
            return child;
        }

        /// <summary>
        /// Removes and returns the direct Child with the given key
        /// </summary>
        /// <returns></returns>
        public StringNode<V> FindAndRemoveChild(string[] keyChain, int index)
        {
            StringNode<V> child = this.FindChild(keyChain, index);
            if (child != null)
                child.Remove();
            return child;
        }

        public void ReadXml(XmlReader reader)
        {
            if ((object) this.Value != null)
            {
                try
                {
                    this.Value.ReadXml(reader);
                }
                catch (Exception ex)
                {
                    LogUtil.ErrorException(ex, string.Format("Failed to parse Node: {0}", (object) this.FullName),
                        new object[0]);
                }
            }

            if (this.Children.Count > 0)
            {
                List<StringNode<V>> stringNodeList = new List<StringNode<V>>();
                int num = 0;
                while (num < this.Children.Count)
                {
                    reader.Read();
                    if (reader.ReadState == ReadState.EndOfFile)
                        throw new Exception("Unexpected EOF in Config.");
                    reader.SkipEmptyNodes();
                    if (reader.NodeType != XmlNodeType.EndElement)
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            StringNode<V> child = this.GetChild(reader.Name);
                            if (child == null)
                            {
                                this.OnInvalidNode(reader);
                            }
                            else
                            {
                                ++num;
                                stringNodeList.Add(child);
                                if (!reader.IsEmptyElement)
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
                    this.Children.Values.Except<StringNode<V>>((IEnumerable<StringNode<V>>) stringNodeList);
                if (stringNodes.Count<StringNode<V>>() > 0)
                    this.m_tree.OnError("Found {0} missing Node(s): {1}", (object) stringNodes.Count<StringNode<V>>(),
                        (object) stringNodes.ToString<StringNode<V>>(", ",
                            (Func<StringNode<V>, object>) (node => (object) node.FullName)));
            }

            reader.SkipEmptyNodes();
            if (reader.IsEmptyElement)
                reader.Read();
            reader.SkipEmptyNodes();
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                this.OnInvalidNode(reader);
                reader.SkipEmptyNodes();
            }

            reader.ReadEndElement();
        }

        private void OnInvalidNode(XmlReader reader)
        {
            this.m_tree.OnError("Found invalid Node \"{0}\"", (object) this.GetQualifiedName(reader.Name));
            if (reader.IsEmptyElement)
                return;
            int num = 1;
            do
            {
                reader.Read();
                if (reader.ReadState == ReadState.EndOfFile)
                    throw new Exception("Unexpected EOF in Config.");
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    reader.ReadEndElement();
                    --num;
                }
                else if (reader.NodeType == XmlNodeType.Element && !reader.IsEmptyElement)
                    ++num;
            } while (num > 0);
        }

        public void WriteXml(XmlWriter writer)
        {
            if ((object) this.Value != null)
                this.Value.WriteXml(writer);
            if (this.Children.Count <= 0)
                return;
            foreach (StringNode<V> stringNode in (IEnumerable<StringNode<V>>) this.Children.Values)
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
            return this.FullName;
        }
    }
}
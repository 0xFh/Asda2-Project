using System;
using System.Xml.Serialization;

namespace WCell.RealmServer.Instances
{
    public class InstanceConfigEntry<E> : IComparable where E : IComparable
    {
        private string m_TypeName;

        public InstanceConfigEntry()
        {
            this.TypeName = " ";
        }

        public InstanceConfigEntry(E id)
            : this(id, " ")
        {
        }

        public InstanceConfigEntry(E id, string typeName)
        {
            this.Name = id;
            this.TypeName = typeName;
        }

        [XmlElement("Name")] public E Name { get; set; }

        [XmlElement("Type")]
        public string TypeName
        {
            get { return this.m_TypeName; }
            set { this.m_TypeName = value; }
        }

        public int CompareTo(object obj)
        {
            InstanceConfigEntry<E> instanceConfigEntry = obj as InstanceConfigEntry<E>;
            if (instanceConfigEntry != null)
                return this.Name.CompareTo((object) instanceConfigEntry.Name);
            return -1;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WCell.Util.DB;

namespace WCell.Util.Data
{
    public abstract class DataField : DataFieldBase, IDataField, IDataFieldBase
    {
        protected MemberInfo m_mappedMember;
        protected string m_name;
        protected string m_fullName;

        protected DataField(DataHolderDefinition dataHolder, string name, IGetterSetter accessor,
            MemberInfo mappedMember, INestedDataField parent)
            : base(dataHolder, accessor, parent)
        {
            this.m_mappedMember = mappedMember;
            this.m_name = name;
        }

        public string Name
        {
            get { return this.m_name; }
        }

        public string FullName
        {
            get { return this.m_fullName; }
        }

        public MemberInfo MappedMember
        {
            get { return this.m_mappedMember; }
        }

        public Type ActualMemberType
        {
            get
            {
                PersistentAttribute persistentAttribute =
                    (PersistentAttribute) ((IEnumerable<object>) this.m_mappedMember.GetCustomAttributes(
                        typeof(PersistentAttribute), true)).FirstOrDefault<object>();
                if (persistentAttribute != null && persistentAttribute.ActualType != (Type) null)
                    return persistentAttribute.ActualType;
                return this.m_mappedMember.GetActualType();
            }
        }

        public abstract DataFieldType DataFieldType { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootObject"></param>
        /// <returns></returns>
        public object GetTargetObject(IDataHolder rootObject)
        {
            if (this.m_parent == null)
                return (object) rootObject;
            object obj = this.m_parent.Accessor.Get(this.m_parent.GetTargetObject(rootObject));
            if (obj == null)
            {
                obj = this.m_parent.Producer.Produce();
                this.m_parent.Accessor.Set((object) rootObject, obj);
            }

            return obj;
        }

        public override string ToString()
        {
            return this.m_name;
        }
    }
}
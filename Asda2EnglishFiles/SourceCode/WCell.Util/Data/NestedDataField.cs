using System;
using System.Collections.Generic;
using System.Reflection;

namespace WCell.Util.Data
{
    public abstract class NestedDataField : DataField, INestedDataField, IDataFieldBase
    {
        protected readonly Dictionary<string, IDataField> m_innerFields =
            new Dictionary<string, IDataField>((IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);

        protected readonly IProducer m_Producer;

        protected NestedDataField(DataHolderDefinition dataHolder, string name, IGetterSetter accessor,
            MemberInfo mappedMember, IProducer producer, INestedDataField parent)
            : base(dataHolder, name, accessor, mappedMember, parent)
        {
            this.m_Producer = producer;
        }

        public Dictionary<string, IDataField> InnerFields
        {
            get { return this.m_innerFields; }
        }

        public MemberInfo BelongsTo
        {
            get { return this.MappedMember; }
        }

        public IProducer Producer
        {
            get { return this.m_Producer; }
        }
    }
}
using System.Collections.Generic;
using System.Reflection;
using WCell.Util.DB;

namespace WCell.Util.Data
{
    public class NestedArrayDataField : ArrayDataField, INestedDataField, IDataFieldBase
    {
        private readonly IProducer m_Producer;

        public NestedArrayDataField(DataHolderDefinition dataHolder, string name, IGetterSetter accessor,
            MemberInfo mappedMember, IProducer producer, IProducer arrayProducer, int length, INestedDataField parent)
            : base(dataHolder, name, accessor, mappedMember, parent, length, arrayProducer)
        {
            this.m_Producer = producer;
            this.m_ArrayAccessors = (IDataFieldAccessor[]) new NestedArrayAccessor[this.m_length];
            for (int index = 0; index < this.m_length; ++index)
                this.m_ArrayAccessors[index] = (IDataFieldAccessor) new NestedArrayAccessor(this, index);
        }

        public IProducer Producer
        {
            get { return this.m_Producer; }
        }

        public Dictionary<string, IDataField> InnerFields
        {
            get { return ((NestedArrayAccessor) this.m_ArrayAccessors[0]).InnerFields; }
        }

        public MemberInfo BelongsTo
        {
            get { return this.MappedMember; }
        }

        public override DataFieldType DataFieldType
        {
            get { return DataFieldType.NestedArray; }
        }

        public override IDataField Copy(INestedDataField parent)
        {
            return (IDataField) new NestedArrayDataField(this.m_DataHolderDefinition, this.m_name, this.m_accessor,
                this.m_mappedMember, this.m_Producer, this.m_arrProducer, this.m_length, parent);
        }
    }
}
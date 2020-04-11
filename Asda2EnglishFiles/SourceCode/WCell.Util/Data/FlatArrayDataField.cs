using System.Reflection;
using WCell.Util.DB;

namespace WCell.Util.Data
{
    public class FlatArrayDataField : ArrayDataField
    {
        public FlatArrayDataField(DataHolderDefinition dataHolder, string name, IGetterSetter accessor,
            MemberInfo mappedMember, int length, IProducer arrProducer, INestedDataField parent)
            : base(dataHolder, name, accessor, mappedMember, parent, length, arrProducer)
        {
            this.m_ArrayAccessors = (IDataFieldAccessor[]) new FlatArrayAccessor[this.m_length];
            for (int index = 0; index < this.m_length; ++index)
                this.m_ArrayAccessors[index] = (IDataFieldAccessor) new FlatArrayAccessor(this, index);
        }

        public override DataFieldType DataFieldType
        {
            get { return DataFieldType.FlatArray; }
        }

        public override IDataField Copy(INestedDataField parent)
        {
            return (IDataField) new FlatArrayDataField(this.m_DataHolderDefinition, this.m_name, this.m_accessor,
                this.m_mappedMember, this.m_length, this.m_arrProducer, parent);
        }
    }
}
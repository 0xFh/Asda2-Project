using System.Collections;
using System.Reflection;
using WCell.Util.DB;

namespace WCell.Util.Data
{
    public class NestedSimpleDataField : NestedDataField
    {
        public NestedSimpleDataField(DataHolderDefinition dataHolder, string name, IGetterSetter accessor,
            MemberInfo mappedMember, IProducer producer, INestedDataField parent)
            : base(dataHolder, name, accessor, mappedMember, producer, parent)
        {
        }

        public IEnumerator GetEnumerator()
        {
            return (IEnumerator) this.m_innerFields.Values.GetEnumerator();
        }

        public override DataFieldType DataFieldType
        {
            get { return DataFieldType.NestedSimple; }
        }

        public override IDataField Copy(INestedDataField parent)
        {
            return (IDataField) new NestedSimpleDataField(this.m_DataHolderDefinition, this.m_name, this.m_accessor,
                this.m_mappedMember, this.m_Producer, parent);
        }
    }
}
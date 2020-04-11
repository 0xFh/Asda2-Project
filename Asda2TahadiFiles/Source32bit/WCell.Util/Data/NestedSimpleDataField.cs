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
      return m_innerFields.Values.GetEnumerator();
    }

    public override DataFieldType DataFieldType
    {
      get { return DataFieldType.NestedSimple; }
    }

    public override IDataField Copy(INestedDataField parent)
    {
      return new NestedSimpleDataField(m_DataHolderDefinition, m_name, m_accessor,
        m_mappedMember, m_Producer, parent);
    }
  }
}
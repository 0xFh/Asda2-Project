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
      m_ArrayAccessors = new FlatArrayAccessor[m_length];
      for(int index = 0; index < m_length; ++index)
        m_ArrayAccessors[index] = new FlatArrayAccessor(this, index);
    }

    public override DataFieldType DataFieldType
    {
      get { return DataFieldType.FlatArray; }
    }

    public override IDataField Copy(INestedDataField parent)
    {
      return new FlatArrayDataField(m_DataHolderDefinition, m_name, m_accessor,
        m_mappedMember, m_length, m_arrProducer, parent);
    }
  }
}
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
      m_Producer = producer;
      m_ArrayAccessors = new NestedArrayAccessor[m_length];
      for(int index = 0; index < m_length; ++index)
        m_ArrayAccessors[index] = new NestedArrayAccessor(this, index);
    }

    public IProducer Producer
    {
      get { return m_Producer; }
    }

    public Dictionary<string, IDataField> InnerFields
    {
      get { return ((NestedArrayAccessor) m_ArrayAccessors[0]).InnerFields; }
    }

    public MemberInfo BelongsTo
    {
      get { return MappedMember; }
    }

    public override DataFieldType DataFieldType
    {
      get { return DataFieldType.NestedArray; }
    }

    public override IDataField Copy(INestedDataField parent)
    {
      return new NestedArrayDataField(m_DataHolderDefinition, m_name, m_accessor,
        m_mappedMember, m_Producer, m_arrProducer, m_length, parent);
    }
  }
}
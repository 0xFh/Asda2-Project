using System;
using System.Collections.Generic;
using System.Reflection;

namespace WCell.Util.Data
{
  public class NestedArrayAccessor : INestedDataField, IDataFieldBase, IGetterSetter, IDataFieldAccessor
  {
    private readonly Dictionary<string, IDataField> m_innerFields =
      new Dictionary<string, IDataField>(StringComparer.InvariantCultureIgnoreCase);

    private readonly NestedArrayDataField m_ArrayField;
    private readonly int m_Index;

    public NestedArrayAccessor(NestedArrayDataField arrayField, int index)
    {
      m_ArrayField = arrayField;
      m_Index = index;
    }

    public Dictionary<string, IDataField> InnerFields
    {
      get { return m_innerFields; }
    }

    public MemberInfo BelongsTo
    {
      get { return m_ArrayField.MappedMember; }
    }

    public int Index
    {
      get { return m_Index; }
    }

    public NestedArrayDataField ArrayField
    {
      get { return m_ArrayField; }
    }

    public INestedDataField Parent
    {
      get { return m_ArrayField.Parent; }
    }

    public IGetterSetter Accessor
    {
      get { return this; }
    }

    public DataHolderDefinition DataHolderDefinition
    {
      get { return m_ArrayField.DataHolderDefinition; }
    }

    public IProducer Producer
    {
      get { return m_ArrayField.Producer; }
    }

    public object GetTargetObject(IDataHolder rootObject)
    {
      return m_ArrayField.GetTargetObject(rootObject);
    }

    public override string ToString()
    {
      return m_ArrayField.Name + "[" + m_Index + "]";
    }

    public object Get(object arrayContainer)
    {
      Array array = m_ArrayField.GetArray(arrayContainer);
      object obj = array.GetValue(m_Index);
      if(obj == null)
      {
        obj = m_ArrayField.Producer.Produce();
        ArrayUtil.SetValue(array, m_Index, obj);
      }

      return obj;
    }

    public void Set(object arrayContainer, object value)
    {
      m_ArrayField.Set(arrayContainer, m_Index, value);
    }
  }
}
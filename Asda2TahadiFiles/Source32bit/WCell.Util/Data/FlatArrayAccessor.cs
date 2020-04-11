namespace WCell.Util.Data
{
  public class FlatArrayAccessor : IFlatDataFieldAccessor, IDataFieldAccessor
  {
    private readonly FlatArrayDataField m_ArrayField;
    private readonly int m_Index;

    public FlatArrayAccessor(FlatArrayDataField arrayField, int index)
    {
      m_ArrayField = arrayField;
      m_Index = index;
    }

    public int Index
    {
      get { return m_Index; }
    }

    public FlatArrayDataField ArrayField
    {
      get { return m_ArrayField; }
    }

    public DataHolderDefinition DataHolderDefinition
    {
      get { return m_ArrayField.DataHolderDefinition; }
    }

    public object Get(IDataHolder obj)
    {
      return m_ArrayField.Get(m_ArrayField.GetTargetObject(obj), m_Index);
    }

    public void Set(IDataHolder obj, object value)
    {
      m_ArrayField.Set(m_ArrayField.GetTargetObject(obj), m_Index, value);
    }

    public override string ToString()
    {
      return m_ArrayField.Name + "[" + m_Index + "]";
    }
  }
}
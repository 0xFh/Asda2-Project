namespace WCell.Util.Data
{
  public abstract class DataFieldBase : IDataFieldBase
  {
    protected DataHolderDefinition m_DataHolderDefinition;
    protected IGetterSetter m_accessor;
    protected INestedDataField m_parent;

    protected DataFieldBase(DataHolderDefinition dataHolder, IGetterSetter accessor, INestedDataField parent)
    {
      m_DataHolderDefinition = dataHolder;
      m_parent = parent;
      m_accessor = accessor;
    }

    public INestedDataField Parent
    {
      get { return m_parent; }
      internal set { m_parent = value; }
    }

    public DataHolderDefinition DataHolderDefinition
    {
      get { return m_DataHolderDefinition; }
    }

    public IGetterSetter Accessor
    {
      get { return m_accessor; }
    }

    public abstract IDataField Copy(INestedDataField parent);
  }
}
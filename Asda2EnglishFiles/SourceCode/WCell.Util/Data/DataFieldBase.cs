namespace WCell.Util.Data
{
    public abstract class DataFieldBase : IDataFieldBase
    {
        protected DataHolderDefinition m_DataHolderDefinition;
        protected IGetterSetter m_accessor;
        protected INestedDataField m_parent;

        protected DataFieldBase(DataHolderDefinition dataHolder, IGetterSetter accessor, INestedDataField parent)
        {
            this.m_DataHolderDefinition = dataHolder;
            this.m_parent = parent;
            this.m_accessor = accessor;
        }

        public INestedDataField Parent
        {
            get { return this.m_parent; }
            internal set { this.m_parent = value; }
        }

        public DataHolderDefinition DataHolderDefinition
        {
            get { return this.m_DataHolderDefinition; }
        }

        public IGetterSetter Accessor
        {
            get { return this.m_accessor; }
        }

        public abstract IDataField Copy(INestedDataField parent);
    }
}
namespace WCell.Util.Data
{
    public class DataFieldProxy : IFlatDataFieldAccessor, IDataFieldAccessor
    {
        private readonly string m_FieldName;
        private readonly DataHolderDefinition m_DataHolderDef;
        private IDataField m_field;

        public DataFieldProxy(string fieldName, DataHolderDefinition dataHolderDef)
        {
            this.m_FieldName = fieldName;
            this.m_DataHolderDef = dataHolderDef;
        }

        public string FieldName
        {
            get { return this.m_FieldName; }
        }

        public IDataField Field
        {
            get
            {
                if (this.m_field == null)
                    this.m_field = this.m_DataHolderDef.GetField(this.m_FieldName);
                return this.m_field;
            }
        }

        public DataHolderDefinition DataHolderDefinition
        {
            get { return this.m_DataHolderDef; }
        }

        public object Get(IDataHolder obj)
        {
            if (this.Field == null)
                return (object) null;
            return this.Field.Accessor.Get((object) obj);
        }

        public void Set(IDataHolder obj, object value)
        {
        }

        public override string ToString()
        {
            return "Proxy for: " + (object) this.m_DataHolderDef;
        }
    }
}
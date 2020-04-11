namespace WCell.Util.Data
{
    public class FlatArrayAccessor : IFlatDataFieldAccessor, IDataFieldAccessor
    {
        private readonly FlatArrayDataField m_ArrayField;
        private readonly int m_Index;

        public FlatArrayAccessor(FlatArrayDataField arrayField, int index)
        {
            this.m_ArrayField = arrayField;
            this.m_Index = index;
        }

        public int Index
        {
            get { return this.m_Index; }
        }

        public FlatArrayDataField ArrayField
        {
            get { return this.m_ArrayField; }
        }

        public DataHolderDefinition DataHolderDefinition
        {
            get { return this.m_ArrayField.DataHolderDefinition; }
        }

        public object Get(IDataHolder obj)
        {
            return this.m_ArrayField.Get(this.m_ArrayField.GetTargetObject(obj), this.m_Index);
        }

        public void Set(IDataHolder obj, object value)
        {
            this.m_ArrayField.Set(this.m_ArrayField.GetTargetObject(obj), this.m_Index, value);
        }

        public override string ToString()
        {
            return this.m_ArrayField.Name + "[" + (object) this.m_Index + "]";
        }
    }
}
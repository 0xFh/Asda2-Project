namespace WCell.Util.Data
{
    public interface IFlatDataFieldAccessor : IDataFieldAccessor
    {
        object Get(IDataHolder obj);

        void Set(IDataHolder obj, object value);
    }
}
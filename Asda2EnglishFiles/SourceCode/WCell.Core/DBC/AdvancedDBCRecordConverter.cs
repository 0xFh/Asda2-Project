namespace WCell.Core.DBC
{
    public class AdvancedDBCRecordConverter<T> : DBCRecordConverter
    {
        public virtual T ConvertTo(byte[] rawData, ref int id)
        {
            id = int.MinValue;
            return default(T);
        }
    }
}
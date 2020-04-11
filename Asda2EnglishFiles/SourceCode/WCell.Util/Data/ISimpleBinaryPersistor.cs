namespace WCell.Util.Data
{
    public interface ISimpleBinaryPersistor : IBinaryPersistor
    {
        int BinaryLength { get; }
    }
}
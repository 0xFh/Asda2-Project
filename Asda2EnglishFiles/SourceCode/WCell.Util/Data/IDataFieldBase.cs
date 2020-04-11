namespace WCell.Util.Data
{
    public interface IDataFieldBase
    {
        INestedDataField Parent { get; }

        DataHolderDefinition DataHolderDefinition { get; }
    }
}
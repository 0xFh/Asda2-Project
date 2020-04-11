namespace WCell.Util.Variables
{
    public interface IVariableDefinition
    {
        string Name { get; }

        bool IsReadOnly { get; }

        bool IsFileOnly { get; }

        string TypeName { get; }

        object Value { get; set; }
    }
}
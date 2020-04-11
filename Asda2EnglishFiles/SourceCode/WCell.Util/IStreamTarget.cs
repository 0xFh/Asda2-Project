namespace WCell.Util
{
    public interface IStreamTarget
    {
        string Name { get; set; }

        IndentTextWriter Writer { get; }

        void Open();

        void Close();

        void Flush();
    }
}
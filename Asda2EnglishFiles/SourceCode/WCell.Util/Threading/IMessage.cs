namespace WCell.Util.Threading
{
    /// <summary>Defines the interface of a message.</summary>
    public interface IMessage
    {
        /// <summary>Executes the message.</summary>
        void Execute();
    }
}
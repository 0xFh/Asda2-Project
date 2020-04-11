namespace WCell.RealmServer.Chat
{
    /// <summary>Defines an object that can accept simple messages.</summary>
    public interface IGenericChatTarget
    {
        /// <summary>Sends a message to the target.</summary>
        /// <param name="message">the message to send</param>
        void SendMessage(string message);
    }
}
namespace WCell.RealmServer.Chat
{
    /// <summary>
    /// Defines an object that can accept messages from other players in different languages.
    /// </summary>
    public interface IChatTarget : IGenericChatTarget
    {
        /// <summary>Send a message to the target.</summary>
        /// <param name="sender">the target of the message</param>
        /// <param name="message">the message to send</param>
        void SendMessage(IChatter sender, string message);
    }
}
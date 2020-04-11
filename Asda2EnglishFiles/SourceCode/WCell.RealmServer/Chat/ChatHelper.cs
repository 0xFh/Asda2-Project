namespace WCell.RealmServer.Chat
{
    /// <summary>
    /// Helper class for chat-related extension methods and other misc. methods.
    /// </summary>
    public static class ChatHelper
    {
        /// <summary>Sends a system message to the target.</summary>
        /// <param name="target">the target being sent a system message</param>
        /// <param name="msg">the message to send</param>
        /// <param name="args">any arguments to be formatted in the message</param>
        public static void SendMessage(this IGenericChatTarget target, string msg, params object[] args)
        {
            target.SendMessage(string.Format(msg, args));
        }
    }
}
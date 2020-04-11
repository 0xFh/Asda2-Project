using System;

namespace WCell.Util.Threading
{
    /// <summary>Defines a message with no input parameters.</summary>
    public class Message : IMessage
    {
        /// <summary>
        /// Returns a recycled or new Message object with the given callback.
        /// TODO: Object recycling
        /// </summary>
        public static Message Obtain(Action callback)
        {
            return new Message(callback);
        }

        /// <summary>Default constructor.</summary>
        public Message()
        {
        }

        /// <summary>Constructs a message with the specific callback.</summary>
        /// <param name="callback">the callback to invoke when the message is executed</param>
        public Message(Action callback)
        {
            this.Callback = callback;
        }

        /// <summary>
        /// The callback that is called when the message is executed.
        /// </summary>
        public Action Callback { get; private set; }

        /// <summary>
        /// Executes the message, calling any callbacks that are bound.
        /// </summary>
        public virtual void Execute()
        {
            Action callback = this.Callback;
            if (callback == null)
                return;
            callback();
        }

        public static implicit operator Message(Action dele)
        {
            return new Message(dele);
        }

        public override string ToString()
        {
            return this.Callback.ToString();
        }
    }
}
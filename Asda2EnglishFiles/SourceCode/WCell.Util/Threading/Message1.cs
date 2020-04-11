using System;

namespace WCell.Util.Threading
{
    /// <summary>Defines a message with one input parameter.</summary>
    /// <typeparam name="T1">the type of the first input parameter</typeparam>
    public class Message1<T1> : IMessage
    {
        /// <summary>Default constructor.</summary>
        public Message1()
        {
        }

        /// <summary>Constructs a message with the specific callback.</summary>
        /// <param name="callback">the callback to invoke when the message is executed</param>
        public Message1(Action<T1> callback)
        {
            this.Callback = callback;
        }

        /// <summary>
        /// Constructs a message with the specific callback and input parameter.
        /// </summary>
        /// <param name="callback">the callback to invoke when the message is executed</param>
        /// <param name="param1">the first input parameter</param>
        public Message1(T1 param1, Action<T1> callback)
        {
            this.Callback = callback;
            this.Parameter1 = param1;
        }

        /// <summary>
        /// The callback that is called when the message is executed.
        /// </summary>
        public Action<T1> Callback { get; set; }

        /// <summary>The first input parameter.</summary>
        public T1 Parameter1 { get; set; }

        /// <summary>
        /// Executes the message, calling any callbacks that are bound, passing the given input parameters.
        /// </summary>
        public virtual void Execute()
        {
            Action<T1> callback = this.Callback;
            if (callback == null)
                return;
            callback(this.Parameter1);
        }

        public static explicit operator Message1<T1>(Action<T1> dele)
        {
            return new Message1<T1>(dele);
        }
    }
}
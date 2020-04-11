using System;

namespace WCell.Util.Threading
{
    public class Message2<T1, T2> : IMessage
    {
        /// <summary>Default constructor.</summary>
        public Message2()
        {
        }

        /// <summary>Constructs a message with the specific callback.</summary>
        /// <param name="callback">the callback to invoke when the message is executed</param>
        public Message2(Action<T1, T2> callback)
        {
            this.Callback = callback;
        }

        /// <summary>
        /// Constructs a message with the specific callback and input parameters.
        /// </summary>
        /// <param name="callback">the callback to invoke when the message is executed</param>
        /// <param name="param1">the first input parameter</param>
        /// <param name="param2">the second input parameter</param>
        public Message2(T1 param1, T2 param2, Action<T1, T2> callback)
        {
            this.Callback = callback;
            this.Parameter1 = param1;
            this.Parameter2 = param2;
        }

        /// <summary>
        /// Constructs a message with the specific callback and input parameters.
        /// </summary>
        /// <param name="callback">the callback to invoke when the message is executed</param>
        /// <param name="param1">the first input parameter</param>
        /// <param name="param2">the second input parameter</param>
        public Message2(T1 param1, T2 param2)
        {
            this.Parameter1 = param1;
            this.Parameter2 = param2;
        }

        /// <summary>
        /// The callback that is called when the message is executed.
        /// </summary>
        public Action<T1, T2> Callback { get; set; }

        /// <summary>The first input parameter.</summary>
        public T1 Parameter1 { get; set; }

        /// <summary>The second input parameter.</summary>
        public T2 Parameter2 { get; set; }

        /// <summary>
        /// Executes the message, calling any callbacks that are bound, passing the given input parameters.
        /// </summary>
        public virtual void Execute()
        {
            Action<T1, T2> callback = this.Callback;
            if (callback == null)
                return;
            callback(this.Parameter1, this.Parameter2);
        }

        public static explicit operator Message2<T1, T2>(Action<T1, T2> dele)
        {
            return new Message2<T1, T2>(dele);
        }
    }
}
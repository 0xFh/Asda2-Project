using System;

namespace WCell.Util.Threading
{
    public class Message4<T1, T2, T3, T4> : IMessage
    {
        /// <summary>Default constructor.</summary>
        public Message4()
        {
        }

        /// <summary>Constructs a message with the specific callback.</summary>
        /// <param name="callback">the callback to invoke when the message is executed</param>
        public Message4(Action<T1, T2, T3, T4> callback)
        {
            this.Callback = callback;
        }

        /// <summary>
        /// Constructs a message with the specific callback and input parameters.
        /// </summary>
        /// <param name="callback">the callback to invoke when the message is executed</param>
        /// <param name="param1">the first input parameter</param>
        /// <param name="param2">the second input parameter</param>
        /// <param name="param3">the third input parameter</param>
        /// <param name="param4">the fourth input parameter</param>
        public Message4(Action<T1, T2, T3, T4> callback, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            this.Callback = callback;
            this.Parameter1 = param1;
            this.Parameter2 = param2;
            this.Parameter3 = param3;
            this.Parameter4 = param4;
        }

        /// <summary>
        /// The callback that is called when the message is executed.
        /// </summary>
        public Action<T1, T2, T3, T4> Callback { get; set; }

        /// <summary>The first input parameter.</summary>
        public T1 Parameter1 { get; set; }

        /// <summary>The second input parameter.</summary>
        public T2 Parameter2 { get; set; }

        /// <summary>The third input parameter.</summary>
        public T3 Parameter3 { get; set; }

        /// <summary>The fourth input parameter.</summary>
        public T4 Parameter4 { get; set; }

        /// <summary>
        /// Executes the message, calling any callbacks that are bound, passing the given input parameters.
        /// </summary>
        public virtual void Execute()
        {
            Action<T1, T2, T3, T4> callback = this.Callback;
            if (callback == null)
                return;
            callback(this.Parameter1, this.Parameter2, this.Parameter3, this.Parameter4);
        }

        public static explicit operator Message4<T1, T2, T3, T4>(Action<T1, T2, T3, T4> callback)
        {
            return new Message4<T1, T2, T3, T4>(callback);
        }
    }
}
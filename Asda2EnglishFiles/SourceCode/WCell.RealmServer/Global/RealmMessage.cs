using System;

namespace WCell.RealmServer.Global
{
    /// <summary>Defines a message with no input parameters.</summary>
    public class RealmMessage : IRealmMessage
    {
        /// <summary>
        /// Returns a recycled or new RealmMessage object with the given callback.
        /// TODO: Object recycling
        /// </summary>
        public static RealmMessage Obtain(Action callback)
        {
            return new RealmMessage(callback);
        }

        /// <summary>Default constructor.</summary>
        public RealmMessage()
        {
        }

        /// <summary>Constructs a message with the specific callback.</summary>
        /// <param name="callback">the callback to invoke when the message is executed</param>
        public RealmMessage(Action callback)
            : this(callback, RealmMessageBoundary.Global)
        {
        }

        /// <summary>Constructs a message with the specific callback.</summary>
        /// <param name="callback">the callback to invoke when the message is executed</param>
        public RealmMessage(Action callback, RealmMessageBoundary boundary)
        {
            this.Callback = callback;
            this.Boundary = boundary;
        }

        public RealmMessageBoundary Boundary { get; private set; }

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

        public static implicit operator RealmMessage(Action dele)
        {
            return new RealmMessage(dele);
        }
    }
}
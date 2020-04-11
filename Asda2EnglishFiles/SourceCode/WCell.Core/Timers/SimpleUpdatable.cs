using System;

namespace WCell.Core.Timers
{
    /// <summary>
    /// A simple wrapper that will execute a callback every time it is updated.
    /// </summary>
    public class SimpleUpdatable : IUpdatable
    {
        /// <summary>The wrapped callback.</summary>
        public Action Callback;

        /// <summary>
        /// Creates a new <see cref="T:WCell.Core.Timers.SimpleUpdatable" /> object.
        /// </summary>
        public SimpleUpdatable()
        {
        }

        /// <summary>
        /// Creates a new <see cref="T:WCell.Core.Timers.SimpleUpdatable" /> object with the given callback.
        /// </summary>
        public SimpleUpdatable(Action callback)
        {
            this.Callback = callback;
        }

        public void Update(int dt)
        {
            this.Callback();
        }
    }
}
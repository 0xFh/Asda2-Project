using System;

namespace WCell.Core.Timers
{
    public class BucketTimer
    {
        private Action m_Action;
        internal TimerPriority priority;

        public BucketTimer(Action action, TimerPriority prio)
        {
            this.m_Action = action;
            this.priority = prio;
        }

        public Action Action
        {
            get { return this.m_Action; }
            set { this.m_Action = value; }
        }
    }
}
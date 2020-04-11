using System;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions
{
    public class AITemporaryIdleAction : IAIAction, IDisposable
    {
        private UpdatePriority m_priority = UpdatePriority.Active;
        private int m_Millis;
        private ProcTriggerFlags m_Flags;
        private Action m_Callback;
        private DateTime startTime;

        public AITemporaryIdleAction(int millis, ProcTriggerFlags flags, Action callback)
        {
            this.m_Millis = millis;
            this.m_Flags = flags;
            this.m_Callback = callback;
        }

        public Unit Owner
        {
            get { return (Unit) null; }
        }

        public UpdatePriority Priority
        {
            get { return this.m_priority; }
        }

        public bool IsGroupAction
        {
            get { return false; }
        }

        public ProcTriggerFlags InterruptFlags
        {
            get { return this.m_Flags; }
        }

        public void Start()
        {
            this.startTime = DateTime.Now;
        }

        public void Update()
        {
            uint totalMilliseconds = (uint) (DateTime.Now - this.startTime).TotalMilliseconds;
            if ((long) totalMilliseconds >= (long) this.m_Millis)
            {
                if (this.m_Callback == null)
                    return;
                this.m_Callback();
                this.m_Callback = (Action) null;
            }
            else if ((long) this.m_Millis - (long) totalMilliseconds > 10000L)
                this.m_priority = UpdatePriority.Background;
            else
                this.m_priority = UpdatePriority.Active;
        }

        public void Stop()
        {
        }

        public void Dispose()
        {
        }
    }
}
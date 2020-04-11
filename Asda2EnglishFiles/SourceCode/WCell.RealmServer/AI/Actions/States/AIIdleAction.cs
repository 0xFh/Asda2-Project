using System;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions.States
{
    public class AIIdleAction : AIAction, IAIStateAction, IAIAction, IDisposable
    {
        public AIIdleAction(Unit owner)
            : base(owner)
        {
        }

        public override void Start()
        {
            if (this.m_owner.IsAlive)
                this.m_owner.FirstAttacker = (Unit) null;
            this.m_owner.Target = (Unit) null;
        }

        public override void Update()
        {
        }

        public override void Stop()
        {
        }

        public override UpdatePriority Priority
        {
            get { return UpdatePriority.Active; }
        }
    }
}
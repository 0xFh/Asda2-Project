using System;
using WCell.Constants.Updates;
using WCell.RealmServer.AI.Actions.States;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions.Movement
{
    /// <summary>Moves to the Target and then enters Idle mode</summary>
    public class AIMoveToTargetAction : AITargetMoveAction, IAIStateAction, IAIAction, IDisposable
    {
        public AIMoveToTargetAction(Unit owner)
            : base(owner)
        {
        }

        public override void Start()
        {
            if (this.m_owner.Target == null)
            {
                this.m_owner.Say("I have no Target to follow.");
                this.m_owner.Brain.EnterDefaultState();
            }
            else
            {
                this.m_target = this.m_owner.Target;
                base.Start();
            }
        }

        protected override void OnArrived()
        {
            this.m_owner.Brain.CurrentAction = (IAIAction) null;
            this.m_owner.Brain.State = BrainState.Idle;
        }

        public override void Stop()
        {
            this.m_owner.Target = (Unit) null;
        }

        public override UpdatePriority Priority
        {
            get { return UpdatePriority.LowPriority; }
        }
    }
}
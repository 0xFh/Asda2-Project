using System;
using WCell.Constants.Updates;
using WCell.RealmServer.AI.Actions.States;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions.Movement
{
    public class AIFollowTargetAction : AITargetMoveAction, IAIStateAction, IAIAction, IDisposable
    {
        public AIFollowTargetAction(Unit owner)
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
                base.Start();
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
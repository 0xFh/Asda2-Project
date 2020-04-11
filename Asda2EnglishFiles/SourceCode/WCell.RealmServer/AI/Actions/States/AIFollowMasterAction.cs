using System;
using WCell.Constants.Updates;
using WCell.RealmServer.AI.Actions.Movement;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions.States
{
    public class AIFollowMasterAction : AITargetMoveAction, IAIStateAction, IAIAction, IDisposable
    {
        public AIFollowMasterAction(Unit owner)
            : base(owner)
        {
        }

        public override void Start()
        {
            if (!this.m_owner.HasMaster)
            {
                this.m_owner.Say("I have no Master to follow.");
                this.m_owner.Brain.EnterDefaultState();
            }
            else
            {
                this.m_owner.Target = this.Target = this.m_owner.Master;
                base.Start();
            }
        }

        public override UpdatePriority Priority
        {
            get { return UpdatePriority.LowPriority; }
        }
    }
}
using System;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions.States
{
    public class AIGuardMasterAction : AIFollowMasterAction, IAIStateAction, IAIAction, IDisposable
    {
        public AIGuardMasterAction(Unit owner)
            : base(owner)
        {
        }

        public override void Update()
        {
            if (this.m_owner.Brain.CheckCombat())
                return;
            base.Update();
        }

        protected override void OnLostTarget()
        {
            this.m_target = this.m_owner.Master;
            if (this.m_target != null)
                return;
            this.Stop();
            this.m_owner.Brain.EnterDefaultState();
        }

        public override UpdatePriority Priority
        {
            get { return UpdatePriority.LowPriority; }
        }
    }
}
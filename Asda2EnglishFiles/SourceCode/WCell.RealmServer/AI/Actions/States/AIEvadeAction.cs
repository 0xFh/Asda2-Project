using System;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions.States
{
    /// <summary>NPC leaves combat and goes home</summary>
    public class AIEvadeAction : AIAction, IAIStateAction, IAIAction, IDisposable
    {
        public AIEvadeAction(Unit owner)
            : base(owner)
        {
        }

        public override void Start()
        {
            this.m_owner.IsEvading = true;
            this.m_owner.Target = (Unit) null;
            NPC owner = this.m_owner as NPC;
            if (owner != null)
            {
                owner.ThreatCollection.Clear();
                owner.Damages.Clear();
            }

            if (!this.m_owner.Movement.MoveTo(this.m_owner.Brain.SourcePoint, true))
                return;
            this.m_owner.OnEvaded();
        }

        public override void Update()
        {
            if (this.m_owner.IsMoving && !this.m_owner.Movement.Update())
                return;
            this.m_owner.OnEvaded();
        }

        public override void Stop()
        {
            this.m_owner.IsEvading = false;
            this.m_owner.Movement.Stop();
        }

        public override UpdatePriority Priority
        {
            get { return UpdatePriority.Active; }
        }
    }
}
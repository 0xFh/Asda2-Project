using System;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions.States
{
    /// <summary>AI movemement action for roaming</summary>
    public class AIRoamAction : AIAction, IAIStateAction, IAIAction, IDisposable
    {
        public AIRoamAction(Unit owner)
            : base(owner)
        {
        }

        public AIRoamAction(Unit owner, AIAction roamAction)
            : base(owner)
        {
            this.Strategy = roamAction;
        }

        public int MinimumRoamSpellCastDelay { get; set; }

        /// <summary>The strategy to be used while roaming</summary>
        public AIAction Strategy { get; set; }

        public override void Start()
        {
            this.m_owner.FirstAttacker = (Unit) null;
            this.m_owner.Target = (Unit) null;
            NPC owner = this.Owner as NPC;
            if (owner != null && !owner.IsInCombat)
            {
                owner.ThreatCollection.Clear();
                owner.Damages.Clear();
            }

            this.Strategy.Start();
        }

        public override void Update()
        {
            if (this.m_owner.Brain.CheckCombat())
                return;
            this.Strategy.Update();
        }

        public override void Stop()
        {
            this.Strategy.Stop();
        }

        public override UpdatePriority Priority
        {
            get { return UpdatePriority.VeryLowPriority; }
        }
    }
}
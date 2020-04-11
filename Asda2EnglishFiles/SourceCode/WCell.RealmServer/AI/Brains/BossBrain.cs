using System.Collections.Generic;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.AI.Brains
{
    public class BossBrain : MobBrain
    {
        protected LinkedListNode<BossPhase> CurrentPhase;
        protected LinkedList<BossPhase> Phases;

        public BossBrain(NPC owner)
            : base(owner)
        {
        }

        public override void OnEnterCombat()
        {
            this.CurrentPhase.Value.OnEnterCombat();
            base.OnEnterCombat();
        }

        public override void OnDamageReceived(IDamageAction action)
        {
            this.CurrentPhase.Value.OnDamageTaken(action);
            base.OnDamageReceived(action);
        }

        public override void OnDamageDealt(IDamageAction action)
        {
            this.CurrentPhase.Value.OnDamageDealt(action);
            base.OnDamageDealt(action);
        }

        public override void OnLeaveCombat()
        {
            this.CurrentPhase.Value.OnLeaveCombat();
            base.OnLeaveCombat();
        }

        public override void OnKilled(Unit killerUnit, Unit victimUnit)
        {
            this.CurrentPhase.Value.OnKilled(killerUnit, victimUnit);
            base.OnKilled(killerUnit, victimUnit);
        }

        public override void OnDeath()
        {
            this.CurrentPhase.Value.OnDeath();
            base.OnDeath();
        }

        public override void OnActivate()
        {
            this.CurrentPhase.Value.OnSpawn();
            base.OnActivate();
        }

        public override void OnCombatTargetOutOfRange()
        {
            this.CurrentPhase.Value.OnCombatTargetOutOfRange();
            base.OnCombatTargetOutOfRange();
        }

        public override void OnHeal(Unit healer, Unit healed, int amtHealed)
        {
            this.CurrentPhase.Value.OnHeal(healer, healed, amtHealed);
            base.OnHeal(healer, healed, amtHealed);
        }
    }
}
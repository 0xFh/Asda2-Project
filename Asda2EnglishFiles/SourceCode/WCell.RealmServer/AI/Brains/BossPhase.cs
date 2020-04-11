using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.AI.Brains
{
    public class BossPhase : IAIEventsHandler
    {
        protected IBrain m_owner;

        public BossPhase(IBrain owner)
        {
            this.m_owner = owner;
        }

        public IBrain Owner
        {
            get { return this.m_owner; }
            set { this.m_owner = value; }
        }

        public virtual void OnEnterCombat()
        {
        }

        public virtual void OnDamageTaken(IDamageAction action)
        {
        }

        public virtual void OnDebuff(Unit attackingUnit, SpellCast cast, Aura debuff)
        {
        }

        public virtual void OnDamageDealt(IDamageAction action)
        {
        }

        public virtual void OnLeaveCombat()
        {
        }

        public virtual void OnKilled(Unit killerUnit, Unit victimUnit)
        {
        }

        public virtual void OnDeath()
        {
        }

        public virtual void OnSpawn()
        {
        }

        public virtual void OnCombatTargetOutOfRange()
        {
        }

        public virtual void OnHeal(Unit healer, Unit healed, int amtHealed)
        {
        }
    }
}
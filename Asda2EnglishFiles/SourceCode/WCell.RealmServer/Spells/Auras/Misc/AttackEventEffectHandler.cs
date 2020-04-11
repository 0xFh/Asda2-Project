using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Misc
{
    public abstract class AttackEventEffectHandler : AuraEffectHandler, IAttackEventHandler
    {
        protected override void Apply()
        {
            this.Owner.AttackEventHandlers.Add((IAttackEventHandler) this);
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.AttackEventHandlers.Remove((IAttackEventHandler) this);
            int miscValueB = this.SpellEffect.MiscValueB;
        }

        public virtual void OnBeforeAttack(DamageAction action)
        {
        }

        public virtual void OnAttack(DamageAction action)
        {
        }

        public virtual void OnDefend(DamageAction action)
        {
        }
    }
}
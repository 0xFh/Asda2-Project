using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Misc
{
	public abstract class AttackEventEffectHandler : AuraEffectHandler, IAttackEventHandler
	{
		protected override void Apply()
		{
			Owner.AttackEventHandlers.Add(this);
		}

		protected override void Remove(bool cancelled)
		{
			Owner.AttackEventHandlers.Remove(this);
            if (SpellEffect.MiscValueB == 100)//from Kings rage
            {
            }
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
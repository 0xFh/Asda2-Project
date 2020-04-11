using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    /// <summary>Do flat damage to any attacker</summary>
    public class VampiricEffectHandler : AttackEventEffectHandler
    {
        public override void OnBeforeAttack(DamageAction action)
        {
        }

        public override void OnAttack(DamageAction action)
        {
            this.Owner.Heal(action.ActualDamage, (Unit) null, (SpellEffect) null);
        }

        public override void OnDefend(DamageAction action)
        {
        }
    }
}
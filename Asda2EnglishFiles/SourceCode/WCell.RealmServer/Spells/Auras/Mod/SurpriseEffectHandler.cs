using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    public class SurpriseEffectHandler : AttackEventEffectHandler
    {
        public override void OnBeforeAttack(DamageAction action)
        {
        }

        public override void OnAttack(DamageAction action)
        {
            if (action.Spell == null)
                return;
            action.Damage = (int) ((double) action.Damage * 1.5);
            Spell spell = SpellHandler.Get(SpellId.Silence10Rank7FromWindSlasher);
            action.Victim.Auras.CreateAndStartAura(this.Owner.SharedReference, spell, false, (Item) null);
            this.Aura.Cancel();
        }
    }
}
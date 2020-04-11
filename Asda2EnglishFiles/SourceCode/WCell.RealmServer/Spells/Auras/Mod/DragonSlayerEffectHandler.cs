using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    public class DragonSlayerEffectHandler : AttackEventEffectHandler
    {
        public override void OnBeforeAttack(DamageAction action)
        {
        }

        public override void OnAttack(DamageAction action)
        {
            if (action.Spell == null)
                return;
            action.Damage = (int) ((double) action.Damage * 1.29999995231628);
            Spell spell = SpellHandler.Get(74U);
            action.Victim.Auras.CreateAndStartAura(this.Owner.SharedReference, spell, false, (Item) null);
            this.m_aura.Cancel();
        }
    }
}
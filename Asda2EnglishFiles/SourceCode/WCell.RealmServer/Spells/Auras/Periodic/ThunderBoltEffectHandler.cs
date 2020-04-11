using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Periodic
{
    public class ThunderBoltEffectHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            int dmg = this.Owner.Health * this.SpellEffect.MiscValue / 100;
            NPC owner = this.Owner as NPC;
            if (owner != null && owner.Entry.IsBoss)
                return;
            DamageAction damageAction =
                this.Owner.DealSpellDamage(this.Owner, this.SpellEffect, dmg, true, true, false, false);
            if (damageAction == null || this.m_aura == null)
                return;
            Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(this.m_aura.CasterUnit as Character,
                this.Owner as Character, this.Owner as NPC, damageAction.ActualDamage);
            damageAction.OnFinished();
            this.Aura.Cancel();
        }

        protected override void Remove(bool cancelled)
        {
        }
    }
}
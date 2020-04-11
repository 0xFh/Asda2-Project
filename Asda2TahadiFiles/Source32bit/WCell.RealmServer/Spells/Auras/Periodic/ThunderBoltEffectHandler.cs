using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Periodic
{
  public class ThunderBoltEffectHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      int dmg = Owner.Health * SpellEffect.MiscValue / 100;
      NPC owner = Owner as NPC;
      if(owner != null && owner.Entry.IsBoss)
        return;
      DamageAction damageAction =
        Owner.DealSpellDamage(Owner, SpellEffect, dmg, true, true, false, false);
      if(damageAction == null || m_aura == null)
        return;
      Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(m_aura.CasterUnit as Character,
        Owner as Character, Owner as NPC, damageAction.ActualDamage);
      damageAction.OnFinished();
      Aura.Cancel();
    }

    protected override void Remove(bool cancelled)
    {
    }
  }
}
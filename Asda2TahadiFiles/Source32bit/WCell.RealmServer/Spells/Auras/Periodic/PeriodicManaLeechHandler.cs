using System;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class PeriodicManaLeechHandler : AuraEffectHandler
  {
    protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference,
      Unit target, ref SpellFailedReason failReason)
    {
      if(target.MaxPower != 0 && target.PowerType == (PowerType) m_spellEffect.MiscValue)
        return;
      failReason = SpellFailedReason.BadTargets;
    }

    protected override void Apply()
    {
      int amount = EffectValue;
      Unit owner = m_aura.Auras.Owner;
      if(m_aura.Spell.HasEffectWith(effect => effect.AuraType == AuraType.Dummy))
        amount = owner.BasePower * amount / 100;
      owner.LeechPower(amount, 1f, m_aura.CasterUnit, m_spellEffect);
    }
  }
}
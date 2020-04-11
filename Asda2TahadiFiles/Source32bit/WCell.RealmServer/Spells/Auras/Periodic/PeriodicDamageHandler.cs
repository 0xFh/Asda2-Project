using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Periodically damages the holder</summary>
  public class PeriodicDamageHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Unit owner = Owner;
      if(!owner.IsAlive)
        return;
      float num = (float) (SpellEffect.MiscValue * (m_aura.CasterUnit != null
                             ? (m_aura.CasterUnit.Class == ClassId.AtackMage ||
                                m_aura.CasterUnit.Class == ClassId.SupportMage ||
                                m_aura.CasterUnit.Class == ClassId.HealMage
                               ? m_aura.CasterUnit.RandomMagicDamage
                               : m_aura.CasterUnit.RandomDamage)
                             : 666.0) / 100.0 * 3.0);
      if(m_aura.Spell.Mechanic == SpellMechanic.Bleeding)
      {
        int bleedBonusPercent = m_aura.Auras.GetBleedBonusPercent();
        num += (float) ((num * (double) bleedBonusPercent + 50.0) / 100.0);
        m_aura.Owner.IncMechanicCount(SpellMechanic.Bleeding, false);
      }

      DamageAction damageAction = owner.DealSpellDamage(m_aura.CasterUnit, m_spellEffect, (int) num,
        true, true, false, false);
      if(damageAction == null)
        return;
      Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(m_aura.CasterUnit as Character,
        Owner as Character, Owner as NPC, damageAction.ActualDamage);
      damageAction.OnFinished();
    }

    protected override void Remove(bool cancelled)
    {
      if(m_aura.Spell.Mechanic != SpellMechanic.Bleeding)
        return;
      m_aura.Owner.DecMechanicCount(SpellMechanic.Bleeding, false);
    }
  }
}
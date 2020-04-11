using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Periodically damages the holder in %</summary>
  public class PeriodicDamagePercentHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Unit owner = Owner;
      if(!owner.IsAlive)
        return;
      int dmg = (Owner.MaxHealth * EffectValue + 50) / 100;
      if(m_aura.Spell.Mechanic == SpellMechanic.Bleeding)
      {
        int bleedBonusPercent = m_aura.Auras.GetBleedBonusPercent();
        dmg += (dmg * bleedBonusPercent + 50) / 100;
      }

      owner.DealSpellDamage(m_aura.CasterUnit, m_spellEffect, dmg, false, true, false, true);
    }
  }
}
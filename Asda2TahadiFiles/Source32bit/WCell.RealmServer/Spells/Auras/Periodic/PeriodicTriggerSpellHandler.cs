using NLog;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Periodically makes the holder cast a Spell</summary>
  public class PeriodicTriggerSpellHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      TriggerSpell(m_spellEffect.TriggerSpell);
    }

    protected void TriggerSpell(Spell spell)
    {
      SpellCast spellCast = m_aura.SpellCast;
      if(spell == null)
        LogManager.GetCurrentClassLogger().Warn("Found invalid periodic TriggerSpell in Spell {0} ({1}) ",
          m_aura.Spell, m_spellEffect.TriggerSpellId);
      else
        SpellCast.ValidateAndTriggerNew(spell, m_aura.CasterReference, Owner,
          Owner, m_aura.Controller as SpellChannel, spellCast?.TargetItem,
          null, m_spellEffect);
    }

    protected override void Remove(bool cancelled)
    {
    }
  }
}
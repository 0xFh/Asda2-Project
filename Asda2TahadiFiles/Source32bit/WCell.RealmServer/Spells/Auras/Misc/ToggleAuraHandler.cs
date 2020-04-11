using NLog;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Misc
{
  /// <summary>
  /// Applies another aura while active and removes it when turning inactive
  /// </summary>
  public class ToggleAuraHandler : AuraEffectHandler
  {
    private Aura activeToggleAura;

    public Spell ToggleAuraSpell { get; set; }

    public ToggleAuraHandler()
    {
    }

    public ToggleAuraHandler(SpellId auraId)
    {
      ToggleAuraSpell = SpellHandler.Get(auraId);
    }

    protected override void Apply()
    {
      if(ToggleAuraSpell == null)
        ToggleAuraSpell = m_spellEffect.TriggerSpell;
      activeToggleAura = Owner.Auras[ToggleAuraSpell];
      if(activeToggleAura == null)
      {
        activeToggleAura = Owner.Auras.CreateAndStartAura(m_aura.CasterReference,
          ToggleAuraSpell, true, null);
        activeToggleAura.CanBeSaved = false;
      }
      else
      {
        LogManager.GetCurrentClassLogger().Warn("Tried to toggle on already created Aura \"{0}\" on {1}",
          activeToggleAura, Owner);
        activeToggleAura.IsActivated = true;
      }
    }

    protected override void Remove(bool cancelled)
    {
      if(activeToggleAura == null)
        return;
      activeToggleAura.Cancel();
      activeToggleAura = null;
    }
  }
}
using NLog;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ProcTriggerSpellHandler : AuraEffectHandler
  {
    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Called when a matching proc event triggers this proc handler with the given
    /// triggerer and action.
    /// </summary>
    public override void OnProc(Unit triggerer, IUnitAction action)
    {
      if(m_spellEffect.TriggerSpell == null)
        return;
      SpellCast.ValidateAndTriggerNew(m_spellEffect.TriggerSpell, m_aura.CasterReference, Owner,
        triggerer, m_aura.Controller as SpellChannel, m_aura.UsedItem, action,
        m_spellEffect);
    }
  }
}
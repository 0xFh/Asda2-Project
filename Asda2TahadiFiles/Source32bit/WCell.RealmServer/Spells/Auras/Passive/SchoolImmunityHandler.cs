using NLog;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class SchoolImmunityHandler : AuraEffectHandler
  {
    private static Logger log = LogManager.GetCurrentClassLogger();

    protected override void Apply()
    {
      m_aura.Auras.Owner.IncDmgImmunityCount(m_spellEffect);
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.DecDmgImmunityCount(m_spellEffect.MiscBitSet);
    }
  }
}
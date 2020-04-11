using NLog;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Applies a mount-aura</summary>
  public class MountedHandler : AuraEffectHandler
  {
    private static Logger log = LogManager.GetCurrentClassLogger();

    protected override void Apply()
    {
      NPCMgr.GetEntry((uint) SpellEffect.MiscValue);
    }

    protected override void Remove(bool cancelled)
    {
      if(m_aura.Spell.IsFlyingMount && !m_aura.Spell.HasFlyEffect)
        --m_aura.Auras.Owner.Flying;
      m_aura.Auras.Owner.DoDismount();
    }
  }
}
using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class UntrackableHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      m_aura.Auras.Owner.StateFlags |= StateFlag.UnTrackable;
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.StateFlags &= ~StateFlag.UnTrackable;
    }
  }
}
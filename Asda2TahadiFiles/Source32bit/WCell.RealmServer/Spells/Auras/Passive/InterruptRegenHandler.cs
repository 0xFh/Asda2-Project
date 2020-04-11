namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Interrupts Regeneration while applied</summary>
  public class InterruptRegenHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      m_aura.Auras.Owner.Regenerates = false;
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.Regenerates = true;
    }
  }
}
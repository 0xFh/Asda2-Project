namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModScaleHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      m_aura.Auras.Owner.ScaleX += EffectValue / 100f;
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.ScaleX -= EffectValue / 100f;
    }
  }
}
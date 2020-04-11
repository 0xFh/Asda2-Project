namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModDecreaseSpeedHandler : AuraEffectHandler
  {
    public float Value;

    protected override void Apply()
    {
      m_aura.Auras.Owner.SpeedFactor += Value = EffectValue / 100f;
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.SpeedFactor -= Value;
    }
  }
}
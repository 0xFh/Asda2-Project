namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModIncreaseSpeedAlwaysHandler : AuraEffectHandler
  {
    private float amount;

    protected override void Apply()
    {
      amount = EffectValue / 100f;
      m_aura.Auras.Owner.SpeedFactor += amount;
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.SpeedFactor -= amount;
    }
  }
}
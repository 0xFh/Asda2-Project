namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>
  /// Increases (or decreases) overall speed
  /// TODO: If ShapeshiftMask is set, it only applies to the given form(s)
  /// </summary>
  public class ModIncreaseSpeedHandler : AuraEffectHandler
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
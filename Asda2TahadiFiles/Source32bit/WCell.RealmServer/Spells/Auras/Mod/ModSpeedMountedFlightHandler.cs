namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Flying mount speed effect.</summary>
  public class ModSpeedMountedFlightHandler : AuraEffectHandler
  {
    private float val;

    protected override void Apply()
    {
      m_aura.Auras.Owner.FlightSpeedFactor += val = EffectValue / 100f;
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.FlightSpeedFactor -= val;
    }
  }
}
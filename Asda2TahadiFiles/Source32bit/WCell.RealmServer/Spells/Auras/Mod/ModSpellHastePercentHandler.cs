namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Modidifies casting speed</summary>
  public class ModSpellHastePercentHandler : AuraEffectHandler
  {
    private float val;

    protected override void Apply()
    {
      m_aura.Auras.Owner.CastSpeedFactor += val = EffectValue / 100f;
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.CastSpeedFactor -= val;
    }
  }
}
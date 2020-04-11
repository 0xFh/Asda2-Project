namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Decreases cost for spells of certain schools (flat)</summary>
  public class ModPowerCostForSchoolHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      m_aura.Auras.Owner.ModPowerCost(m_spellEffect.MiscBitSet, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.ModPowerCost(m_spellEffect.MiscBitSet, -EffectValue);
    }

    public override bool IsPositive
    {
      get { return EffectValue <= 0; }
    }
  }
}
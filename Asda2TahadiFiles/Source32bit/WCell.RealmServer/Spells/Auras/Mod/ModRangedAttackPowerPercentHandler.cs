namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModRangedAttackPowerPercentHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      m_aura.Auras.Owner.RangedAttackPowerMultiplier += EffectValue / 100f;
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.RangedAttackPowerMultiplier -= EffectValue / 100f;
    }
  }
}
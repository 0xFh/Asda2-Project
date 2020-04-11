namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModMeleeAttackPowerPercentHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      m_aura.Auras.Owner.MeleeAttackPowerMultiplier += EffectValue / 100f;
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.MeleeAttackPowerMultiplier -= EffectValue / 100f;
    }
  }
}
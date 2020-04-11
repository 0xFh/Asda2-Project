namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Modifies MaxHealth</summary>
  public class ModMaxHealthHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      m_aura.Auras.Owner.MaxHealthModFlat += EffectValue;
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.MaxHealthModFlat -= EffectValue;
    }
  }
}
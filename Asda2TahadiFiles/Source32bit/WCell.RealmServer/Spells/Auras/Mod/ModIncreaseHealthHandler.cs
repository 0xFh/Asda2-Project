namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModIncreaseHealthHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Owner.Health += EffectValue;
      Owner.MaxHealthModFlat += EffectValue;
    }

    protected override void Remove(bool cancelled)
    {
      Owner.MaxHealthModFlat -= EffectValue;
    }
  }
}
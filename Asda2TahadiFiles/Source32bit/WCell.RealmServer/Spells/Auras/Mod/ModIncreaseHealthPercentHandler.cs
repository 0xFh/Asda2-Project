namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModIncreaseHealthPercentHandler : AuraEffectHandler
  {
    private int health;

    protected override void Apply()
    {
      health = (Owner.MaxHealth * EffectValue + 50) / 100;
      Owner.Health += health;
      Owner.MaxHealthModScalar += EffectValue / 100f;
    }

    protected override void Remove(bool cancelled)
    {
      Owner.Health -= health;
      Owner.MaxHealthModScalar -= EffectValue / 100f;
    }
  }
}
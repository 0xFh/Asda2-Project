namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModAttackerCritChancePercentHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Owner.AttackerPhysicalCritChancePercentMod += EffectValue;
    }

    protected override void Remove(bool cancelled)
    {
      Owner.AttackerPhysicalCritChancePercentMod -= EffectValue;
    }
  }
}
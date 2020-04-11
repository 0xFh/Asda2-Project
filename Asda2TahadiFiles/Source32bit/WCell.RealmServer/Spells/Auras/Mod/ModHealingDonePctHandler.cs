using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Increases healing taken in %</summary>
  public class ModHealingDonePctHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Character owner = Owner as Character;
      if(owner == null)
        return;
      owner.HealingDoneModPct += EffectValue;
    }

    protected override void Remove(bool cancelled)
    {
      Character owner = Owner as Character;
      if(owner == null)
        return;
      owner.HealingDoneModPct -= EffectValue;
    }
  }
}
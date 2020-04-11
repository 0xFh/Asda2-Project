using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Increases healing done by %</summary>
  public class ModHealingTakenPctHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Character owner = Owner as Character;
      if(owner == null)
        return;
      owner.HealingTakenModPct += SpellEffect.MiscValue;
    }

    protected override void Remove(bool cancelled)
    {
      Character owner = Owner as Character;
      if(owner == null)
        return;
      owner.HealingTakenModPct -= SpellEffect.MiscValue;
    }
  }
}
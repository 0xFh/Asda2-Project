namespace WCell.RealmServer.Spells.Auras.Misc
{
  /// <summary>
  /// Increases your attack power by $s2 for every ${$m1*$m2} armor value you have.
  /// TODO: Update when armor changes
  /// </summary>
  public class ModAPByArmorHandler : AuraEffectHandler
  {
    private int amt;

    protected override void Apply()
    {
      amt = (Owner.Armor + EffectValue - 1) / EffectValue;
      if(amt > 0)
        Owner.MeleeAttackPowerModsPos += amt;
      else
        Owner.MeleeAttackPowerModsNeg -= amt;
    }

    protected override void Remove(bool cancelled)
    {
      if(amt > 0)
        Owner.MeleeAttackPowerModsPos -= amt;
      else
        Owner.MeleeAttackPowerModsNeg += amt;
    }
  }
}
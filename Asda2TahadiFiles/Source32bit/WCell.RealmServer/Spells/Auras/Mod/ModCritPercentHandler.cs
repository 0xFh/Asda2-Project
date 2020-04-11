using WCell.Constants;
using WCell.Constants.Items;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Modifies MeleeCritHitRating</summary>
  public class ModCritPercentHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      ModValues(EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      ModValues(-EffectValue);
    }

    private void ModValues(int delta)
    {
      if(SpellEffect.Spell.HasItemRequirements)
      {
        if(SpellEffect.Spell.EquipmentSlot == EquipmentSlot.ExtraWeapon)
          Owner.ChangeModifier(StatModifierInt.RangedCritChance, delta);
        else
          Owner.ModCritMod(DamageSchool.Physical, delta);
      }
      else if(SpellEffect.Spell.SchoolMask == DamageSchoolMask.Physical)
      {
        Owner.ModCritMod(DamageSchool.Physical, delta);
        Owner.ChangeModifier(StatModifierInt.RangedCritChance, delta);
      }
      else
        Owner.ModCritMod(SpellEffect.Spell.Schools, delta);
    }
  }
}
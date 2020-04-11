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
            this.ModValues(this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            this.ModValues(-this.EffectValue);
        }

        private void ModValues(int delta)
        {
            if (this.SpellEffect.Spell.HasItemRequirements)
            {
                if (this.SpellEffect.Spell.EquipmentSlot == EquipmentSlot.ExtraWeapon)
                    this.Owner.ChangeModifier(StatModifierInt.RangedCritChance, delta);
                else
                    this.Owner.ModCritMod(DamageSchool.Physical, delta);
            }
            else if (this.SpellEffect.Spell.SchoolMask == DamageSchoolMask.Physical)
            {
                this.Owner.ModCritMod(DamageSchool.Physical, delta);
                this.Owner.ChangeModifier(StatModifierInt.RangedCritChance, delta);
            }
            else
                this.Owner.ModCritMod(this.SpellEffect.Spell.Schools, delta);
        }
    }
}
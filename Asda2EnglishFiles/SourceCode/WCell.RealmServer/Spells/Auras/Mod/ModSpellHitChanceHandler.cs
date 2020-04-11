using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Mods Spell crit chance in %</summary>
    public class ModSpellHitChanceHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Unit owner = this.Owner;
            if (this.m_spellEffect.MiscValue == 0)
            {
                for (DamageSchool school = DamageSchool.Physical; school < DamageSchool.Count; ++school)
                    owner.ModSpellHitChance(school, this.EffectValue);
            }
            else
                owner.ModSpellHitChance(this.m_spellEffect.MiscBitSet, this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            Unit owner = this.Owner;
            if (this.m_spellEffect.MiscValue == 0)
            {
                for (DamageSchool school = DamageSchool.Physical; school < DamageSchool.Count; ++school)
                    owner.ModSpellHitChance(school, -this.EffectValue);
            }
            else
                owner.ModSpellHitChance(this.m_spellEffect.MiscBitSet, -this.EffectValue);
        }
    }
}
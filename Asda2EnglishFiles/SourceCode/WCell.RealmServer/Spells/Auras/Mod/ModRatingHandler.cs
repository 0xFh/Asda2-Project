using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Modifies CombatRatings</summary>
    public class ModRatingHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Character owner = this.m_aura.Auras.Owner as Character;
            if (owner == null)
                return;
            owner.ModCombatRating(this.m_spellEffect.MiscBitSet, this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            Character owner = this.m_aura.Auras.Owner as Character;
            if (owner == null)
                return;
            owner.ModCombatRating(this.m_spellEffect.MiscBitSet, -this.EffectValue);
        }
    }
}
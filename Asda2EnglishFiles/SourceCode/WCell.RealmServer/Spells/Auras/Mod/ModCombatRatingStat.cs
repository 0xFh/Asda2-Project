using WCell.Constants;
using WCell.Constants.Misc;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Modifies CombatRatings</summary>
    public class ModCombatRatingStat : AuraEffectHandler
    {
        private int value;

        protected override void Apply()
        {
            Character owner = this.m_aura.Auras.Owner as Character;
            if (owner == null)
                return;
            this.value = owner.GetTotalStatValue((StatType) this.m_spellEffect.MiscValueB) / this.EffectValue;
            owner.ModCombatRating((CombatRating) this.m_spellEffect.MiscValue, this.value);
        }

        protected override void Remove(bool cancelled)
        {
            Character owner = this.m_aura.Auras.Owner as Character;
            if (owner == null)
                return;
            owner.ModCombatRating((CombatRating) this.m_spellEffect.MiscValue, -this.value);
        }
    }
}
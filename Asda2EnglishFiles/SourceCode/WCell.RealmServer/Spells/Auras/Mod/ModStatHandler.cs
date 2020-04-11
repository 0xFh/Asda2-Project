using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModStatHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.Owner.AddStatMod((StatType) this.m_spellEffect.MiscValue, this.EffectValue,
                this.m_aura.Spell.IsPassive);
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.RemoveStatMod((StatType) this.m_spellEffect.MiscValue, this.EffectValue,
                this.m_aura.Spell.IsPassive);
        }
    }
}
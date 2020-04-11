using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Adds immunity against a specific DispelType</summary>
    public class DispelImmunityHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.m_aura.Auras.Owner.IncDispelImmunity((DispelType) this.m_spellEffect.MiscValue);
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.DecDispelImmunity((DispelType) this.m_spellEffect.MiscValue);
        }
    }
}
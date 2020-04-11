using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Drains Mana and applies damage</summary>
    public class PowerBurnHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Unit owner = this.Owner;
            if (owner.PowerType != (PowerType) this.m_spellEffect.MiscValue || this.m_aura.CasterUnit == null)
                return;
            owner.BurnPower(this.EffectValue, this.m_spellEffect.ProcValue, this.m_aura.CasterUnit, this.m_spellEffect);
        }
    }
}
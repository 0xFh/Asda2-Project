using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Regenerates a percentage of your total Mana every tick
    /// </summary>
    public class RegenPercentOfTotalHealthHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Unit owner = this.m_aura.Auras.Owner;
            if (!owner.IsAlive)
                return;
            owner.HealPercent(this.EffectValue, this.m_aura.CasterUnit, this.m_spellEffect);
        }
    }
}
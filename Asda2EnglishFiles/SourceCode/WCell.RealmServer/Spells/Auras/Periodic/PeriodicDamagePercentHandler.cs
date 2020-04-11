using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Periodically damages the holder in %</summary>
    public class PeriodicDamagePercentHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Unit owner = this.Owner;
            if (!owner.IsAlive)
                return;
            int dmg = (this.Owner.MaxHealth * this.EffectValue + 50) / 100;
            if (this.m_aura.Spell.Mechanic == SpellMechanic.Bleeding)
            {
                int bleedBonusPercent = this.m_aura.Auras.GetBleedBonusPercent();
                dmg += (dmg * bleedBonusPercent + 50) / 100;
            }

            owner.DealSpellDamage(this.m_aura.CasterUnit, this.m_spellEffect, dmg, false, true, false, true);
        }
    }
}
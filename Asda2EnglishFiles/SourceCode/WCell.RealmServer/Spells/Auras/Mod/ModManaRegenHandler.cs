using System;
using WCell.Constants;
using WCell.Util.Variables;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Used by some Druid talents
    /// Regenerates mana in % of a given stat periodically (default every 5 secs)
    /// </summary>
    public class ModManaRegenHandler : AuraEffectHandler
    {
        /// <summary>
        /// Used for ModManaRegen effects that don't have an Amplitude
        /// </summary>
        [Variable("DefaultManaRegenBuffAmplitude")]
        public static int DefaultAmplitude = 5000;

        protected override void Apply()
        {
            this.m_aura.Auras.Owner.Energize(
                (int) Math.Round(
                    (double) this.m_aura.Auras.Owner.GetTotalStatValue((StatType) this.m_spellEffect.MiscValue) *
                    ((double) this.EffectValue / 100.0)), this.m_aura.CasterUnit, this.m_spellEffect);
        }
    }
}
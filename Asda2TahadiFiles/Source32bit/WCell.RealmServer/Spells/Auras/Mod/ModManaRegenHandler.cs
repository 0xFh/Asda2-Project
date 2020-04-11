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
      m_aura.Auras.Owner.Energize(
        (int) Math.Round(
          m_aura.Auras.Owner.GetTotalStatValue((StatType) m_spellEffect.MiscValue) *
          (EffectValue / 100.0)), m_aura.CasterUnit, m_spellEffect);
    }
  }
}
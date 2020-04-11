using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>
  /// Boosts interrupted Mana regen.
  /// See: http://www.wowwiki.com/Formulas:Mana_Regen#Five_Second_Rule
  /// </summary>
  public class ModManaRegenInterruptHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      m_aura.Auras.Owner.ChangeModifier(StatModifierInt.ManaRegenInterruptPct, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.ChangeModifier(StatModifierInt.ManaRegenInterruptPct, -EffectValue);
    }
  }
}
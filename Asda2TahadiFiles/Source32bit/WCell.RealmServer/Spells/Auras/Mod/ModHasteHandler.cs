using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>
  /// Mods Melee Attack speed (positive value decreases time)
  /// Same as ModAttackSpeed
  /// </summary>
  public class ModHasteHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.MeleeAttackTime, -EffectValue / 100f);
      m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.RangedAttackTime,
        -EffectValue / 100f);
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.MeleeAttackTime, EffectValue / 100f);
      m_aura.Auras.Owner.ChangeModifier(StatModifierFloat.RangedAttackTime, EffectValue / 100f);
    }
  }
}
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModPowerRegenHandler : AuraEffectHandler
  {
    protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference,
      Unit target, ref SpellFailedReason failReason)
    {
      PowerType miscValue = (PowerType) m_spellEffect.MiscValue;
      if(target.PowerType == miscValue)
        return;
      failReason = SpellFailedReason.BadTargets;
    }

    protected override void Apply()
    {
      m_aura.Auras.Owner.ChangeModifier(StatModifierInt.PowerRegen, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.ChangeModifier(StatModifierInt.PowerRegen, -EffectValue);
    }
  }
}
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>
  /// Regenerates a percentage of your total Mana every tick
  /// </summary>
  public class RegenPercentOfTotalManaHandler : AuraEffectHandler
  {
    protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference,
      Unit target, ref SpellFailedReason failReason)
    {
    }

    protected override void Apply()
    {
      Owner.Energize((EffectValue * Owner.MaxPower + 50) / 100, m_aura.CasterUnit,
        m_spellEffect);
    }
  }
}
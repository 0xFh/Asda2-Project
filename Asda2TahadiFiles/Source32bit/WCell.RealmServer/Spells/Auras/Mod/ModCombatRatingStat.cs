using WCell.Constants;
using WCell.Constants.Misc;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Modifies CombatRatings</summary>
  public class ModCombatRatingStat : AuraEffectHandler
  {
    private int value;

    protected override void Apply()
    {
      Character owner = m_aura.Auras.Owner as Character;
      if(owner == null)
        return;
      value = owner.GetTotalStatValue((StatType) m_spellEffect.MiscValueB) / EffectValue;
      owner.ModCombatRating((CombatRating) m_spellEffect.MiscValue, value);
    }

    protected override void Remove(bool cancelled)
    {
      Character owner = m_aura.Auras.Owner as Character;
      if(owner == null)
        return;
      owner.ModCombatRating((CombatRating) m_spellEffect.MiscValue, -value);
    }
  }
}
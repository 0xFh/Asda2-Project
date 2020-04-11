using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Modify the amount of reputation given.</summary>
  public class ModReputationGainHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Character owner = m_aura.Auras.Owner as Character;
      if(owner == null)
        return;
      owner.ReputationGainModifierPercent += EffectValue;
    }

    protected override void Remove(bool cancelled)
    {
      Character owner = m_aura.Auras.Owner as Character;
      if(owner == null)
        return;
      owner.ReputationGainModifierPercent -= EffectValue;
    }
  }
}
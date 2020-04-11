using WCell.Constants.Misc;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Temporarily changes the language of the holder</summary>
  public class ModLanguageHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      if(!(m_aura.Auras.Owner is Character))
        return;
      m_aura.Auras.Owner.SpokenLanguage = (ChatLanguage) m_spellEffect.MiscValue;
    }

    protected override void Remove(bool cancelled)
    {
      if(!(m_aura.Auras.Owner is Character))
        return;
      m_aura.Auras.Owner.SpokenLanguage = ChatLanguage.Universal;
    }
  }
}
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class AddModifierPercentHandler : AddModifierEffectHandler
  {
    protected override void Apply()
    {
      Character owner = m_aura.Auras.Owner as Character;
      if(owner == null)
        return;
      Charges = m_aura.Spell.ProcCharges;
      owner.PlayerAuras.AddSpellModifierPercent(this);
    }

    protected override void Remove(bool cancelled)
    {
      Character owner = m_aura.Auras.Owner as Character;
      if(owner == null)
        return;
      owner.PlayerAuras.RemoveSpellModifierPercent(this);
    }
  }
}
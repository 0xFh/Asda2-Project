using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>
  /// All kinds of different Spell modifiers (mostly caused by talents)
  /// </summary>
  public class AddModifierFlatHandler : AddModifierEffectHandler
  {
    protected override void Apply()
    {
      Character owner = m_aura.Auras.Owner as Character;
      if(owner == null)
        return;
      Charges = m_aura.Spell.ProcCharges;
      owner.PlayerAuras.AddSpellModifierFlat(this);
    }

    protected override void Remove(bool cancelled)
    {
      Character owner = m_aura.Auras.Owner as Character;
      if(owner == null)
        return;
      owner.PlayerAuras.RemoveSpellModifierFlat(this);
    }
  }
}
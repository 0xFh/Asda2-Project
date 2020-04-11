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
            Character owner = this.m_aura.Auras.Owner as Character;
            if (owner == null)
                return;
            this.Charges = this.m_aura.Spell.ProcCharges;
            owner.PlayerAuras.AddSpellModifierFlat((AddModifierEffectHandler) this);
        }

        protected override void Remove(bool cancelled)
        {
            Character owner = this.m_aura.Auras.Owner as Character;
            if (owner == null)
                return;
            owner.PlayerAuras.RemoveSpellModifierFlat((AddModifierEffectHandler) this);
        }
    }
}
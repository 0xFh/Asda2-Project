using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class AddModifierPercentHandler : AddModifierEffectHandler
    {
        protected override void Apply()
        {
            Character owner = this.m_aura.Auras.Owner as Character;
            if (owner == null)
                return;
            this.Charges = this.m_aura.Spell.ProcCharges;
            owner.PlayerAuras.AddSpellModifierPercent((AddModifierEffectHandler) this);
        }

        protected override void Remove(bool cancelled)
        {
            Character owner = this.m_aura.Auras.Owner as Character;
            if (owner == null)
                return;
            owner.PlayerAuras.RemoveSpellModifierPercent((AddModifierEffectHandler) this);
        }
    }
}
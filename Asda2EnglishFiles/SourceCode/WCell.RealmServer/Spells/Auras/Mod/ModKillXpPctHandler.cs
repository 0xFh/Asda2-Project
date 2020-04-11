using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModKillXpPctHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Character owner = this.m_aura.Auras.Owner as Character;
            if (owner == null)
                return;
            owner.KillExperienceGainModifierPercent += this.EffectValue;
        }

        protected override void Remove(bool cancelled)
        {
            Character owner = this.m_aura.Auras.Owner as Character;
            if (owner == null)
                return;
            owner.KillExperienceGainModifierPercent -= this.EffectValue;
        }
    }
}
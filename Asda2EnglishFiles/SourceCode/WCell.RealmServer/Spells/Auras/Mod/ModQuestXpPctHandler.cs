using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModQuestXpPctHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Character owner = this.m_aura.Auras.Owner as Character;
            if (owner == null)
                return;
            owner.QuestExperienceGainModifierPercent += this.EffectValue;
        }

        protected override void Remove(bool cancelled)
        {
            Character owner = this.m_aura.Auras.Owner as Character;
            if (owner == null)
                return;
            owner.QuestExperienceGainModifierPercent -= this.EffectValue;
        }
    }
}
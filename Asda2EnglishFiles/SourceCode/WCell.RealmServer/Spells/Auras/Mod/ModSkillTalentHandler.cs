using WCell.Constants.Skills;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Skills;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Adds a flat modifier to one Skill</summary>
    public class ModSkillTalentHandler : AuraEffectHandler
    {
        private Skill skill;

        protected override void Apply()
        {
            if (!(this.m_aura.Auras.Owner is Character))
                return;
            this.skill = ((Character) this.m_aura.Auras.Owner).Skills[(SkillId) this.m_spellEffect.MiscValue];
            if (this.skill == null)
                return;
            this.skill.Modifier += (short) this.EffectValue;
        }

        protected override void Remove(bool cancelled)
        {
            if (this.skill == null)
                return;
            this.skill.Modifier -= (short) this.EffectValue;
        }
    }
}
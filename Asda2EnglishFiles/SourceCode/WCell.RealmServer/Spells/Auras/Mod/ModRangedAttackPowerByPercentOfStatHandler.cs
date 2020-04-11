using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModRangedAttackPowerByPercentOfStatHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Character owner = this.Owner as Character;
            if (owner == null)
                return;
            owner.ModRangedAPModByStat((StatType) this.m_spellEffect.MiscValue, this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            Character owner = this.Owner as Character;
            if (owner == null)
                return;
            owner.ModRangedAPModByStat((StatType) this.m_spellEffect.MiscValue, -this.EffectValue);
        }
    }
}
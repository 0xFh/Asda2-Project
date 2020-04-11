using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    public class ModHealingByPercentOfStatHandler : AuraEffectHandler
    {
        private int value;

        protected override void Apply()
        {
            if (!(this.m_aura.Auras.Owner is Character))
                return;
            this.value =
                (this.Owner.GetTotalStatValue((StatType) this.SpellEffect.MiscValueB) * this.EffectValue + 50) / 100;
            ((Character) this.m_aura.Auras.Owner).HealingDoneMod += this.value;
        }

        protected override void Remove(bool cancelled)
        {
            if (!(this.m_aura.Auras.Owner is Character))
                return;
            ((Character) this.m_aura.Auras.Owner).HealingDoneMod -= this.value;
        }
    }
}
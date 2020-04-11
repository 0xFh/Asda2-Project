using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModIncreaseSwimSpeedHandler : AuraEffectHandler
    {
        private float val;

        protected override void Apply()
        {
            this.m_aura.Auras.Owner.SwimSpeedFactor =
                UnitUpdates.GetMultiMod(this.val = (float) this.EffectValue / 100f,
                    this.m_aura.Auras.Owner.SwimSpeedFactor);
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.SwimSpeedFactor =
                UnitUpdates.GetMultiMod(-this.val, this.m_aura.Auras.Owner.SwimSpeedFactor);
        }
    }
}
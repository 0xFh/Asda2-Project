using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Dead players wear this</summary>
    public class GhostHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Unit owner = this.m_aura.Auras.Owner;
            if (owner is Character)
                ((Character) owner).PlayerFlags |= PlayerFlags.Ghost;
            int race = (int) owner.Race;
            owner.SpeedFactor += Character.DeathSpeedFactorIncrease;
            ++owner.WaterWalk;
            this.m_aura.Auras.GhostAura = this.m_aura;
        }

        protected override void Remove(bool cancelled)
        {
            Unit owner = this.m_aura.Auras.Owner;
            if (owner is Character)
                ((Character) owner).PlayerFlags &= ~PlayerFlags.Ghost;
            int race = (int) owner.Race;
            if (this.m_aura.Auras.GhostAura == this.m_aura)
                this.m_aura.Auras.GhostAura = (Aura) null;
            owner.SpeedFactor -= Character.DeathSpeedFactorIncrease;
            --owner.WaterWalk;
            this.m_aura.Auras.Owner.OnResurrect();
        }

        /// <summary>Not positive also means that its not removable</summary>
        public override bool IsPositive
        {
            get { return false; }
        }
    }
}
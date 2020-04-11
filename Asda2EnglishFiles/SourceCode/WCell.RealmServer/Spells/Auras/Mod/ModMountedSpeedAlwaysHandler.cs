using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Same as ModIncreaseMountedSpeed, it seems</summary>
    public class ModMountedSpeedAlwaysHandler : AuraEffectHandler
    {
        private float val;

        protected override void Apply()
        {
            this.m_aura.Auras.Owner.MountSpeedMod = UnitUpdates.GetMultiMod(this.val = (float) this.EffectValue / 100f,
                this.m_aura.Auras.Owner.MountSpeedMod);
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.MountSpeedMod =
                UnitUpdates.GetMultiMod(-this.val, this.m_aura.Auras.Owner.MountSpeedMod);
        }
    }
}
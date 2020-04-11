using NLog;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Applies a mount-aura</summary>
    public class MountedHandler : AuraEffectHandler
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        protected override void Apply()
        {
            NPCMgr.GetEntry((uint) this.SpellEffect.MiscValue);
        }

        protected override void Remove(bool cancelled)
        {
            if (this.m_aura.Spell.IsFlyingMount && !this.m_aura.Spell.HasFlyEffect)
                --this.m_aura.Auras.Owner.Flying;
            this.m_aura.Auras.Owner.DoDismount();
        }
    }
}
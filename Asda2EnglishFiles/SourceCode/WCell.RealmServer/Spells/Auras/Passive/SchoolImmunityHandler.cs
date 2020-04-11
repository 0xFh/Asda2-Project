using NLog;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class SchoolImmunityHandler : AuraEffectHandler
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        protected override void Apply()
        {
            this.m_aura.Auras.Owner.IncDmgImmunityCount(this.m_spellEffect);
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.DecDmgImmunityCount(this.m_spellEffect.MiscBitSet);
        }
    }
}
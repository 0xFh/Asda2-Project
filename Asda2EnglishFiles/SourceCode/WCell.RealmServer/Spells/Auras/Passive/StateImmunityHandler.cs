using NLog;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class StateImmunityHandler : AuraEffectHandler
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        protected override void Apply()
        {
        }

        protected override void Remove(bool cancelled)
        {
        }
    }
}
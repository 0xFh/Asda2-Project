using NLog;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ProcTriggerSpellHandler : AuraEffectHandler
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Called when a matching proc event triggers this proc handler with the given
        /// triggerer and action.
        /// </summary>
        public override void OnProc(Unit triggerer, IUnitAction action)
        {
            if (this.m_spellEffect.TriggerSpell == null)
                return;
            SpellCast.ValidateAndTriggerNew(this.m_spellEffect.TriggerSpell, this.m_aura.CasterReference, this.Owner,
                (WorldObject) triggerer, this.m_aura.Controller as SpellChannel, this.m_aura.UsedItem, action,
                this.m_spellEffect);
        }
    }
}
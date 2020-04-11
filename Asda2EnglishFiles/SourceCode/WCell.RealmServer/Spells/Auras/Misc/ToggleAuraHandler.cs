using NLog;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Misc
{
    /// <summary>
    /// Applies another aura while active and removes it when turning inactive
    /// </summary>
    public class ToggleAuraHandler : AuraEffectHandler
    {
        private Aura activeToggleAura;

        public Spell ToggleAuraSpell { get; set; }

        public ToggleAuraHandler()
        {
        }

        public ToggleAuraHandler(SpellId auraId)
        {
            this.ToggleAuraSpell = SpellHandler.Get(auraId);
        }

        protected override void Apply()
        {
            if (this.ToggleAuraSpell == null)
                this.ToggleAuraSpell = this.m_spellEffect.TriggerSpell;
            this.activeToggleAura = this.Owner.Auras[this.ToggleAuraSpell];
            if (this.activeToggleAura == null)
            {
                this.activeToggleAura = this.Owner.Auras.CreateAndStartAura(this.m_aura.CasterReference,
                    this.ToggleAuraSpell, true, (Item) null);
                this.activeToggleAura.CanBeSaved = false;
            }
            else
            {
                LogManager.GetCurrentClassLogger().Warn("Tried to toggle on already created Aura \"{0}\" on {1}",
                    (object) this.activeToggleAura, (object) this.Owner);
                this.activeToggleAura.IsActivated = true;
            }
        }

        protected override void Remove(bool cancelled)
        {
            if (this.activeToggleAura == null)
                return;
            this.activeToggleAura.Cancel();
            this.activeToggleAura = (Aura) null;
        }
    }
}
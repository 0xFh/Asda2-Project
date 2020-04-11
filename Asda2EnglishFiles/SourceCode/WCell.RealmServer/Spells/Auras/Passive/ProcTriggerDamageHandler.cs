using NLog;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Does school damage to its targets</summary>
    public class ProcTriggerDamageHandler : AuraEffectHandler
    {
        public override void OnProc(Unit triggerer, IUnitAction action)
        {
            int dmg = this.m_spellEffect.CalcEffectValue(this.m_aura.CasterReference);
            if (this.Owner.MayAttack((IFactionMember) triggerer))
                this.Owner.DealSpellDamage(triggerer, this.m_spellEffect, dmg, true, true, false, true);
            else
                LogManager.GetCurrentClassLogger()
                    .Warn(
                        "Invalid damage effect on Spell {0} was triggered by {1} who cannot be attacked by Aura-Owner {2}.",
                        (object) this.m_aura.Spell, (object) triggerer, (object) this.Owner);
        }
    }
}
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Regenerates a percentage of your total Mana every tick
    /// </summary>
    public class RegenPercentOfTotalManaHandler : AuraEffectHandler
    {
        protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference,
            Unit target, ref SpellFailedReason failReason)
        {
        }

        protected override void Apply()
        {
            this.Owner.Energize((this.EffectValue * this.Owner.MaxPower + 50) / 100, this.m_aura.CasterUnit,
                this.m_spellEffect);
        }
    }
}
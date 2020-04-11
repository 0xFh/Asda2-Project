using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    /// <summary>
    /// Forces the wearer to only attack the caster while the Aura is applied
    /// </summary>
    public class ModTauntAuraHandler : AuraEffectHandler
    {
        protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterRef, Unit target,
            ref SpellFailedReason failReason)
        {
            if (!(target is NPC))
                failReason = SpellFailedReason.BadTargets;
            if (casterRef == null || !(casterRef.Object is Unit))
                return;
            Unit unit = (Unit) casterRef.Object;
            Spell spell = this.m_aura.Spell;
            if (!spell.HasBeneficialEffects || spell.IsAreaSpell || !spell.HasTargets || unit.Target == null ||
                (!unit.IsFriendlyWith((IFactionMember) unit.Target) || target.Target == unit.Target))
                return;
            failReason = SpellFailedReason.NoValidTargets;
        }

        protected override void Apply()
        {
            NPC owner = (NPC) this.Owner;
            Unit casterUnit = this.m_aura.CasterUnit;
            if (casterUnit == null)
                return;
            owner.ThreatCollection.Taunter = casterUnit;
        }

        protected override void Remove(bool cancelled)
        {
            NPC owner = (NPC) this.Owner;
            if (owner.ThreatCollection.Taunter != this.m_aura.CasterUnit)
                return;
            owner.ThreatCollection.Taunter = (Unit) null;
        }
    }
}